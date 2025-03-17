using Azure.Storage.Blobs;
using Programming.Team.Business.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Programming.Team.Business
{
    public class ResumeBlob : IResumeBlob
    {
        protected static class Containers
        {
            public const string resumes = nameof(resumes);
        }
        protected BlobServiceClient Client { get; }
        protected BlobClient GetClient(string container, Guid id)
        {
            var blobClient = Client.GetBlobContainerClient(container);
            return blobClient.GetBlobClient(id.ToString());
        }
        public ResumeBlob(BlobServiceClient client)
        {
            Client = client;
        }
        public async Task<byte[]?> GetResume(Guid postingId, CancellationToken token = default)
        {
            var client = GetClient(Containers.resumes, postingId);
            var blobDownloadInfo = await client.DownloadContentAsync(token);
            using (MemoryStream memoryStream = new MemoryStream())
            {
                await blobDownloadInfo.Value.Content.ToStream().CopyToAsync(memoryStream);
                return memoryStream.ToArray();
            }
        }

        public async Task UploadResume(Guid postingId, byte[] pdfData, CancellationToken token = default)
        {
            var client = GetClient(Containers.resumes, postingId);
            using (var stream = new MemoryStream(pdfData))
            {
                await client.UploadAsync(stream, true, token);
            }
        }
    }
}
