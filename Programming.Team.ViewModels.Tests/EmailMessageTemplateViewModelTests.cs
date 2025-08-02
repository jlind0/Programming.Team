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
using Programming.Team.Messaging.Core;
using Programming.Team.Templating.Core;
using ReactiveUI;
using Programming.Team.ViewModels.Resume;

namespace Programming.Team.ViewModels.Tests;

/*
Summary:
This test class verifies the correct construction and behavior of the EmailMessageTemplateViewModel, focusing on dependency injection, property initialization, and interaction with SelectUsersViewModel. It ensures that dependencies are properly mocked, initial values are set, and that commands and properties behave as expected. The Fetch command test confirms that the userFacade is called when fetching users.
*/

[TestClass]
public class EmailMessageTemplateViewModelTests
{
    // Dependency mocks for the EmailMessageTemplateViewModel
    private Mock<IBusinessRepositoryFacade<EmailMessageTemplate, Guid>>? facade;
    private Mock<IUserBusinessFacade>? userFacade;
    private Mock<IEmailMessaging>? emailMassaging;
    private Mock<IDocumentTemplator>? documentTemplator;
    private Mock<ILogger<EmailMessageTemplateViewModel>>? logger;
    private Mock<EmailMessageTemplate>? entity;
    private SelectUsersViewModel? realSelectUsersViewModel;

    // Initializes all required mocks and the SelectUsersViewModel before each test
    [TestInitialize]
    public void Setup()
    {
        facade = new Mock<IBusinessRepositoryFacade<EmailMessageTemplate, Guid>>();
        userFacade = new Mock<IUserBusinessFacade>();
        emailMassaging = new Mock<IEmailMessaging>();
        documentTemplator = new Mock<IDocumentTemplator>();
        var selectLoggerMock = new Mock<ILogger<SelectEntitiesViewModel<Guid, User, UserViewModel, IUserBusinessFacade>>>();
        // Use the same userFacade mock for SelectUsersViewModel
        realSelectUsersViewModel = new SelectUsersViewModel(userFacade.Object, selectLoggerMock.Object);
        logger = new Mock<ILogger<EmailMessageTemplateViewModel>>();
        entity = new Mock<EmailMessageTemplate>();

        // Setup the mock for the facade's Add method to verify correct parameters
        facade.Setup(f => f.Add(
            It.Is<EmailMessageTemplate>(t => t.SubjectTemplate == "Test Subject" && t.MessageTemplate == "Test Message"
                && t.IsHtml == true && t.Name == "Test Name"),
            It.IsAny<IUnitOfWork>(),
            It.IsAny<CancellationToken>()
        )).Returns(Task.FromResult(entity.Object)).Verifiable(Times.Once());

        var vm = CreateViewModel();
        // Set initial values for the ViewModel
        vm.SubjectTemplate = "Test Subject";
        vm.MessageTemplate = "Test Message";
        vm.IsHtml = true;
        vm.Name = "Test Name";
    }

    // Verifies that the SelectUsersViewModel is injected and not null
    [TestMethod]
    public void SelectUsersViewModel_IsInjectedCorrectly()
    {
        var viewModel = CreateViewModel();
        Assert.IsNotNull(viewModel.SelectUsersVM);
    }

    // Verifies that the Selected collection in SelectUsersViewModel is accessible and initially empty
    [TestMethod]
    public void SelectUsersViewModel_SelectedUsers_AreAccessible()
    {
        var viewModel = CreateViewModel();
        Assert.IsNotNull(viewModel.SelectUsersVM.Selected);
        Assert.AreEqual(0, viewModel.SelectUsersVM.Selected.Count);
    }

    // Verifies that the Fetch command in SelectUsersViewModel triggers the userFacade.Get method
    [TestMethod]
    public async Task SelectUsersViewModel_Fetch_IsCalled()
    {
        // Mock the user facade to return some test data
        var testUsers = new List<User> { new User { Id = Guid.NewGuid() } };
        var resultSet = new RepositoryResultSet<Guid, User>
        {
            Entities = testUsers,
            Count = 1,
            Page = 1,
            PageSize = 10
        };

        userFacade!.Setup(f => f.Get(
            It.IsAny<IUnitOfWork?>(),
            It.IsAny<Pager?>(),
            It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(),
            It.IsAny<Func<IQueryable<User>, IOrderedQueryable<User>>>(),
            It.IsAny<Func<IQueryable<User>, IQueryable<User>>>(),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(resultSet);

        var viewModel = CreateViewModel();
        // Simulate fetch with the real SelectUsersViewModel
        var request = new DataGridRequest<Guid, User>();
        var result = await viewModel.SelectUsersVM.Fetch.Execute(request);

        // Verify that the facade was called
        userFacade.Verify(f => f.Get(
            It.IsAny<IUnitOfWork?>(),
            It.IsAny<Pager?>(),
            It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(),
            It.IsAny<Func<IQueryable<User>, IOrderedQueryable<User>>>(),
            It.IsAny<Func<IQueryable<User>, IQueryable<User>>>(),
            It.IsAny<CancellationToken>()
        ), Times.Once());

        Assert.IsNotNull(result);
    }

    // Verifies that the initial values of the ViewModel are correct
    [TestMethod]
    public void ViewModel_InitialValues_AreCorrect()
    {
        var viewModel = CreateViewModel();
        Assert.AreEqual("", viewModel.MessageTemplate);
        Assert.AreEqual("", viewModel.SubjectTemplate);
        Assert.AreEqual(false, viewModel.IsHtml);
        Assert.AreEqual("", viewModel.Name);
    }

    // Verifies that setting the MessageTemplate property updates its value
    [TestMethod]
    public void MessageTemplate_SetValue_UpdatesProperty()
    {
        var viewModel = CreateViewModel();
        viewModel.MessageTemplate = "Test";
        Assert.AreEqual("Test", viewModel.MessageTemplate);
    }

    // Helper method to create the EmailMessageTemplateViewModel with all dependencies
    private EmailMessageTemplateViewModel CreateViewModel()
    {
        return new EmailMessageTemplateViewModel(
            facade!.Object,
            userFacade!.Object,
            emailMassaging!.Object,
            documentTemplator!.Object,
            realSelectUsersViewModel!,
            logger!.Object,
            entity!.Object
        );
    }
}