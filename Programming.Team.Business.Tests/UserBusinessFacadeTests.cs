using Microsoft.Extensions.Logging;
using Moq;
using Programming.Team.Core;
using Programming.Team.Data.Core;
using System.Linq.Expressions;

namespace Programming.Team.Business.Tests;

[TestClass]
public class UserBusinessFacadeTests
{
    [TestMethod]
    public async Task UserBusinessRepositoryTest_AddRecruiter()
    {
        // Arrange
        var logger = new Mock<ILogger<User>>();
        var repository = new Mock<IUserRepository>();
        var userBusinessFacade = new UserBusinessFacade(repository.Object, logger.Object);
        var token = new CancellationToken();
        var user = new User();
        var userId = Guid.NewGuid();
        var recruiterId = Guid.NewGuid();
        // Act
        await userBusinessFacade.AddRecruiter(userId, recruiterId, token: token);
        // Assert
        repository.Verify(r => r.AddRecruiter(userId, recruiterId, It.IsAny<IUnitOfWork>(), token), Times.Once());
    }
    [TestMethod]
    public async Task UserBusinessRepositoryTest_RemoveRecruiter()
    {
        var logger = new Mock<ILogger<User>>();
        var repository = new Mock<IUserRepository>();
        var userBusinessFacade = new UserBusinessFacade(repository.Object, logger.Object);
        var token = new CancellationToken();
        var user = new User();
        var userId = Guid.NewGuid();
        var recruiterId = Guid.NewGuid();
        await userBusinessFacade.RemoveRecruiter(userId, recruiterId, token: token);
        repository.Verify(r => r.RemoveRecruiter(userId, recruiterId, It.IsAny<IUnitOfWork>(), token), Times.Once());
    }
    [TestMethod]
    public async Task UserBusinessRepositoryTest_Add()
    {
        var user = new User();
        var logger = new Mock<ILogger<User>>();
        var repository = new Mock<IUserRepository>();
        
        var userBusinessFacade = new UserBusinessFacade(repository.Object, logger.Object);
        var token = new CancellationToken();

        await userBusinessFacade.Add(user, token: token);
        
        repository.Verify(r => r.Add(user, It.IsAny<IUnitOfWork>(), token), Times.Once());
    }
    [TestMethod]
    public async Task UserBusinessRepositoryTest_Update()
    {
        var user = new User();
        var logger = new Mock<ILogger<User>>();
        var repository = new Mock<IUserRepository>();
        var userBusinessFacade = new UserBusinessFacade(repository.Object, logger.Object);
        repository.Setup(r => r.GetByID(It.IsAny<Guid>(), It.IsAny<IUnitOfWork>(), It.IsAny<Func<IQueryable<User>, IQueryable<User>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(new User()
        {
            ResumeGenerationsLeft = 1
        });
        var token = new CancellationToken();

        await userBusinessFacade.Update(user, token: token);
        Assert.IsTrue(user.ResumeGenerationsLeft == 1);
        repository.Verify(r => r.Update(user, It.IsAny<IUnitOfWork>(),It.IsAny<Func<IQueryable<User>, IQueryable<User>>>(), token), Times.Once());
    }
    [TestMethod]
    public async Task UserBusinessRepositoryTest_Delete()
    {
        var logger = new Mock<ILogger<User>>();
        var repository = new Mock<IUserRepository>();
        var userBusinessFacade = new UserBusinessFacade(repository.Object, logger.Object);
        var token = new CancellationToken();
        var userId = Guid.NewGuid();
        await userBusinessFacade.Delete(userId, token: token);
        repository.Verify(r => r.Delete(userId, It.IsAny<IUnitOfWork>(), token), Times.Once());
    }
    [TestMethod]
    public async Task UserBusinessRepositoryTest_Count()
    {
        var logger = new Mock<ILogger<User>>();
        var repository = new Mock<IUserRepository>();
        var userBusinessFacade = new UserBusinessFacade(repository.Object, logger.Object);
        var token = new CancellationToken();
        await userBusinessFacade.Count(token: token);
        repository.Verify(r => r.Count(It.IsAny<IUnitOfWork>(), It.IsAny<System.Linq.Expressions.Expression<System.Func<User, bool>>>(), token), Times.Once());
    }
    [TestMethod]
    public async Task UserBusinessRepositoryTest_GetByObjectId()
    {
        var logger = new Mock<ILogger<User>>();
        var repository = new Mock<IUserRepository>();
        var userBusinessFacade = new UserBusinessFacade(repository.Object, logger.Object);
        var token = new CancellationToken();
        var objectId = "123";
        await userBusinessFacade.GetByObjectIdAsync(objectId, token: token);
        repository.Verify(r => r.GetByObjectIdAsync(objectId, It.IsAny<IUnitOfWork>(), It.IsAny<Expression<Func<User, object>>>(), token), Times.Once());
    }
    [TestMethod]
    public async Task UserBusinessRepositoryTest_UtilizeResumeGeneration_Admin()
    {
        var logger = new Mock<ILogger<User>>();
        var repository = new Mock<IUserRepository>();
        var userBusinessFacade = new UserBusinessFacade(repository.Object, logger.Object);
        var token = new CancellationToken();
        var userId = Guid.NewGuid();
        repository.Setup(r => r.UtilizeResumeGeneration(userId, It.IsAny<IUnitOfWork>(), token)).ReturnsAsync(true);
        repository.Setup(r => r.GetByID(It.IsAny<Guid>(), It.IsAny<IUnitOfWork>(), It.IsAny<Func<IQueryable<User>, IQueryable<User>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(new User()
        {
            ResumeGenerationsLeft = 1,
            Roles = new List<Role>()
            {
                new Role()
                {
                    Name = "Admin"
                }
            }
        });
        await userBusinessFacade.UtilizeResumeGeneration(userId, token: token);
        repository.Verify(r => r.UtilizeResumeGeneration(userId, It.IsAny<IUnitOfWork>(), token), Times.Never());
    }
    [TestMethod]
    public async Task UserBusinessRepositoryTest_UtilizeResumeGeneration()
    {
        var logger = new Mock<ILogger<User>>();
        var repository = new Mock<IUserRepository>();
        var userBusinessFacade = new UserBusinessFacade(repository.Object, logger.Object);
        var token = new CancellationToken();
        var userId = Guid.NewGuid();
        repository.Setup(r => r.UtilizeResumeGeneration(userId, It.IsAny<IUnitOfWork>(), token)).ReturnsAsync(true);
        repository.Setup(r => r.GetByID(It.IsAny<Guid>(), It.IsAny<IUnitOfWork>(), It.IsAny<Func<IQueryable<User>, IQueryable<User>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(new User()
        {
            ResumeGenerationsLeft = 1
        });
        await userBusinessFacade.UtilizeResumeGeneration(userId, token: token);
        repository.Verify(r => r.UtilizeResumeGeneration(userId, It.IsAny<IUnitOfWork>(), token), Times.Once());
    }
    [TestMethod]
    public async Task UserBusinessRepositoryTest_Get()
    {
        var logger = new Mock<ILogger<User>>();
        var repository = new Mock<IUserRepository>();
        var userBusinessFacade = new UserBusinessFacade(repository.Object, logger.Object);
        repository.Setup(r => r.Get(It.IsAny<IUnitOfWork>(), null, It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<Func<IQueryable<User>, IOrderedQueryable<User>>>(), It.IsAny<Func<IQueryable<User>, IQueryable<User>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(new RepositoryResultSet<Guid, User>());
        await userBusinessFacade.Get();
        repository.Verify(r => r.Get(It.IsAny<IUnitOfWork>(), null, It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<Func<IQueryable<User>, IOrderedQueryable<User>>>(), It.IsAny<Func<IQueryable<User>, IQueryable<User>>>(), It.IsAny<CancellationToken>()), Times.Once());
    }
    [TestMethod]
    public async Task UserBusinessRepositoryTest_GetByID()
    {
        var logger = new Mock<ILogger<User>>();
        var repository = new Mock<IUserRepository>();
        var userBusinessFacade = new UserBusinessFacade(repository.Object, logger.Object);
        var token = new CancellationToken();
        var userId = Guid.NewGuid();
        await userBusinessFacade.GetByID(userId, token: token);
        repository.Verify(r => r.GetByID(userId, It.IsAny<IUnitOfWork>(), null, token), Times.Once());
    }
}
