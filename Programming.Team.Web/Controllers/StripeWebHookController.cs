using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Programming.Team.Business.Core;
using Programming.Team.Core;
using Programming.Team.PurchaseManager.Core;
using Stripe;
using Stripe.Checkout;

namespace Programming.Team.Web.Controllers
{
    [Route("/api/stripehooks")]
    [ApiController]
    public class StripeWebhookController : Controller
    {
        protected string WebHookSecret { get; }
        protected ILogger Logger { get; }
        protected SessionService SessionService { get; }
        protected IPurchaseManager<Package, Purchase> PurchaseManager { get; }
        protected IPurchaseManager<DocumentTemplate, DocumentTemplatePurchase> DocumentTemplatePurchaseManager { get; }
        protected IBusinessRepositoryFacade<DocumentTemplatePurchase, Guid> DocumentTemplatePurchaseFacade { get; }
        protected IBusinessRepositoryFacade<Purchase, Guid> PurchaseFacade { get; }
        protected IAccountManager AccountManager { get; }
        protected IUserBusinessFacade UserFacade { get; }
        public StripeWebhookController(IConfiguration configuration, IUserBusinessFacade userFacade, IPurchaseManager<Package, Purchase> purchaseManager, IPurchaseManager<DocumentTemplate, DocumentTemplatePurchase> documentTemplatePurchaseManager,
            IBusinessRepositoryFacade<Purchase, Guid> purchaseFacade, IBusinessRepositoryFacade<DocumentTemplatePurchase, Guid> documentTemplatePurchaseFacade,
            ILogger<StripeWebhookController> logger, SessionService sessionService, IAccountManager accountManager)
        {
            WebHookSecret = configuration["Stripe:WebHookKey"] ?? throw new InvalidDataException();
            Logger = logger;
            SessionService = sessionService;
            PurchaseManager = purchaseManager;
            PurchaseFacade = purchaseFacade;
            DocumentTemplatePurchaseManager = documentTemplatePurchaseManager;
            DocumentTemplatePurchaseFacade = documentTemplatePurchaseFacade;
            AccountManager = accountManager;
            UserFacade = userFacade;
        }
        [HttpPost]
        public async Task<IActionResult> Index()
        {
            try
            {
                var str = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
                Logger.LogInformation(str);
                var stripeEvent = EventUtility.ConstructEvent(str, Request.Headers["Stripe-Signature"], WebHookSecret);
                if (stripeEvent.Type == "checkout.session.completed")
                {
                    var session = stripeEvent.Data.Object as Session;
                    
                    if (session == null)
                        throw new InvalidDataException();
                    var options = new SessionGetOptions();
                    options.AddExpand("line_items");

                    // Retrieve the session. If you require line items in the response, you may include them by expanding line_items.
                    var sessionWithLineItems = await SessionService.GetAsync(session.Id, options);
                    if (sessionWithLineItems.Metadata.TryGetValue("Id", out var subscriptionId) &&
                        sessionWithLineItems.Metadata.TryGetValue("PurchaseType", out var purchseType))
                    {
                        if (sessionWithLineItems.AmountTotal != null)
                        {
                            if(purchseType == typeof(DocumentTemplate).Name)
                            {
                                var documentPurchase = await DocumentTemplatePurchaseFacade.GetByID(Guid.Parse(subscriptionId));
                                if (documentPurchase == null)
                                    throw new InvalidDataException();
                                documentPurchase.StripePaymentIntentId = session.PaymentIntentId;
                                await DocumentTemplatePurchaseManager.FinishPurchase(documentPurchase);
                            }
                            else if(purchseType == typeof(Package).Name)
                            {
                                var purchase = await PurchaseFacade.GetByID(Guid.Parse(subscriptionId));
                                if (purchase == null)
                                    throw new InvalidDataException();
                                purchase.StripePaymentIntentId = session.PaymentIntentId;
                                await PurchaseManager.FinishPurchase(purchase);
                            }    
                        }
                    }
                    else
                        throw new InvalidDataException();
                }
                else if(stripeEvent.Type == "charge.refunded")
                {
                    var payment = stripeEvent.Data.Object as Charge;
                    if(payment == null)
                        throw new InvalidDataException();
                    var purchases = await PurchaseFacade.Get(filter: x => x.StripePaymentIntentId == payment.PaymentIntentId);
                    if(purchases.Count == 0)
                    {
                        var documentTemplatePurchases = await DocumentTemplatePurchaseFacade.Get(filter: x => x.StripePaymentIntentId == payment.PaymentIntentId);
                        if(documentTemplatePurchases.Count == 0)
                            throw new InvalidDataException("No purchases found for the refunded payment intent.");
                        await DocumentTemplatePurchaseManager.RefundPurchase(documentTemplatePurchases.Entities.Single());
                    }
                    else
                    {
                        await PurchaseManager.RefundPurchase(purchases.Entities.Single());
                    }

                }
                return Ok();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                return BadRequest(ex);
            }
        }
    }
    [Route("/api/stripeconnected")]
    [ApiController]
    public class ConnectedStripeWebhookController : Controller
    {
        protected string WebHookSecret { get; }
        protected ILogger Logger { get; }
        protected SessionService SessionService { get; }
        protected IPurchaseManager<Package, Purchase> PurchaseManager { get; }
        protected IPurchaseManager<DocumentTemplate, DocumentTemplatePurchase> DocumentTemplatePurchaseManager { get; }
        protected IBusinessRepositoryFacade<DocumentTemplatePurchase, Guid> DocumentTemplatePurchaseFacade { get; }
        protected IBusinessRepositoryFacade<Purchase, Guid> PurchaseFacade { get; }
        protected IAccountManager AccountManager { get; }
        protected IUserBusinessFacade UserFacade { get; }
        public ConnectedStripeWebhookController(IConfiguration configuration, IUserBusinessFacade userFacade, IPurchaseManager<Package, Purchase> purchaseManager, IPurchaseManager<DocumentTemplate, DocumentTemplatePurchase> documentTemplatePurchaseManager,
            IBusinessRepositoryFacade<Purchase, Guid> purchaseFacade, IBusinessRepositoryFacade<DocumentTemplatePurchase, Guid> documentTemplatePurchaseFacade,
            ILogger<StripeWebhookController> logger, SessionService sessionService, IAccountManager accountManager)
        {
            WebHookSecret = configuration["Stripe:ConnectedWebHookKey"] ?? throw new InvalidDataException();
            Logger = logger;
            SessionService = sessionService;
            PurchaseManager = purchaseManager;
            PurchaseFacade = purchaseFacade;
            DocumentTemplatePurchaseManager = documentTemplatePurchaseManager;
            DocumentTemplatePurchaseFacade = documentTemplatePurchaseFacade;
            AccountManager = accountManager;
            UserFacade = userFacade;
        }
        [HttpPost]
        public async Task<IActionResult> Index()
        {
            try
            {
                var str = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
                Logger.LogInformation(str);
                var stripeEvent = EventUtility.ConstructEvent(str, Request.Headers["Stripe-Signature"], WebHookSecret);
                if (stripeEvent.Type == "account.application.authorized" || stripeEvent.Type == "account.application.deauthorized" ||
                    stripeEvent.Type == "account.updated" || stripeEvent.Type == "account.external_account.created" ||
                    stripeEvent.Type == "account.external_account.updated")
                {
                    var acct = stripeEvent.Account;
                    
                    if (acct == null)
                        throw new InvalidDataException("Account not found in event data.");
                    var users = await UserFacade.Get(filter: x => x.StripeAccountId == acct);
                    if (users.Count != 1)
                        throw new InvalidDataException("No users found for the authorized account.");
                    await AccountManager.FinalizeAccount(users.Entities.Single());
                }
                return Ok();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                return BadRequest(ex);
            }
        }
    }
}
