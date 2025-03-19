using Microsoft.EntityFrameworkCore;
using Moq;
using Programming.Team.Data.Core;

namespace Programming.Team.Data.Tests;

[TestClass]
public class UnitOfWorkTests
{
    private Mock<IContextFactory> _contextFactoryMock;
    private Mock<DbContext> _dbContextMock;
    private UnitOfWork _unitOfWork;

    [TestInitialize]
    public void Setup()
    {
        _contextFactoryMock = new Mock<IContextFactory>();
        _dbContextMock = new Mock<DbContext>();

        // Mock CreateContext to return the mocked DbContext
        _contextFactoryMock.Setup(cf => cf.CreateContext()).Returns(_dbContextMock.Object);

        _unitOfWork = new UnitOfWork(_contextFactoryMock.Object);
    }

    [TestMethod]
    public async Task Commit_ShouldCallSaveChangesAsync()
    {
        // Act
        await _unitOfWork.Commit();

        // Assert
        _dbContextMock.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task DisposeAsync_ShouldCallDisposeAsync()
    {
        // Act
        await _unitOfWork.DisposeAsync();

        // Assert
        _dbContextMock.Verify(db => db.DisposeAsync(), Times.Once);
    }

    [TestMethod]
    public async Task Rollback_ShouldCallDisposeAsync()
    {
        // Act
        await _unitOfWork.Rollback();

        // Assert
        _dbContextMock.Verify(db => db.DisposeAsync(), Times.Once);
    }
}
