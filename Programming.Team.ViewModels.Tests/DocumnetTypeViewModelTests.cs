using Moq;
using Programming.Team.Business.Core;
using Programming.Team.Core;
using Microsoft.Extensions.Logging;
using Programming.Team.ViewModels.Admin;
using System.Reactive.Linq;

namespace Programming.Team.ViewModels.Tests;

[TestClass]
public class DocumnetTypeViewModelTests
{
    [TestMethod]
    public async Task DocumnetTypeViewModelTests_AddDocumentTypeViewModel_Init()
    {
        var facade = new Mock<IBusinessRepositoryFacade<DocumentType, int>>();
        var logger = new Mock<ILogger<AddEntityViewModel<int, DocumentType, IBusinessRepositoryFacade<DocumentType, int>>>>();
        var vm = new AddDocumentTypeViewModel(facade.Object, logger.Object);
        await vm.Init.Execute().GetAwaiter();
        Assert.IsFalse(vm.CanAdd);
    }
}
