using Microsoft.Extensions.Logging;
using Moq;
using Programming.Team.Core;
using Programming.Team.Data.Core;

namespace Programming.Team.Business.Tests;

[TestClass]
public class RoleBusinessFacadeTests
{
    [TestMethod]
    public async Task RoleBusinessRepositoryTest_GetUserIds()
    {
        var logger = new Mock<ILogger<Role>>();
        var repository = new Mock<IRoleRepository>();
        var roleBusinessFacade = new RoleBusinessFacade(repository.Object, logger.Object);
        var token = new CancellationToken();
        var roleId = Guid.NewGuid();
        await roleBusinessFacade.GetUserIds(roleId, token: token);
        repository.Verify(r => r.GetUserIds(roleId, It.IsAny<IUnitOfWork>(), token), Times.Once());
    }
    [TestMethod]
    public async Task RoleBusinessRepositoryTest_SetSelectedUsersToRole()
    {
        var logger = new Mock<ILogger<Role>>();
        var repository = new Mock<IRoleRepository>();
        var roleBusinessFacade = new RoleBusinessFacade(repository.Object, logger.Object);
        var token = new CancellationToken();
        var roleId = Guid.NewGuid();
        var userIds = new Guid[] { Guid.NewGuid() };
        await roleBusinessFacade.SetSelectedUsersToRole(roleId, userIds, token: token);
        repository.Verify(r => r.SetSelectedUsersToRole(roleId, userIds, It.IsAny<IUnitOfWork>(), token), Times.Once());
    }
}
