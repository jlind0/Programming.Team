using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting.Logging;
using Moq;
using Programming.Team.Business.Core;
using Programming.Team.Core;
using Programming.Team.PurchaseManager.Core;
using Programming.Team.ViewModels.Purchase;
using System.Linq.Expressions;
using P = Programming.Team.Core.Purchase;

namespace Programming.Team.ViewModels.Tests;

[TestClass]
public class PurchaseHistoryViewModelTests
{
    private PurchaseHistoryViewModel? viewModel;
    private Mock<IPurchaseManager<Package, P>>? purchaseManager;
    private Mock<IBusinessRepositoryFacade<P, Guid>>? facade;
    private Mock<ILogger<ManageEntityViewModel<Guid, P, IBusinessRepositoryFacade<P, Guid>>>>? logger;
    private PurchaseHistoryViewModel? vm;

	[TestInitialize]
    public void TestInitialize()
    {
        facade = new Mock<IBusinessRepositoryFacade<P, Guid>>();
        logger = new Mock<ILogger<ManageEntityViewModel<Guid, P, IBusinessRepositoryFacade<P, Guid>>>>();

		vm = new PurchaseHistoryViewModel(facade.Object, logger.Object);
	}

	//helper class for protected methods
    private class PurchaseHistoryViewModelHelper : PurchaseHistoryViewModel
    {
        public PurchaseHistoryViewModelHelper(IBusinessRepositoryFacade<P, Guid> facade, ILogger<ManageEntityViewModel<Guid, P, IBusinessRepositoryFacade<P, Guid>>> logger)
            : base(facade, logger)
        {
        }
        public Task<Expression<Func<P, bool>>?> CallGetBaseFilterCondition() => base.GetBaseFilterCondition();
	}

	[TestMethod]
    public void Test_CallGetBaseFilterCondition()
    {
        // Arrange
        var helper = new PurchaseHistoryViewModelHelper(facade!.Object, logger!.Object);
        // Act
        var result = helper.CallGetBaseFilterCondition().Result;
        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result, typeof(Expression<Func<P, bool>>));
	}

    [TestCleanup]
    public void TestCleanup()
    {
        viewModel = null;
        purchaseManager = null;
        facade = null;
        logger = null;
        vm = null;
	}
}
