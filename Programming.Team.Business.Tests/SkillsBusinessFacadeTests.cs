using Microsoft.Extensions.Logging;
using Moq;
using Programming.Team.Core;
using Programming.Team.Data.Core;

namespace Programming.Team.Business.Tests;

[TestClass]
public class SkillsBusinessFacadeTests
{
    [TestMethod]
    public async Task SkillsBusinessFacade_GetSkillsExcludingPosition()
    {
        // Arrange
        var logger = new Mock<ILogger<Skill>>();
        var repository = new Mock<ISkillsRespository>();
        var skillsBusinessFacade = new SkillsBusinessFacade(repository.Object, logger.Object);
        var token = new CancellationToken();
        var positionId = Guid.NewGuid();
        // Act
        await skillsBusinessFacade.GetSkillsExcludingPosition(positionId, token: token);
        // Assert
        repository.Verify(r => r.GetSkillsExcludingPosition(positionId, It.IsAny<IUnitOfWork>(), token), Times.Once());
    }
}
