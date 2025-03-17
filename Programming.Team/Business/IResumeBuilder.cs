using Programming.Team.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Programming.Team.Business.Core
{
    public interface IResumeBuilder
    {
        Task<Resume> BuildResume(Guid userId, IProgress<string>? progress = null, CancellationToken token = default);
        Task<Posting> BuildPosting(Guid userId, Guid documentTemplateId, string name, string positionText, Resume resume, IProgress<string>? progress = null, ResumeConfiguration? config = null, CancellationToken token = default);
        Task<Posting> RebuildPosting(Posting posting, Resume resume, bool enrich = true, bool renderPDF = true, IProgress<string>? progress = null, ResumeConfiguration? config = null, CancellationToken token = default);
        Task RenderResume(Posting posting, CancellationToken token = default);
    }
    public interface IResumeBlob
    {
        Task UploadResume(Guid postingId, byte[] pdfData, CancellationToken token = default);
        Task<byte[]?> GetResume(Guid postingId, CancellationToken token = default);
    }
}
