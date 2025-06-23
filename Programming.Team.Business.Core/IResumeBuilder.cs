using Programming.Team.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Programming.Team.Business.Core
{
    /// <summary>
    /// Builds resumes and postings
    /// </summary>
    public interface IResumeBuilder
    {
        /// <summary>
        /// Builds a resume for a user
        /// </summary>
        /// <param name="userId">Target User.Id</param>
        /// <param name="progress">Optional progress indicator</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>A Resume</returns>
        Task<Resume> BuildResume(Guid userId, IProgress<string>? progress = null, CancellationToken token = default);
        /// <summary>
        /// Builds a posting for a user
        /// </summary>
        /// <param name="userId">Target User.Id</param>
        /// <param name="documentTemplateId">Target DocumentTempate.Id</param>
        /// <param name="name">Name of position</param>
        /// <param name="positionText">Job Description</param>
        /// <param name="resume">A Built Resume object</param>
        /// <param name="progress">Optional progress indicator</param>
        /// <param name="config">Optional configuration for the build</param>
        /// <param name="token">Cancellation Token</param>
        /// <returns>a Job Posting</returns>
        Task<Posting> BuildPosting(Guid userId, Guid documentTemplateId, string name, string positionText, Resume resume, IProgress<string>? progress = null, ResumeConfiguration? config = null, CancellationToken token = default);
        /// <summary>
        /// Rebuilds a posting with a new resume
        /// </summary>
        /// <param name="posting">The existing posting</param>
        /// <param name="resume">The target Resume</param>
        /// <param name="enrich">Indicates AI enhancements to be used (will NOT count a resume generation)</param>
        /// <param name="renderPDF">Indicates if the PDF should be rendered</param>
        /// <param name="progress">An optional progress indicator</param>
        /// <param name="config">An optional configuration</param>
        /// <param name="token">An optional cancellation token</param>
        /// <returns>The rebuilt Job Posting</returns>
        Task<Posting> RebuildPosting(Posting posting, Resume resume, bool enrich = true, bool renderPDF = true, IProgress<string>? progress = null, ResumeConfiguration? config = null, CancellationToken token = default);
        /// <summary>
        /// Renders a resume to a PDF
        /// </summary>
        /// <param name="posting">The target posting</param>
        /// <param name="token">optional cancellation token</param>
        /// <returns>The work of rendering a resume</returns>
        Task RenderResume(Posting posting, CancellationToken token = default);

        Task BuildCoverLetter(Posting posting, Guid documentTemplateId, IProgress<string>? progress = null, bool renderPDF = true, CancellationToken token = default);
        Task RenderCoverLetter(Posting posting, CancellationToken token = default);
        Task RenderMarkdown(Posting posting, Guid templateId, CancellationToken token = default);
    }
    /// <summary>
    /// Facade to working with storage for Resumes
    /// </summary>
    public interface IResumeBlob
    {
        /// <summary>
        /// Uploads a resume to a posting
        /// </summary>
        /// <param name="postingId">Posting.Id</param>
        /// <param name="pdfData">PDF Bytes</param>
        /// <param name="token">Optional Cancellation Token</param>
        /// <returns>A Task representing the work</returns>
        Task UploadResume(Guid postingId, byte[] pdfData, CancellationToken token = default);
        /// <summary>
        /// Gets a resume for a posting
        /// </summary>
        /// <param name="postingId">Posting.Id</param>
        /// <param name="token">Cancellation Token</param>
        /// <returns>bytes for a resume</returns>
        Task<byte[]?> GetResume(Guid postingId, CancellationToken token = default);
        Task UploadCoverLetter(Guid postingId, byte[] pdfData, CancellationToken token = default);
        Task<byte[]?> GetCoverLetter(Guid postingId, CancellationToken token = default);
    }
}
