using Microsoft.Extensions.Logging;
using Moq;
using Programming.Team.Core;
using Programming.Team.Data.Core;
using Programming.Team.PurchaseManager.Core;

namespace Programming.Team.Business.Tests;

[TestClass]
public class PackageBusinessFacadeTests
{
    [TestMethod]
    public async Task PackageBusinessFacadeTest_Add()
    {
        // Arrange
        var logger = new Mock<ILogger<Package>>();
        var repository = new Mock<IRepository<Package, Guid>>();
        var purchaseManager = new Mock<IPurchaseManager>();
        var packageBusinessFacade = new PackageBusinessFacade(purchaseManager.Object, repository.Object, logger.Object);
        var token = new CancellationToken();
        var package = new Package();
        // Act
        await packageBusinessFacade.Add(package, token: token);
        // Assert
        purchaseManager.Verify(p => p.ConfigurePackage(package, token), Times.Once());
        repository.Verify(r => r.Add(package, It.IsAny<IUnitOfWork>(), token), Times.Once());
    }
    [TestMethod]
    public async Task PackageBusinessFacadeTest_Update()
    {
        var logger = new Mock<ILogger<Package>>();
        var repository = new Mock<IRepository<Package, Guid>>();
        var purchaseManager = new Mock<IPurchaseManager>();
        var packageBusinessFacade = new PackageBusinessFacade(purchaseManager.Object, repository.Object, logger.Object);
        var token = new CancellationToken();
        var package = new Package();
        await packageBusinessFacade.Update(package, token: token);
       
        repository.Verify(r => r.Update(package, It.IsAny<IUnitOfWork>(), It.IsAny<Func<IQueryable<Package>, IQueryable<Package>>>(), token), Times.Once());
        purchaseManager.Verify(p => p.ConfigurePackage(It.IsAny<Package>(), token), Times.Once());
    }
}
