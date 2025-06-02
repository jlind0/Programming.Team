using Microsoft.Extensions.Logging;
using Moq;
using Programming.Team.Core;
using Programming.Team.Data.Core;

namespace Programming.Team.Business.Tests;

[TestClass]
public class SectionTemplateBusinessFacadeTests
{
    [TestMethod]
    public async Task SectionTemplateBusinessFacade_GetBySection()
    {
        // Arrange
        var logger = new Mock<ILogger<SectionTemplate>>();
        var repository = new Mock<ISectionTemplateRepository>();
        var sectionTemplateBusinessFacade = new SectionTemplateBusinessFacade(repository.Object, logger.Object);
        var token = new CancellationToken();
        var sectionId = ResumePart.Bio;
        // Act
        await sectionTemplateBusinessFacade.GetBySection(sectionId, Guid.NewGuid(), token: token);
        // Assert
        repository.Verify(r => r.GetBySection(sectionId, It.IsAny<Guid>(), It.IsAny<IUnitOfWork>(), token), Times.Once());
    }
    [TestMethod]
    public async Task SectionTemplatebusinessFacade_GetDefaultSection()
    {
        var logger = new Mock<ILogger<SectionTemplate>>();
        var repository = new Mock<ISectionTemplateRepository>();
        var sectionTemplateBusinessFacade = new SectionTemplateBusinessFacade(repository.Object, logger.Object);
        var token = new CancellationToken();
        var sectionId = ResumePart.Bio;
        await sectionTemplateBusinessFacade.GetDefaultSection(sectionId, Guid.NewGuid(), token: token);
        repository.Verify(r => r.GetDefaultSection(sectionId, It.IsAny<Guid>(), It.IsAny<IUnitOfWork>(), token), Times.Once());
    }
}
