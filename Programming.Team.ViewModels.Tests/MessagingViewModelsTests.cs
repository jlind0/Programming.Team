using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Programming.Team.Business.Core;
using Programming.Team.Core;
using Programming.Team.Data.Core;
using Programming.Team.ViewModels.Admin;
using System.Reactive.Linq;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;

namespace Programming.Team.ViewModels.Tests;
[TestClass]
public class MessagingViewModelsTests
{
    private Mock<IBusinessRepositoryFacade<EmailMessageTemplate, Guid>>? facade;
    private Mock<ILogger<AddEntityViewModel<Guid, EmailMessageTemplate, IBusinessRepositoryFacade<EmailMessageTemplate, Guid>>>>? logger;
    private AddEmailMessageTemplateViewModel? vm;

    [TestInitialize]
    public void Setup()
    {
        facade = new Mock<IBusinessRepositoryFacade<EmailMessageTemplate, Guid>>();
        logger = new Mock<ILogger<AddEntityViewModel<Guid, EmailMessageTemplate, IBusinessRepositoryFacade<EmailMessageTemplate, Guid>>>>();
        facade.Setup(f => f.Add(
            It.Is<EmailMessageTemplate>(t => "Some Subject" == t.SubjectTemplate && "Some Body" == t.MessageTemplate 
            && true == t.IsHtml && "Some Name" == t.Name),
            It.IsAny<IUnitOfWork>(),
            It.IsAny<CancellationToken>()
        )).Verifiable(Times.Once());
        vm = new AddEmailMessageTemplateViewModel(facade.Object, logger.Object);
    }

    [TestMethod]
    public async Task AddEmailMessageTemplateViewModel_AddSuccess()
    {
        // Act
        vm!.SubjectTemplate = "Some Subject";
        vm!.MessageTemplate = "Some Body";
        vm!.IsHtml = true;
        vm!.Name = "Some Name";

        Assert.IsTrue(vm.CanAdd);

        var e = await vm.Add.Execute().GetAwaiter();
        // Assert result
        Assert.IsNotNull(e);
        Assert.IsInstanceOfType(e, typeof(EmailMessageTemplate));

        // Assert the properties of the added email message template
        Assert.AreEqual("Some Subject", e.SubjectTemplate);
        Assert.AreEqual("Some Body", e.MessageTemplate);
        Assert.IsTrue(e.IsHtml);
        Assert.AreEqual("Some Name", e.Name);

        // Assert the view model properties are reset
        Assert.AreEqual(string.Empty, vm.SubjectTemplate);
        Assert.AreEqual(string.Empty, vm.MessageTemplate);
        Assert.IsTrue(vm.IsHtml);
        Assert.AreEqual(string.Empty, vm.Name);

        facade!.Verify();
    }

    [TestMethod]
    public void IsHtml_SetValue_RaisesPropertyChanged()
    {
        // Act
        bool eventRaised = false;
        vm!.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(vm.IsHtml))
                eventRaised = true;
        };

        vm.IsHtml = false;

        Assert.IsTrue(eventRaised);
        Assert.AreEqual(false, vm.IsHtml);
    }
}

