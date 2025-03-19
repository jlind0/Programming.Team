using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Security.Claims;

namespace Programming.Team.Data.Tests;

[TestClass]
public class ContextFactoryTests
{
    private Mock<IServiceProvider> _serviceProviderMock;
    private Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private Mock<AuthenticationStateProvider> _authStateProviderMock;
    private DbContextOptions<ResumesContext> _dbContextOptions;

    [TestInitialize]
    public void Setup()
    {
        _serviceProviderMock = new Mock<IServiceProvider>();
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _authStateProviderMock = new Mock<AuthenticationStateProvider>();
        _dbContextOptions = new DbContextOptionsBuilder<ResumesContext>()
            .UseInMemoryDatabase("TestDatabase")
            .Options;
    }

    [TestMethod]
    public void CreateContext_ShouldReturnResumesContextInstance()
    {
        // Arrange
        var factory = new ContextFactory(_dbContextOptions, _serviceProviderMock.Object);

        // Act
        var context = factory.CreateContext();

        // Assert
        Assert.IsNotNull(context);
        Assert.IsInstanceOfType(context, typeof(ResumesContext));
    }

    [TestMethod]
    public async Task GetPrincipal_ShouldReturnUserFromAuthenticationStateProvider()
    {
        // Arrange
        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "test-user") }));
        var authState = new AuthenticationState(claimsPrincipal);

        _authStateProviderMock
            .Setup(a => a.GetAuthenticationStateAsync())
            .ReturnsAsync(authState);

        _serviceProviderMock
            .Setup(s => s.GetService(typeof(AuthenticationStateProvider)))
            .Returns(_authStateProviderMock.Object);

        var factory = new ContextFactory(_dbContextOptions, _serviceProviderMock.Object);

        // Act
        var result = await factory.GetPrincipal();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("test-user", result.Identity.Name);
    }

    [TestMethod]
    public async Task GetPrincipal_ShouldReturnUserFromHttpContextAccessor_WhenAuthenticationStateProviderIsNull()
    {
        // Arrange
        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "http-user") }));

        var httpContextMock = new Mock<HttpContext>();
        httpContextMock.Setup(h => h.User).Returns(claimsPrincipal);

        _httpContextAccessorMock.Setup(h => h.HttpContext).Returns(httpContextMock.Object);
        _serviceProviderMock
            .Setup(s => s.GetService(typeof(AuthenticationStateProvider)))
            .Returns(null);
        _serviceProviderMock
            .Setup(s => s.GetService(typeof(IHttpContextAccessor)))
            .Returns(_httpContextAccessorMock.Object);

        var factory = new ContextFactory(_dbContextOptions, _serviceProviderMock.Object);

        // Act
        var result = await factory.GetPrincipal();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("http-user", result.Identity.Name);
    }

    [TestMethod]
    public async Task GetImpersonatedUser_ShouldReturnUserId_FromSession()
    {
        // Arrange
        var expectedUserId = Guid.NewGuid();
        var sessionMock = new Mock<ISession>();
        sessionMock.Setup(s => s.TryGetValue("ImpersonatedUserId", out It.Ref<byte[]>.IsAny)).Callback((string key, out byte[] value) =>
        {
            value = System.Text.Encoding.UTF8.GetBytes(expectedUserId.ToString());
        }).Returns(true);

        var httpContextMock = new Mock<HttpContext>();
        httpContextMock.Setup(h => h.Session).Returns(sessionMock.Object);

        _httpContextAccessorMock.Setup(h => h.HttpContext).Returns(httpContextMock.Object);
        _serviceProviderMock.Setup(s => s.GetService(typeof(IHttpContextAccessor))).Returns(_httpContextAccessorMock.Object);

        var factory = new ContextFactory(_dbContextOptions, _serviceProviderMock.Object);

        // Act
        var result = await factory.GetImpersonatedUser();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(expectedUserId, result.Value);
    }

    [TestMethod]
    public async Task SetImpersonatedUser_ShouldStoreUserIdInSession()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var sessionMock = new Mock<ISession>();

        var httpContextMock = new Mock<HttpContext>();
        httpContextMock.Setup(h => h.Session).Returns(sessionMock.Object);

        _httpContextAccessorMock.Setup(h => h.HttpContext).Returns(httpContextMock.Object);
        _serviceProviderMock.Setup(s => s.GetService(typeof(IHttpContextAccessor))).Returns(_httpContextAccessorMock.Object);

        var factory = new ContextFactory(_dbContextOptions, _serviceProviderMock.Object);

        // Act
        await factory.SetImpersonatedUser(userId);

        // Assert
        sessionMock.Verify(s => s.Set("ImpersonatedUserId", System.Text.Encoding.UTF8.GetBytes(userId.ToString())), Times.Once);
    }

    [TestMethod]
    public async Task SetImpersonatedUser_ShouldRemoveUserId_WhenNull()
    {
        // Arrange
        var sessionMock = new Mock<ISession>();

        var httpContextMock = new Mock<HttpContext>();
        httpContextMock.Setup(h => h.Session).Returns(sessionMock.Object);

        _httpContextAccessorMock.Setup(h => h.HttpContext).Returns(httpContextMock.Object);
        _serviceProviderMock.Setup(s => s.GetService(typeof(IHttpContextAccessor))).Returns(_httpContextAccessorMock.Object);

        var factory = new ContextFactory(_dbContextOptions, _serviceProviderMock.Object);

        // Act
        await factory.SetImpersonatedUser(null);

        // Assert
        sessionMock.Verify(s => s.Remove("ImpersonatedUserId"), Times.Once);
    }
}
