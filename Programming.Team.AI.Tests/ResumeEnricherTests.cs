using Microsoft.Extensions.Logging;
using Moq;
using Programming.Team.AI.Core;
using Programming.Team.Core;
using System.Text.Json;

namespace Programming.Team.AI.Tests;

[TestClass]
public class ResumeEnricherTests
{
    [TestMethod]
    public async Task EnrichmentTest()
    {
        // Arrange
        var logger = new Mock<ILogger<ResumeEnricher>>();
        var chatGPT = new Mock<IChatGPT>();
        var strResume = await File.ReadAllTextAsync("resume.json");
        var strPosting = await File.ReadAllTextAsync("posting.json");
        var resume = JsonSerializer.Deserialize<Resume>(strResume) ?? throw new InvalidDataException();
        var posting = JsonSerializer.Deserialize<Posting>(strPosting) ?? throw new InvalidDataException();
        var progress = new Mock<IProgress<string>>();
        var token = new CancellationToken();
        var resumeEnricher = new ResumeEnricher(logger.Object, chatGPT.Object);
        // Act
        await resumeEnricher.EnrichResume(resume, posting, progress.Object, token);
        // Assert
        chatGPT.Verify(c => c.GetRepsonse(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce());
    }
}
