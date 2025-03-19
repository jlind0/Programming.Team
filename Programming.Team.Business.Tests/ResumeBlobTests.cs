using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Moq;

namespace Programming.Team.Business.Tests;

[TestClass]
public class ResumeBlobTests
{
    [TestMethod]
    public async Task ResumeBlobTests_UploadResume()
    {
        var client = new Mock<BlobServiceClient>();
        var blobContainerClient = new Mock<BlobContainerClient>();
        var blobClient = new Mock<BlobClient>();
        blobClient.Setup(c => c.UploadAsync(It.IsAny<Stream>(), It.IsAny<bool>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Mock<Response<BlobContentInfo>>().Object);
        blobContainerClient.Setup(c => c.GetBlobClient(It.IsAny<string>())).Returns(blobClient.Object);
        client.Setup(c => c.GetBlobContainerClient(It.IsAny<string>())).Returns(blobContainerClient.Object);
        ResumeBlob resumeBlob = new ResumeBlob(client.Object);
        var token = new CancellationToken();
        await resumeBlob.UploadResume(Guid.NewGuid(), [0x00], token);
        blobClient.Verify(c => c.UploadAsync(It.IsAny<Stream>(), It.IsAny<bool>(), token), Times.Once());
    }
    [TestMethod]
    public async Task ResumeBlobTests_GetResume()
    {
        var client = new Mock<BlobServiceClient>();
        var blobContainerClient = new Mock<BlobContainerClient>();
        var blobClient = new Mock<BlobClient>();

        // Create real BlobDownloadResult instance
        var binaryData = new BinaryData([0x00]);
        var blobDownloadResult = BlobsModelFactory.BlobDownloadResult(binaryData);
        var blobDownloadInfo = Mock.Of<Response<BlobDownloadResult>>(r => r.Value == blobDownloadResult);

        // Setup the mock behavior
        blobClient.Setup(c => c.DownloadContentAsync(It.IsAny<CancellationToken>())).ReturnsAsync(blobDownloadInfo);
        blobContainerClient.Setup(c => c.GetBlobClient(It.IsAny<string>())).Returns(blobClient.Object);
        client.Setup(c => c.GetBlobContainerClient(It.IsAny<string>())).Returns(blobContainerClient.Object);

        ResumeBlob resumeBlob = new ResumeBlob(client.Object);
        var token = new CancellationToken();
        var result = await resumeBlob.GetResume(Guid.NewGuid(), token);
        Assert.IsNotNull(result);
        // Verify that DownloadContentAsync was called
        blobClient.Verify(c => c.DownloadContentAsync(It.IsAny<CancellationToken>()), Times.Once());

    }
}
