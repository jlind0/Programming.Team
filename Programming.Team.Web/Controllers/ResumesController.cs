using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Programming.Team.Business.Core;
using Programming.Team.Core;
using RS = Microsoft.AspNetCore.Http.Results;

namespace Programming.Team.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class ResumesController : ControllerBase
    {
        protected IResumeBlob ResumeBlob { get; }
        protected IBusinessRepositoryFacade<Posting, Guid> Facade { get; }
        public ResumesController(IResumeBlob resumeBlob, IBusinessRepositoryFacade<Posting, Guid> facade)
        {
            ResumeBlob = resumeBlob;
            Facade = facade;
        }
        [HttpGet("{postingId}")]
        public async Task<IResult> GetThumbnail(Guid postingId , CancellationToken token = default)
        {
            return RS.Bytes(await ResumeBlob.GetResume(postingId, token) ?? throw new InvalidDataException(), "application/pdf");
        }
        [HttpGet("summary/{postingId}")]
        public async Task<IResult> GetSummary(Guid postingId, CancellationToken token = default)
        {
            return RS.Bytes(await ResumeBlob.GetResumeSummary(postingId, token) ?? throw new InvalidDataException(), "application/pdf");
        }
        [HttpGet("{postingId}.txt")]
        public async Task<IActionResult> GetMarkdown(Guid postingId , CancellationToken token = default)
        {
            try
            {
                var posting = await Facade.GetByID(postingId, token: token);
                if (posting?.ResumeMarkdown == null)
                    return BadRequest();
                posting.ResumeMarkdown = posting.ResumeMarkdown.Replace("\\item", "--").Replace("\\begin{itemize}", "").Replace("\\end{itemize}", "").Replace("<", "").Replace(">", "").Replace("{", "").Replace("}", "").Replace("\\", "").Replace("[", "").Replace("]", "").Replace("\"", "'").Replace("&", "and");
                return Content(posting.ResumeMarkdown, "text/plain; charset=utf-8", System.Text.Encoding.UTF8);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }
        [HttpGet("{postingId}.tex")]
        public async Task<IActionResult> GetLaTeX(Guid postingId, CancellationToken token = default)
        {
            try
            {
                var posting = await Facade.GetByID(postingId, token: token);
                if (posting?.RenderedLaTex == null)
                    return BadRequest();
                return Content(posting.RenderedLaTex, "application/x-tex", System.Text.Encoding.UTF8);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }
        [HttpGet("summary/{postingId}.tex")]
        public async Task<IActionResult> GetSummaryLaTeX(Guid postingId, CancellationToken token = default)
        {
            try
            {
                var posting = await Facade.GetByID(postingId, token: token);
                if (posting?.ResumeSummaryLatex == null)
                    return BadRequest();
                return Content(posting.ResumeSummaryLatex, "application/x-tex", System.Text.Encoding.UTF8);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }
    }
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class CoverLettersController : ControllerBase
    {
        protected IResumeBlob ResumeBlob { get; }
        protected IBusinessRepositoryFacade<Posting, Guid> Facade { get; }
        public CoverLettersController(IResumeBlob resumeBlob, IBusinessRepositoryFacade<Posting, Guid> facade)
        {
            ResumeBlob = resumeBlob;
            Facade = facade;
        }
        [HttpGet("{postingId}")]
        public async Task<IResult> GetThumbnail(Guid postingId, CancellationToken token = default)
        {
            return RS.Bytes(await ResumeBlob.GetCoverLetter(postingId, token) ?? throw new InvalidDataException(), "application/pdf");
        }
        [HttpGet("{postingId}.tex")]
        public async Task<IActionResult> GetLaTeX(Guid postingId, CancellationToken token = default)
        {
            try
            {
                var posting = await Facade.GetByID(postingId, token: token);
                if (posting?.CoverLetterLaTeX == null)
                    return BadRequest();
                return Content(posting.CoverLetterLaTeX, "application/x-tex", System.Text.Encoding.UTF8);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }
    }
}
