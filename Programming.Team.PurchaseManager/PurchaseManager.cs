using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Programming.Team.Core;
using Programming.Team.Data.Core;
using Programming.Team.PurchaseManager.Core;
using Stripe;
using Stripe.Checkout;
using Stripe.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Programming.Team.PurchaseManager
{
    public abstract class PurchaseManager<TPurchaseable, TPurchase> : IPurchaseManager<TPurchaseable, TPurchase>
        where TPurchaseable: Entity<Guid>, IStripePurchaseable, new()
        where TPurchase: Entity<Guid>, IStripePurchase, new()
    {
        protected ProductService ProductService { get; }
        protected PriceService PriceService { get; }
        protected PaymentLinkService PaymentLinkService { get; }
        protected SessionService SessionService { get; }
        protected AccountService AccountService { get; }
        protected PayoutService PayoutService { get; }
        protected string PaymentSuccessUri { get; }
        protected IRepository<TPurchaseable, Guid> PurchaseableRepository { get; }
        protected IRepository<TPurchase, Guid> PurchaseRepository { get; }
        protected IUserRepository UserRepository { get; set; }
        public PurchaseManager(IConfiguration config, IUserRepository userRepository, IRepository<TPurchaseable, Guid> packageRepository, IRepository<TPurchase, Guid> purchaseRepository,
            ProductService productService, PriceService priceService, PaymentLinkService paymentLinkService, SessionService sessionService, AccountService accountService, PayoutService payoutService)
        {
            UserRepository = userRepository;
            PurchaseRepository = purchaseRepository;
            PurchaseableRepository = packageRepository;
            ProductService = productService;
            PriceService = priceService;
            PaymentLinkService = paymentLinkService;
            SessionService = sessionService;
            AccountService = accountService;
            PayoutService = payoutService;
            PaymentSuccessUri = config["Stripe:SuccessUrl"] ?? throw new InvalidDataException();

        }
        protected virtual async Task CreateProduct(IStripePurchaseable entity, CancellationToken token = default)
        {
            var prod = new ProductCreateOptions()
            {
                Name = entity.StripeName
            };
            var product = await ProductService.CreateAsync(prod, cancellationToken: token);
            if (product != null)
            {
                entity.StripeProductId = product.Id;
                await CreatePrice(entity, token);
            }
        }
        protected virtual async Task CreatePrice(IStripePurchaseable entity, CancellationToken token = default)
        {
            if (!string.IsNullOrWhiteSpace(entity.StripeProductId))
            {
                var price = new PriceCreateOptions()
                {
                    UnitAmountDecimal = entity.Price * 100,
                    Currency = "usd",
                    Product = entity.StripeProductId
                };
                var p = await PriceService.CreateAsync(price, cancellationToken: token);
                if (p != null)
                {
                    entity.StripePriceId = p.Id;
                    var options = new PaymentLinkCreateOptions();
                    options.LineItems =
                    [
                        new PaymentLinkLineItemOptions()
                        {
                            Price = entity.StripePriceId,
                            Quantity = 1
                        }
                    ];
                    var link = await PaymentLinkService.CreateAsync(options, cancellationToken: token);
                    entity.StripeUrl = link.Id;
                }

            }
        }

        public virtual async Task ConfigurePackage(TPurchaseable purchaseable, CancellationToken token = default)
        {
            var oldPackage = await PurchaseableRepository.GetByID(purchaseable.Id, token: token);
            if (purchaseable.StripeProductId == null)
            {
                await CreateProduct(purchaseable, token);
            }
            if (oldPackage != null && purchaseable.Price != oldPackage.Price)
            {
                await CreatePrice(purchaseable, token);
            }
            if (oldPackage != null)
                await PurchaseableRepository.Update(purchaseable, token: token);
            else
                await PurchaseableRepository.Add(purchaseable, token: token);
        }

        public async virtual Task FinishPurchase(TPurchase purchase, CancellationToken token = default)
        {
            purchase.IsPaid = true;
            await HandleFinalPurchase(purchase, token);
            await PurchaseRepository.Update(purchase, token: token);
        }
        protected abstract Task HandleFinalPurchase(TPurchase purchase, CancellationToken token = default);
        public virtual async Task<TPurchase> StartPurchase(TPurchaseable purchaseable, CancellationToken token = default)
        {
            TPurchase purchase = new TPurchase()
            {
                Id = Guid.NewGuid(),
                PricePaid = purchaseable.Price ?? throw new InvalidDataException(),
                UserId = await PurchaseableRepository.GetCurrentUserId(fetchTrueUserId: true, token: token) ?? throw new InvalidDataException()
            };
            await HydratePurchase(purchase, purchaseable, token);
            var opts = new Stripe.Checkout.SessionCreateOptions()
            {
                Mode = "payment",
                SuccessUrl = PaymentSuccessUri,
                LineItems = new List<SessionLineItemOptions>()
                {
                    new SessionLineItemOptions()
                    {
                        Price = purchaseable.StripePriceId,
                        Quantity = 1
                    }
                },
                Metadata = new Dictionary<string, string>()
                {
                    {"Id", purchase.Id.ToString() },
                    {"PurchaseType", purchaseable.GetType().Name }
                }
            };
            var session = await SessionService.CreateAsync(opts, cancellationToken: token);
            purchase.StripeSessionUrl = session.Url;
            await PurchaseRepository.Add(purchase, token: token);
            return purchase;
        }

        protected abstract Task HydratePurchase(TPurchase purchase, TPurchaseable purchaseable, CancellationToken token = default);
        protected abstract Task ReversePurchase(TPurchase purchase, CancellationToken token = default);
        public virtual async Task RefundPurchase(TPurchase purchase, CancellationToken token = default)
        {
            purchase.IsRefunded = true;
            purchase.RefundDate = DateTime.UtcNow;
            purchase.IsPaid = false;
            await ReversePurchase(purchase, token);
            await PurchaseRepository.Update(purchase, token: token);
        }
    }
    public class PackagePurchaseManager : PurchaseManager<Package, Purchase>
    {
        public PackagePurchaseManager(IConfiguration config, IUserRepository userRepository, IRepository<Package, Guid> packageRepository, IRepository<Purchase, Guid> purchaseRepository, ProductService productService, PriceService priceService, PaymentLinkService paymentLinkService, SessionService sessionService, AccountService accountService, PayoutService payoutService) : base(config, userRepository, packageRepository, purchaseRepository, productService, priceService, paymentLinkService, sessionService, accountService, payoutService)
        {
        }

        protected override async Task HandleFinalPurchase(Purchase purchase, CancellationToken token = default)
        {
            var package = await PurchaseableRepository.GetByID(purchase.PackageId, token: token);
            var user = await UserRepository.GetByID(purchase.UserId, token: token);
            if (user == null || package == null)
                throw new InvalidDataException();
            user.ResumeGenerationsLeft += package.ResumeGenerations;
            await UserRepository.Update(user, token: token);
        }

        protected override Task HydratePurchase(Purchase purchase, Package purchaseable, CancellationToken token = default)
        {
            purchase.PackageId = purchaseable.Id;
            return Task.CompletedTask;
        }

        protected override async Task ReversePurchase(Purchase purchase, CancellationToken token = default)
        {
            var user = await UserRepository.GetByID(purchase.UserId, token: token);
            if (user == null)
                throw new InvalidDataException();
            user.ResumeGenerationsLeft -= purchase.ResumeGenerations;
            await UserRepository.Update(user, token: token);
        }
    }
    public class DocumentTemplatePurchaseManager : PurchaseManager<DocumentTemplate, DocumentTemplatePurchase>
    {
        public DocumentTemplatePurchaseManager(IConfiguration config, IUserRepository userRepository, IRepository<DocumentTemplate, Guid> packageRepository, IRepository<DocumentTemplatePurchase, Guid> purchaseRepository, ProductService productService, PriceService priceService, PaymentLinkService paymentLinkService, SessionService sessionService, AccountService accountService, PayoutService payoutService) : base(config, userRepository, packageRepository, purchaseRepository, productService, priceService, paymentLinkService, sessionService, accountService, payoutService)
        {
        }

        protected override Task HandleFinalPurchase(DocumentTemplatePurchase purchase, CancellationToken token = default)
        {
            return Task.CompletedTask;
        }

        protected override Task HydratePurchase(DocumentTemplatePurchase purchase, DocumentTemplate purchaseable, CancellationToken token = default)
        {
            purchase.DocumentTemplateId = purchaseable.Id;
            return Task.CompletedTask;
        }

        protected override Task ReversePurchase(DocumentTemplatePurchase purchase, CancellationToken token = default)
        {
            return Task.CompletedTask;
        }
    }
}
