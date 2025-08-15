using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Programming.Team.Business.Core;
using Programming.Team.Core;
using Programming.Team.Data.Core;
using Programming.Team.ViewModels.Recruiter;
using ReactiveUI;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Programming.Team.ViewModels.Tests;

[TestClass]
public class ImpersonatorViewModelTests
{
    private ImpersonatorViewModel? vm;
    private Mock<IContextFactory>? factory;
    private Mock<IUserBusinessFacade>? facade;
    private Mock<ILogger<ManageEntityViewModel<Guid, User, IUserBusinessFacade>>>? logger;
    private Mock<DataGridRequest<Guid, User>>? request;
    private User? impersonatedUser;
    private Guid? impersonatedUserId;
    private CancellationToken token;
    private TestNavigationManager? manage;

	[TestInitialize]
	public void Initialize()
	{
        manage = new TestNavigationManager();
		factory = new Mock<IContextFactory>();
		facade = new Mock<IUserBusinessFacade>();
		logger = new Mock<ILogger<ManageEntityViewModel<Guid, User, IUserBusinessFacade>>>();
		request = new Mock<DataGridRequest<Guid, User>>();
		impersonatedUser = new User();
		impersonatedUserId = Guid.NewGuid();
		token = CancellationToken.None;
		vm = new ImpersonatorViewModel(
			manage,
			factory.Object,
			facade.Object,
			logger.Object
		);
	}

    [TestMethod]
    public void TestImpersonatedUserProperty()
    {
        Assert.IsNull(vm!.ImpersonatedUser);
        vm.ImpersonatedUser = impersonatedUser;
        Assert.AreEqual(impersonatedUser, vm.ImpersonatedUser);
    }

	//helper class to test protected methods
	private class ImpersonatorViewModelHelper : ImpersonatorViewModel
    {
        public ImpersonatorViewModelHelper(NavigationManager manage, IContextFactory factory, IUserBusinessFacade facade, ILogger<ManageEntityViewModel<Guid, User, IUserBusinessFacade>> logger)
            : base(manage, factory, facade, logger)
        {
        }
        public Task<Expression<Func<User, bool>>?> CallGetBaseFilterCondition() => base.GetBaseFilterCondition();
        public Task<RepositoryResultSet<Guid, User>?> CallDoFetch(DataGridRequest<Guid, User> request, CancellationToken token) => base.DoFetch(request, token);
        public Task<Guid?> CallDoLoadImpersonation(CancellationToken token) => base.DoLoadImpersonation(token);
        public Task CallDoImpersonate(Guid userId, CancellationToken token) => base.DoImpersonate(userId, token);
        public new Task DoEndImpersonation(CancellationToken token) => base.DoEndImpersonation(token);
    }

	//helper class for abstract mocking
	public class TestNavigationManager : NavigationManager
	{
		public string? LastUri;
		public bool? LastForceLoad;
		public bool? LastReplace;
		public NavigationOptions? LastOptions;

		public TestNavigationManager()
		{
			Initialize("http://localhost/", "http://localhost/");
		}

		protected override void NavigateToCore(string uri, NavigationOptions options)
		{
			LastUri = uri;
			LastOptions = options;
		}
	}

	[TestMethod]
    public void TestConstructor()
    {
        Assert.IsNotNull(vm);
        Assert.IsNotNull(vm.Impersonate);
        Assert.IsNotNull(vm.EndImpersonation);
        Assert.IsNotNull(vm.Load);
    }
    [TestMethod]
    public async Task Impersonate_SetsImpersonatedUserAndNavigates()
    {
        var helper = new ImpersonatorViewModelHelper(manage!, factory!.Object, facade!.Object, logger!.Object);
        var userId = Guid.NewGuid();
        factory.Setup(f => f.SetImpersonatedUser(userId)).Returns(Task.CompletedTask).Verifiable();
        manage!.NavigateTo("/resume/profile", false);

        await helper.CallDoImpersonate(userId, token);

        factory.Verify(f => f.SetImpersonatedUser(userId), Times.Once);
        manage!.NavigateTo("/resume/profile", false);
    }

    [TestCleanup]
    public void Cleanup()
    {
        vm = null;
        factory = null;
        facade = null;
        logger = null;
        request = null;
        impersonatedUser = null;
        impersonatedUserId = null;
        token = CancellationToken.None;
    }
}