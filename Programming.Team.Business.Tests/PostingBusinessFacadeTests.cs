using Microsoft.Extensions.Logging;
using Moq;
using Programming.Team.Core;
using Programming.Team.Data.Core;

namespace Programming.Team.Business.Tests;

[TestClass]
public class PostingBusinessFacadeTests
{
    [TestMethod]
    public async Task PostingBusinessFacadeTest_Add()
    {
        // Arrange
        var logger = new Mock<ILogger<Posting>>();
        var repository = new Mock<IRepository<Posting, Guid>>();
        var postingBusinessFacade = new PostingBusinessFacade(repository.Object, logger.Object);
        var token = new CancellationToken();
        var posting = new Posting()
        {
            Details = "<p>Details</p>Some other text.",
        };
        // Act
        await postingBusinessFacade.Add(posting, token: token);
        // Assert
        repository.Verify(r => r.Add(posting, It.IsAny<IUnitOfWork>(), token), Times.Once());
        Assert.AreEqual("DetailsSome other text.", posting.Details);
    }
    [TestMethod]
    public async Task PostingBusinessFacadeTest_Update()
    {
        var logger = new Mock<ILogger<Posting>>();
        var repository = new Mock<IRepository<Posting, Guid>>();
        var postingBusinessFacade = new PostingBusinessFacade(repository.Object, logger.Object);
        var token = new CancellationToken();
        var posting = new Posting()
        {
            Details = "<p>Details</p>Some other text.",
        };
        repository.Setup(r => r.Update(posting, It.IsAny<IUnitOfWork>(), It.IsAny<Func<IQueryable<Posting>, IQueryable<Posting>>>(), token)).ReturnsAsync(posting);
        var posting2 = await postingBusinessFacade.Update(posting, token: token);
        repository.Verify(r => r.Update(posting, It.IsAny<IUnitOfWork>(), It.IsAny<Func<IQueryable<Posting>, IQueryable<Posting>>>(), token), Times.Once());
        Assert.AreEqual("DetailsSome other text.", posting2.Details);
    }
}
