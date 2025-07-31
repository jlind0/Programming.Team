using Microsoft.Extensions.Logging;
using Moq;
using Programming.Team.Business.Core;
using Programming.Team.Core;
using Programming.Team.Data.Core;
using Programming.Team.ViewModels.Admin;
using System.Reactive.Linq;

namespace Programming.Team.ViewModels.Tests;

[TestClass]
public class FAQViewModelsTests
{
    [TestMethod]
    public async Task AddFAQViewModel_AddSuccess() 
    {
        var facade = new Mock<IBusinessRepositoryFacade<FAQ, Guid>>();
        var logger = new Mock<ILogger<AddEntityViewModel<Guid, FAQ, IBusinessRepositoryFacade<FAQ, Guid>>>>();
        facade.Setup(p => p.Add(It.Is<FAQ>(f => "Some Question" == f.Question && "Some Answer" == f.Answer && "1" == f.SortOrder ), It.IsAny<IUnitOfWork>(), It.IsAny<CancellationToken>())).Verifiable(Times.Once());
        var vm = new AddFAQViewModel(facade.Object, logger.Object);
        Assert.IsFalse(vm.CanAdd);
        vm.Question = "Some Question";
        vm.Answer = "Some Answer";
        vm.SortOrder = "1";
        Assert.IsTrue(vm.CanAdd);
        var e = await vm.Add.Execute().GetAwaiter();
        Assert.AreEqual("Some Question", e.Question);
        Assert.AreEqual("Some Answer", e.Answer);
        Assert.AreEqual("1", e.SortOrder);
        
        Assert.AreEqual(string.Empty, vm.Question);
        Assert.AreEqual(string.Empty, vm.Answer);
        Assert.IsNull(vm.SortOrder);
        Assert.IsFalse(vm.CanAdd);
        facade.Verify();
    }
}
