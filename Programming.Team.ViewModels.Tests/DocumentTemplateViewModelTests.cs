/*using DynamicData.Binding;
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
    [TestMethod]
    public async Task DocumentTemplateViewModelTests_DocumentTemplateViewModel_ById()
    {
        var logger = new Mock<ILogger>();
        var facade = new Mock<IBusinessRepositoryFacade<DocumentTemplate, Guid>>();
        facade.Setup(f => f.GetByID(It.IsAny<Guid>(), It.IsAny<IUnitOfWork>(), 
            It.IsAny<Func<IQueryable<DocumentTemplate>, IQueryable<DocumentTemplate>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DocumentTemplate()
            {
                Id = Guid.NewGuid(),
                DocumentTypeId = 1,
                Name = "Some name",
                Template = "Some template"
            }).Verifiable(Times.Once);
        var vm = new DocumentTemplateViewModel(logger.Object, facade.Object, Guid.NewGuid());
        await vm.Load.Execute().GetAwaiter();
        IDocumentTemplate documentTemplate = vm;
        Assert.AreEqual(1, documentTemplate.DocumentTypeId);
        Assert.AreEqual("Some name", documentTemplate.Name);
        Assert.AreEqual("Some template", documentTemplate.Template);
        facade.Verify();
    }
    [TestMethod]
    public async Task DocumentTemplateViewModelTests_DocumentTemplateViewModel_ByEntity()
    {
        var logger = new Mock<ILogger>();
        var facade = new Mock<IBusinessRepositoryFacade<DocumentTemplate, Guid>>();
        var vm = new DocumentTemplateViewModel(logger.Object, facade.Object, new DocumentTemplate()
        {
            Id = Guid.NewGuid(),
            DocumentTypeId = 1,
            Name = "Some name",
            Template = "Some template"
        });
        await vm.Load.Execute().GetAwaiter();
        IDocumentTemplate documentTemplate = vm;
        Assert.AreEqual(1, documentTemplate.DocumentTypeId);
        Assert.AreEqual("Some name", documentTemplate.Name);
        Assert.AreEqual("Some template", documentTemplate.Template);
        facade.Verify();
    }
    [TestMethod]
    public async Task DocumentTemplateViewModelTests_DocumentTemplateViewModel_Update()
    {
        var logger = new Mock<ILogger>();
        var facade = new Mock<IBusinessRepositoryFacade<DocumentTemplate, Guid>>();
        facade.Setup(f => f.Update(It.IsAny<DocumentTemplate>(), It.IsAny<IUnitOfWork>(),
            It.IsAny<Func<IQueryable<DocumentTemplate>, IQueryable<DocumentTemplate>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DocumentTemplate()
            {
                Id = Guid.NewGuid(),
                DocumentTypeId = 1,
                Name = "Some other name",
                Template = "Some template"
            }).Verifiable(Times.Once);
        var vm = new DocumentTemplateViewModel(logger.Object, facade.Object, new DocumentTemplate()
        {
            Id = Guid.NewGuid(),
            DocumentTypeId = 1,
            Name = "Some name",
            Template = "Some template"
        });
        await vm.Load.Execute().GetAwaiter();
        IDocumentTemplate documentTemplate = vm;
        Assert.AreEqual(1, documentTemplate.DocumentTypeId);
        Assert.AreEqual("Some name", documentTemplate.Name);
        Assert.AreEqual("Some template", documentTemplate.Template);
        
        documentTemplate.Name = "Some other name";
        bool updateEventRaised = false;
        vm.Updated += (s, e) =>
        {
            updateEventRaised = true;
        };
        await vm.Update.Execute().GetAwaiter();
        Assert.AreEqual("Some other name", documentTemplate.Name);
        Assert.IsTrue(updateEventRaised);
        facade.Verify();
    }
    [TestMethod]
    public async Task DocumentTemplateViewModelTests_DocumentTemplateViewModel_Delete()
    {
        var logger = new Mock<ILogger>();
        var facade = new Mock<IBusinessRepositoryFacade<DocumentTemplate, Guid>>();
        facade.Setup(f => f.Delete(It.IsAny<Guid>(), It.IsAny<IUnitOfWork>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask).Verifiable(Times.Once);
        var vm = new DocumentTemplateViewModel(logger.Object, facade.Object, new DocumentTemplate()
        {
            Id = Guid.NewGuid(),
            DocumentTypeId = 1,
            Name = "Some name",
            Template = "Some template"
        });
        await vm.Load.Execute().GetAwaiter();
        IDocumentTemplate documentTemplate = vm;
        Assert.AreEqual(1, documentTemplate.DocumentTypeId);
        Assert.AreEqual("Some name", documentTemplate.Name);
        Assert.AreEqual("Some template", documentTemplate.Template);
        bool deleteEventRaised = false;
        vm.Deleted += (s, e) =>
        {
            deleteEventRaised = true;
        };
        await vm.Delete.Execute().GetAwaiter();
        Assert.IsTrue(deleteEventRaised);
        facade.Verify();
    }
    [TestMethod]
    public async Task DocumentTemplatesViewModel_Load()
    {
        var logger = new Mock<ILogger<EntitiesViewModel<Guid, DocumentTemplate, DocumentTemplateViewModel, IBusinessRepositoryFacade<DocumentTemplate, Guid>>>>();
        var addLogger = new Mock<ILogger<AddEntityViewModel<Guid, DocumentTemplate, IBusinessRepositoryFacade<DocumentTemplate, Guid>>>>();
        var facade = new Mock<IBusinessRepositoryFacade<DocumentTemplate, Guid>>();
        var docType1 = new DocumentType()
        {
            Id = 1,
            Name = "Type 1"
        };
        facade.Setup(p => p.Get(It.IsAny<IUnitOfWork>(), null, It.IsAny<Expression<Func<DocumentTemplate, bool>>>(),
            It.IsAny<Func<IQueryable<DocumentTemplate>, IOrderedQueryable<DocumentTemplate>>>(),
            It.IsAny<Func<IQueryable<DocumentTemplate>, IQueryable<DocumentTemplate>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(new RepositoryResultSet<Guid, DocumentTemplate>()
            {
                Count = 3,
                Entities = 
                [
                    new DocumentTemplate() 
                    {
                        DocumentTypeId = 1,
                        DocumentType = docType1,
                        Name = "Template 1",
                        Template = "Some Template",
                    },
                    new DocumentTemplate()
                    {
                        DocumentTypeId = 1,
                        DocumentType = docType1,
                        Name = "Template 2",
                        Template = "Some Template",
                    },
                    new DocumentTemplate()
                    {
                        DocumentTypeId = 1,
                        DocumentType = docType1,
                        Name = "Template 3",
                        Template = "Some Template",
                    }
                ]
            }).Verifiable(Times.Once);
        var docTypesFacade = new Mock<IBusinessRepositoryFacade<DocumentType, int>>();
        
        docTypesFacade.Setup(f => f.Get(It.IsAny<IUnitOfWork>(), null, It.IsAny<Expression<Func<DocumentType, bool>>>(),
           It.IsAny<Func<IQueryable<DocumentType>, IOrderedQueryable<DocumentType>>>(),
           It.IsAny<Func<IQueryable<DocumentType>, IQueryable<DocumentType>>>(),
           It.IsAny<CancellationToken>())).ReturnsAsync(new RepositoryResultSet<int, DocumentType>()
           {
               Count = 2,
               Entities =
               [
                   docType1,
                    new DocumentType()
                    {
                        Id = 2,
                        Name = "Type 2"
                    }
               ]
           }).Verifiable(Times.Once);
        var addVM = new AddDocumentTemplateViewModel(docTypesFacade.Object, facade.Object, addLogger.Object);
        
        var vm = new DocumentTemplatesViewModel(addVM, facade.Object, logger.Object);
        await addVM.Init.Execute().GetAwaiter();
        await vm.Load.Execute().GetAwaiter();
        Assert.AreEqual(3, vm.Entities.Count);
        docTypesFacade.Verify();
        facade.Verify();
    }
}
*/