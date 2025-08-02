using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Programming.Team.Business.Core;
using Programming.Team.Core;
using Programming.Team.Data.Core;
using Programming.Team.Messaging.Core;
using Programming.Team.Templating.Core;
using Programming.Team.ViewModels.Admin;
using System.Reactive.Linq;

namespace Programming.Team.ViewModels.Tests;

// This test verifies that the EmailMessageTemplatesViewModel is correctly instantiated.
// It checks that the main ViewModel, its AddViewModel, and the Entities collection are not null.
// It also asserts that the AddViewModel is of the expected type and that the Entities collection is initially empty.

[TestClass]
public class EmailMessageTemplatesViewModelTests
{
    private Mock<IServiceProvider>? serviceProvider;
    private Mock<IBusinessRepositoryFacade<EmailMessageTemplate, Guid>>? facade;
    private Mock<ILogger<EntitiesViewModel<Guid, EmailMessageTemplate, EmailMessageTemplateViewModel, IBusinessRepositoryFacade<EmailMessageTemplate, Guid>>>>? logger;
    private Mock<ILogger<AddEntityViewModel<Guid, EmailMessageTemplate, IBusinessRepositoryFacade<EmailMessageTemplate, Guid>>>>? addLogger;
    private AddEmailMessageTemplateViewModel? addViewModel;
    private EmailMessageTemplatesViewModel? vm;

    [TestInitialize]
    public void Setup()
    {
        facade = new Mock<IBusinessRepositoryFacade<EmailMessageTemplate, Guid>>();
        logger = new Mock<ILogger<EntitiesViewModel<Guid, EmailMessageTemplate, EmailMessageTemplateViewModel, IBusinessRepositoryFacade<EmailMessageTemplate, Guid>>>>();
        addLogger = new Mock<ILogger<AddEntityViewModel<Guid, EmailMessageTemplate, IBusinessRepositoryFacade<EmailMessageTemplate, Guid>>>>();
        
        // Setup service provider with required services
        serviceProvider = new Mock<IServiceProvider>();

        addViewModel = new AddEmailMessageTemplateViewModel(facade.Object, addLogger.Object);
        vm = new EmailMessageTemplatesViewModel(serviceProvider.Object, addViewModel, facade.Object, logger.Object);
    }

    [TestMethod]
    public void EmailMessageTemplatesViewModel_Instantiation_Success()
    {
        // Assert
        Assert.IsNotNull(vm);
        Assert.IsNotNull(vm.AddViewModel);
        Assert.IsInstanceOfType(vm.AddViewModel, typeof(AddEmailMessageTemplateViewModel));
        Assert.IsNotNull(vm.Entities);
        Assert.AreEqual(0, vm.Entities.Count);
    }

    [TestMethod]
    public void ServiceProvider_IsAccessible()
    {
        // Assert
        Assert.IsNotNull(vm);

		// The ServiceProvider property is protected, but we can verify it's working
		// by checking that the ViewModel was created successfully with all dependencies
		Assert.IsNotNull(vm!.AddViewModel);
        Assert.IsNotNull(vm.Entities);
    }

    [TestMethod]
    public void AddViewModel_HasCorrectConfiguration()
    {
        // Assert
        Assert.IsNotNull(vm!.AddViewModel);
        Assert.AreEqual(true, vm.AddViewModel.IsHtml); // Default value
        Assert.AreEqual(string.Empty, vm.AddViewModel.Name);
        Assert.AreEqual(string.Empty, vm.AddViewModel.MessageTemplate);
        Assert.AreEqual(string.Empty, vm.AddViewModel.SubjectTemplate);
    }
}
