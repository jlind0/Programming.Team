using DynamicData.Binding;
using Microsoft.Extensions.Logging;
using Moq;
using Programming.Team.Business.Core;
using Programming.Team.Core;
using Programming.Team.Data.Core;
using Programming.Team.ViewModels.Admin;
using System.Linq.Expressions;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace Programming.Team.ViewModels.Tests;

[TestClass]
public class DocumentTemplateViewModelTests
{
    [TestMethod]
    public async Task DocumentTemplateViewModelTests_AddDocumentTemplateViewModel_Init()
    {
        var logger = new Mock<ILogger<AddEntityViewModel<Guid, DocumentTemplate, IBusinessRepositoryFacade<DocumentTemplate, Guid>>>>();
        var facade = new Mock<IBusinessRepositoryFacade<DocumentTemplate, Guid>>();
        var documentTypesFacade = new Mock<IBusinessRepositoryFacade<DocumentType, int>>();
        documentTypesFacade.Setup(f => f.Get(It.IsAny<IUnitOfWork>(), null, It.IsAny<Expression<Func<DocumentType, bool>>>(),
            It.IsAny<Func<IQueryable<DocumentType>, IOrderedQueryable<DocumentType>>>(),
            It.IsAny<Func<IQueryable<DocumentType>, IQueryable<DocumentType>>>(),
            It.IsAny<CancellationToken>())).ReturnsAsync(new RepositoryResultSet<int, DocumentType>()
            {
                Count = 2,
                Entities =
                [
                    new DocumentType()
                    {
                        Id = 1,
                        Name = "Type 1"
                    },
                    new DocumentType()
                    {
                        Id = 2,
                        Name = "Type 2"
                    }
                ]
            }).Verifiable(Times.Once);
        var vm = new AddDocumentTemplateViewModel(documentTypesFacade.Object, facade.Object, logger.Object);
        var disposable = new CompositeDisposable();
        int? documentTypeId = null;
        vm.WhenPropertyChanged(p => p.DocumentTypeId).Subscribe(p =>
        {
            documentTypeId = p.Sender.DocumentTypeId;
        }).DisposeWith(disposable);
        try
        {
            await vm.Init.Execute().GetAwaiter();
            Assert.AreEqual(2, vm.DocumentTypes.Count);
        }
        finally
        {
            disposable.Dispose();
        }
        Assert.AreEqual(1, documentTypeId);
        documentTypesFacade.Verify();
    }
    [TestMethod]
    public async Task DocumentTemplateViewModelTests_AddDocumentTemplateViewModel_Add()
    {
        var logger = new Mock<ILogger<AddEntityViewModel<Guid, DocumentTemplate, IBusinessRepositoryFacade<DocumentTemplate, Guid>>>>();
        var facade = new Mock<IBusinessRepositoryFacade<DocumentTemplate, Guid>>();
        bool wascalled = false;
        facade.Setup(p => p.Add(It.IsAny<DocumentTemplate>(), It.IsAny<IUnitOfWork>(), 
            It.IsAny<CancellationToken>())).Callback((DocumentTemplate p, IUnitOfWork uow, CancellationToken t) =>
        {
            p.Id = Guid.NewGuid();
            Assert.AreEqual(1, p.DocumentTypeId);
            Assert.AreEqual("Some name", p.Name);
            Assert.AreEqual("Some template", p.Template);
            wascalled = true;
        }).Returns(Task.CompletedTask).Verifiable(Times.Once);
        var documentTypesFacade = new Mock<IBusinessRepositoryFacade<DocumentType, int>>();
        documentTypesFacade.Setup(f => f.Get(It.IsAny<IUnitOfWork>(), null, It.IsAny<Expression<Func<DocumentType, bool>>>(),
            It.IsAny<Func<IQueryable<DocumentType>, IOrderedQueryable<DocumentType>>>(),
            It.IsAny<Func<IQueryable<DocumentType>, IQueryable<DocumentType>>>(),
            It.IsAny<CancellationToken>())).ReturnsAsync(new RepositoryResultSet<int, DocumentType>()
            {
                Count = 2,
                Entities =
                [
                    new DocumentType()
                    {
                        Id = 1,
                        Name = "Type 1"
                    },
                    new DocumentType()
                    {
                        Id = 2,
                        Name = "Type 2"
                    }
                ]
            }).Verifiable(Times.Once);
        var vm = new AddDocumentTemplateViewModel(documentTypesFacade.Object, facade.Object, logger.Object);
        bool eventRaised = false;
        vm.Added += (s, e) =>
        {
            eventRaised = true;
        };
        await vm.Init.Execute().GetAwaiter();
        IDocumentTemplate documentTemplate = vm;
        Assert.AreEqual(1, documentTemplate.DocumentTypeId);
        Assert.IsFalse(vm.CanAdd);
        documentTemplate.Name = "Some name";
        Assert.IsFalse(vm.CanAdd);
        documentTemplate.Template = "Some template";
        Assert.IsTrue(vm.CanAdd);
        await vm.Add.Execute().GetAwaiter();
        documentTypesFacade.Verify();
        facade.Verify();
        Assert.IsTrue(wascalled);
        Assert.IsTrue(eventRaised);
        Assert.IsFalse(vm.CanAdd);
    }
}
