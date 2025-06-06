using Microsoft.Extensions.Configuration;
using Moq;
using Programming.Team.Core;
using Programming.Team.Data.Core;
using Stripe;
using Stripe.Checkout;
using Programming.Team.PurchaseManager;

namespace Programming.Team.PurchaseManager.Tests;

[TestClass]
public class PurchaseManagerTests
{
    [TestMethod]
    public async Task PruchaseManagerTests_CreateProduct()
    {
        var config = new Mock<IConfiguration>();
        config.Setup(c => c["Stripe:SuccessUrl"]).Returns("Dummy URL");
        var userRepository = new Mock<IUserRepository>();
        var packageRepository = new Mock<IRepository<Package, Guid>>();
        packageRepository.Setup(p => p.GetByID(It.IsAny<Guid>(), It.IsAny<IUnitOfWork>(), 
            It.IsAny<Func<IQueryable<Package>, IQueryable<Package>>>(), 
            It.IsAny<CancellationToken>())).ReturnsAsync(null as Package).Verifiable(Times.Once);
        packageRepository.Setup(p => p.Add(It.IsAny<Package>(), It.IsAny<IUnitOfWork>(),
            It.IsAny<CancellationToken>())).Verifiable(Times.Once);
        var purchaseRepository = new Mock<IRepository<Purchase, Guid>>();
        var productService = new Mock<ProductService>();
        productService.Setup(p => p.CreateAsync(It.IsAny<ProductCreateOptions>(), 
            It.IsAny<RequestOptions>(), It.IsAny<CancellationToken>())).ReturnsAsync(
            new Product() { Id = Guid.NewGuid().ToString()}).Verifiable(Times.Once);
        var priceService = new Mock<PriceService>();
        priceService.Setup(p => p.CreateAsync(It.IsAny<PriceCreateOptions>(),
            It.IsAny<RequestOptions>(), It.IsAny<CancellationToken>())).ReturnsAsync(
            new Price() { Id = Guid.NewGuid().ToString() }).Verifiable(Times.Once);
        var paymentLinkService = new Mock<PaymentLinkService>();
        paymentLinkService.Setup(p => p.CreateAsync(
            It.IsAny<PaymentLinkCreateOptions>(), It.IsAny<RequestOptions>(), 
            It.IsAny<CancellationToken>())).ReturnsAsync(new PaymentLink() { Id = Guid.NewGuid().ToString() });
        var sessionService = new Mock<SessionService>();
        var accountService = new Mock<AccountService>();
        var payoutService = new Mock<PayoutService>();
        var purchaseManager = new PackagePurchaseManager(config.Object, userRepository.Object, 
            packageRepository.Object, purchaseRepository.Object, productService.Object, priceService.Object, 
            paymentLinkService.Object, sessionService.Object, accountService.Object, payoutService.Object);
        var package = new Package() { ResumeGenerations = 1, Price = 10 };
        await purchaseManager.ConfigurePackage(package);
        Assert.IsNotNull(package.StripeProductId);
        Assert.IsNotNull(package.StripePriceId);
        Assert.IsNotNull(package.StripeUrl);
        packageRepository.Verify();
        productService.Verify();
        priceService.Verify();
        paymentLinkService.Verify();

    }
    [TestMethod]
    public async Task PruchaseManagerTests_UpdateProduct()
    {
        var config = new Mock<IConfiguration>();
        config.Setup(c => c["Stripe:SuccessUrl"]).Returns("Dummy URL");
        var userRepository = new Mock<IUserRepository>();
        var packageRepository = new Mock<IRepository<Package, Guid>>();
        packageRepository.Setup(p => p.GetByID(It.IsAny<Guid>(), It.IsAny<IUnitOfWork>(),
            It.IsAny<Func<IQueryable<Package>, IQueryable<Package>>>(),
            It.IsAny<CancellationToken>())).ReturnsAsync(new Package() { 
                Id = Guid.NewGuid(), 
                ResumeGenerations = 1, Price = 5, 
                StripePriceId = Guid.NewGuid().ToString(),
                StripeProductId = Guid.NewGuid().ToString(),
                StripeUrl = Guid.NewGuid().ToString()}).Verifiable(Times.Once);
        packageRepository.Setup(p => p.Update(It.IsAny<Package>(), It.IsAny<IUnitOfWork>(), 
            It.IsAny<Func<IQueryable<Package>, IQueryable<Package>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(
            (Package package, IUnitOfWork uow, Func<IQueryable<Package>, IQueryable<Package>> props, 
            CancellationToken token) => package).Verifiable(Times.Once);
        var purchaseRepository = new Mock<IRepository<Purchase, Guid>>();
        var productService = new Mock<ProductService>();
        productService.Setup(p => p.CreateAsync(It.IsAny<ProductCreateOptions>(),
            It.IsAny<RequestOptions>(), It.IsAny<CancellationToken>())).ReturnsAsync(
            new Product() { Id = Guid.NewGuid().ToString() }).Verifiable(Times.Never);
        var priceService = new Mock<PriceService>();
        priceService.Setup(p => p.CreateAsync(It.IsAny<PriceCreateOptions>(),
            It.IsAny<RequestOptions>(), It.IsAny<CancellationToken>())).ReturnsAsync(
            new Price() { Id = Guid.NewGuid().ToString() }).Verifiable(Times.Once);
        var paymentLinkService = new Mock<PaymentLinkService>();
        paymentLinkService.Setup(p => p.CreateAsync(
            It.IsAny<PaymentLinkCreateOptions>(), It.IsAny<RequestOptions>(),
            It.IsAny<CancellationToken>())).ReturnsAsync(new PaymentLink() { Id = Guid.NewGuid().ToString() });
        var sessionService = new Mock<SessionService>();
        var accountService = new Mock<AccountService>();
        var payoutService = new Mock<PayoutService>();
        var purchaseManager = new PackagePurchaseManager(config.Object, userRepository.Object,
            packageRepository.Object, purchaseRepository.Object, productService.Object, priceService.Object,
            paymentLinkService.Object, sessionService.Object, accountService.Object, payoutService.Object);
        var package = new Package() { ResumeGenerations = 1, Price = 10,
            StripePriceId = Guid.NewGuid().ToString(),
            StripeProductId = Guid.NewGuid().ToString(),
            StripeUrl = Guid.NewGuid().ToString()
        };
        await purchaseManager.ConfigurePackage(package);
        Assert.IsNotNull(package.StripeProductId);
        Assert.IsNotNull(package.StripePriceId);
        Assert.IsNotNull(package.StripeUrl);
        packageRepository.Verify();
        productService.Verify();
        priceService.Verify();
        paymentLinkService.Verify();

    }
    /*[TestMethod]
    public async Task PurchaseManagerTests_StartPurchase()
    {
        var config = new Mock<IConfiguration>();
        config.Setup(c => c["Stripe:SuccessUrl"]).Returns("Dummy URL");
        var userRepository = new Mock<IUserRepository>();
        var packageRepository = new Mock<IRepository<Package, Guid>>();
        packageRepository.Setup(p => p.GetByID(It.IsAny<Guid>(), It.IsAny<IUnitOfWork>(),
            It.IsAny<Func<IQueryable<Package>, IQueryable<Package>>>(),
            It.IsAny<CancellationToken>())).ReturnsAsync(new Package()
            {
                Id = Guid.NewGuid(),
                ResumeGenerations = 1,
                Price = 5,
                StripePriceId = Guid.NewGuid().ToString(),
                StripeProductId = Guid.NewGuid().ToString(),
                StripeUrl = Guid.NewGuid().ToString()
            }).Verifiable(Times.Once);
        packageRepository.Setup(p => p.GetCurrentUserId(It.IsAny<IUnitOfWork>(),It.IsAny<bool>(), It.IsAny<CancellationToken>())
            ).ReturnsAsync(Guid.NewGuid()).Verifiable(Times.Once);
        var purchaseRepository = new Mock<IRepository<Purchase, Guid>>();
        purchaseRepository.Setup(p => p.Add(It.IsAny<Purchase>(), It.IsAny<IUnitOfWork>(), It.IsAny<CancellationToken>()))
            .Verifiable(Times.Once);
        var productService = new Mock<ProductService>();
        var priceService = new Mock<PriceService>();
        var paymentLinkService = new Mock<PaymentLinkService>();
        var sessionService = new Mock<SessionService>();
        sessionService.Setup(s => s.CreateAsync(It.IsAny<SessionCreateOptions>(), It.IsAny<RequestOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Session() { Id = Guid.NewGuid().ToString(), Url = Guid.NewGuid().ToString() }).Verifiable(Times.Once);
        var accountService = new Mock<AccountService>();
        var payoutService = new Mock<PayoutService>();
        var purchaseManager = new PackagePurchaseManager(config.Object, userRepository.Object,
            packageRepository.Object, purchaseRepository.Object, productService.Object, priceService.Object,
            paymentLinkService.Object, sessionService.Object, accountService.Object, payoutService.Object);
        var purchase = await purchaseManager.StartPurchase(new Package());
        Assert.IsNotNull(purchase.StripeSessionUrl);
        packageRepository.Verify();
        sessionService.Verify();
        purchaseRepository.Verify();
    }
    [TestMethod]
    public async Task PurchaseManagerTests_CompletePurchase()
    {
        var config = new Mock<IConfiguration>();
        config.Setup(c => c["Stripe:SuccessUrl"]).Returns("Dummy URL");
        var userRepository = new Mock<IUserRepository>();
        userRepository.Setup(p => p.Update(It.IsAny<User>(), It.IsAny<IUnitOfWork>(), It.IsAny<Func<IQueryable<User>, IQueryable<User>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User user, IUnitOfWork uow, Func<IQueryable<User>, IQueryable<User>> props, CancellationToken token) => user).Verifiable(Times.Once);
        var packageRepository = new Mock<IRepository<Package, Guid>>();
        var purchaseRepository = new Mock<IRepository<Purchase, Guid>>();
        purchaseRepository.Setup(p => p.GetByID(It.IsAny<Guid>(), It.IsAny<IUnitOfWork>(), 
            It.IsAny<Func<IQueryable<Purchase>, IQueryable<Purchase>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Purchase()
            {
                Id = Guid.NewGuid(),
                PackageId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                StripeSessionUrl = Guid.NewGuid().ToString(),
                ResumeGenerations = 1,
                IsPaid = false,
                User = new User()
                {
                    Id = Guid.NewGuid(),
                    ResumeGenerationsLeft = 0
                }
            });
        purchaseRepository.Setup(p => p.Update(It.IsAny<Purchase>(), It.IsAny<IUnitOfWork>(),
            It.IsAny<Func<IQueryable<Purchase>, IQueryable<Purchase>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Purchase purchase, IUnitOfWork uow, Func<IQueryable<Purchase>, IQueryable<Purchase>> props, CancellationToken token) => purchase).Verifiable(Times.Once);
        var productService = new Mock<ProductService>();
        var priceService = new Mock<PriceService>();
        var paymentLinkService = new Mock<PaymentLinkService>();
        var sessionService = new Mock<SessionService>();
        var accountService = new Mock<AccountService>();
        var payoutService = new Mock<PayoutService>();
        var purchaseManager = new PackagePurchaseManager(config.Object, userRepository.Object,
            packageRepository.Object, purchaseRepository.Object, productService.Object, priceService.Object,
            paymentLinkService.Object, sessionService.Object, accountService.Object, payoutService.Object);
        await purchaseManager.FinishPurchase(new Purchase());
        purchaseRepository.Verify();
        userRepository.Verify();
    }*/
}