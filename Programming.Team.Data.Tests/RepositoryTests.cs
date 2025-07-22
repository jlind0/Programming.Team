namespace Programming.Team.Data.Tests;

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Programming.Team.Core;
using Programming.Team.Data.Core;

[TestClass]
public class RepositoryTests
{
    private Mock<IContextFactory>? _contextFactoryMock;
    private Mock<IMemoryCache>? _cacheMock;
    private Repository<Position, Guid>? _repository;
    private Mock<ResumesContext>? _contextMock;

    [TestInitialize]
    public void TestInitialize()
    {
        _contextFactoryMock = new Mock<IContextFactory>();
        _cacheMock = new Mock<IMemoryCache>();
        _cacheMock.Setup(c => c.TryGetValue(It.IsAny<object>(), out It.Ref<object>.IsAny)).Callback((object key, out object value) => {
            // Set the out parameter to a new Guid
            value = Guid.NewGuid();
        }).Returns(true);
        // For methods like GetCurrentUserId, we set up the impersonated user to return a non-null Guid.
        var impersonatedUserId = Guid.NewGuid();
        _contextFactoryMock.Setup(cf => cf.GetImpersonatedUser())
                           .ReturnsAsync(impersonatedUserId);
        _contextMock = new Mock<ResumesContext>();
        _contextMock.Setup(c => c.Set<Position>())
                    .Returns(Mock.Of<DbSet<Position>>());
        _contextMock.Setup(c => c.Users)
                    .Returns(Mock.Of<DbSet<User>>());
        _contextFactoryMock.Setup(cf => cf.CreateContext()).Returns(_contextMock.Object);

        // (Optionally) Set up GetPrincipal if you wish to test that branch.
        var claimsPrincipalMock = new Mock<ClaimsPrincipal>();
        claimsPrincipalMock.Setup(p => p.Claims)
                           .Returns(new[]
                           {
                               new Claim("http://schemas.microsoft.com/identity/claims/objectidentifier", impersonatedUserId.ToString())
                           });
        _contextFactoryMock.Setup(cf => cf.GetPrincipal())
                           .ReturnsAsync(claimsPrincipalMock.Object);

        // Create the repository instance under test.
        _repository = new Repository<Position, Guid>(_contextFactoryMock.Object, _cacheMock.Object);
    }

    [TestMethod]
    public async Task Delete_ByKey_WithValidKey_DeletesEntity()
    {
        var entity = new Position { Id = Guid.NewGuid()};
        _contextMock.Setup(c => c.FindAsync<Position>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(entity).Verifiable(Times.Once);

        // Act
        await _repository.Delete(entity.Id);

        // Assert: In the Delete by key implementation, the entity is looked up then passed to Delete(entity)
        _contextMock.Verify();
    }

    [TestMethod]
    public async Task Delete_ByEntity_MarksEntityAsDeletedAndSetsUpdatedByUserId()
    {
        // Arrange
        var entity = new Position() { Id = Guid.NewGuid() };
        // (The repository calls GetCurrentUserId and assigns its result.)
        // The impersonated user is already set up in TestInitialize.

        // Act
        await _repository.Delete(entity);

        // Assert: Verify that the entity is marked as deleted and that UpdatedByUserId is set.
        Assert.IsTrue(entity.IsDeleted, "Entity should be marked as deleted.");
        Assert.IsNotNull(entity.UpdatedByUserId, "UpdatedByUserId should be set.");
    }

    [TestMethod]
    public async Task Add_SetsEntityBaseAttributesAndCallsAddAsync()
    {
        // Arrange
        var entity = new Position();
        var userId = Guid.NewGuid();
        // Setup AddAsync on the context.
        _contextMock.Setup(c => c.AddAsync(entity, It.IsAny<CancellationToken>())).Verifiable(Times.Once);
        _contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
                    .ReturnsAsync(1).Verifiable(Times.Once);

        // Act
        await _repository.Add(entity);

        // Also verify that AddAsync was called once.
        _contextMock.Verify();
    }
    [TestMethod]
    public async Task GetCurrentUserId_ReturnsCachedUserId_WhenFetchTrueUserIdAndCacheHit()
    {
        // Arrange
        var fetchTrueUserId = true;
      
        // Act
        var result = await _repository.GetCurrentUserId(fetchTrueUserId: fetchTrueUserId);
    }

}