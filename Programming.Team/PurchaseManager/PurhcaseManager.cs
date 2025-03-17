using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Programming.Team.Core;
using Programming.Team.Data.Core;
using Programming.Team.PurchaseManager.Core;
using Stripe;
using Stripe.Checkout;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Programming.Team.PurchaseManager
{
    public class PurhcaseManager : IPurchaseManager
    {
        protected ProductService ProductService { get; }
        protected PriceService PriceService { get; }
        protected PaymentLinkService PaymentLinkService { get; }
        protected SessionService SessionService { get; }
        protected AccountService AccountService { get; }
        protected PayoutService PayoutService { get; }
        protected string PaymentSuccessUri { get; }
        protected IRepository<Package, Guid> PackageRepository { get; }
        protected IRepository<Purchase, Guid> PurchaseRepository { get; }
        protected IUserRepository UserRepository { get; set; }
        public PurhcaseManager(IConfiguration config, IUserRepository userRepository, IRepository<Package, Guid> packageRepository, IRepository<Purchase, Guid> purchaseRepository,
            ProductService productService, PriceService priceService, PaymentLinkService paymentLinkService, SessionService sessionService, AccountService accountService, PayoutService payoutService)
        {
            UserRepository = userRepository;
            PurchaseRepository = purchaseRepository;
            PackageRepository = packageRepository;
            ProductService = productService;
            PriceService = priceService;
            PaymentLinkService = paymentLinkService;
            SessionService = sessionService;
            AccountService = accountService;
            PayoutService = payoutService;
            PaymentSuccessUri = config["Stripe:SuccessUrl"] ?? throw new InvalidDataException();

        }
        protected async Task CreateProduct(Package entity, CancellationToken token = default)
        {
            var prod = new ProductCreateOptions()
            {
                Name = $"{entity.ResumeGenerations} Resume Generations"
            };
            var product = await ProductService.CreateAsync(prod, cancellationToken: token);
            if (product != null)
            {
                entity.StripeProductId = product.Id;
                await CreatePrice(entity, token);
            }
        }
        protected async Task CreatePrice(Package entity, CancellationToken token = default)
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
        public async Task ConfigurePackage(Package package, CancellationToken token = default)
        {
            var oldPackage = await PackageRepository.GetByID(package.Id, token: token);
            if (package.StripeProductId == null)
            {
                await CreateProduct(package, token);
            }
            if (package.Price != oldPackage?.Price)
            { 
                await CreatePrice(package, token);
            }
            if(oldPackage != null)
                await PackageRepository.Update(package, token: token);
            else
                await PackageRepository.Add(package, token: token);
        }
        public async Task<Purchase> StartPurchase(Guid packageId, CancellationToken token = default)
        {
            var package = await PackageRepository.GetByID(packageId, token: token);
            if (package == null)
                throw new InvalidDataException();
            Purchase purchase = new Purchase()
            {
                Id = Guid.NewGuid(),
                PackageId = packageId,
                PricePaid = package.Price,
                ResumeGenerations = package.ResumeGenerations,
                UserId = await PackageRepository.GetCurrentUserId(fetchTrueUserId: true, token: token) ?? throw new InvalidDataException()
            };
            var opts = new Stripe.Checkout.SessionCreateOptions()
            {
                Mode =  "payment",
                SuccessUrl = PaymentSuccessUri,
                LineItems = new List<SessionLineItemOptions>()
                {
                    new SessionLineItemOptions()
                    {
                        Price = package.StripePriceId,
                        Quantity = 1
                    }
                },
                Metadata = new Dictionary<string, string>()
                {
                    {nameof(Purchase.Id), purchase.Id.ToString() }
                }
            };
            var session = await SessionService.CreateAsync(opts, cancellationToken: token);
            purchase.StripeSessionUrl = session.Url;
            await PurchaseRepository.Add(purchase, token: token);
            return purchase;
        }
        public async Task FinishPurchase(Guid purchaseId, CancellationToken token = default)
        {
            var purchase = await PurchaseRepository.GetByID(purchaseId, properites: q => q.Include(e => e.User), token: token);
            if (purchase == null)
                throw new InvalidDataException();
            purchase.IsPaid = true;
            purchase.User.ResumeGenerationsLeft += purchase.ResumeGenerations;
            await UserRepository.Update(purchase.User, token: token);
            await PurchaseRepository.Update(purchase, token: token);
        }
    }
}
