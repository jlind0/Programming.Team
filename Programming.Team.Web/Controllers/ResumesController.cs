using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Programming.Team.Business.Core;
using RS = Microsoft.AspNetCore.Http.Results;

namespace Programming.Team.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class ResumesController : ControllerBase
    {
        protected IResumeBlob ResumeBlob { get; }
        public ResumesController(IResumeBlob resumeBlob)
        {
            ResumeBlob = resumeBlob;
        }
        [HttpGet("{postingId}")]
        public async Task<IResult> GetThumbnail(Guid postingId , CancellationToken token = default)
        {
            return RS.Bytes(await ResumeBlob.GetResume(postingId, token) ?? throw new InvalidDataException(), "application/pdf");
        }
    }
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class CoverLettersController : ControllerBase
    {
        protected IResumeBlob ResumeBlob { get; }
        public CoverLettersController(IResumeBlob resumeBlob)
        {
            ResumeBlob = resumeBlob;
        }
        [HttpGet("{postingId}")]
        public async Task<IResult> GetThumbnail(Guid postingId, CancellationToken token = default)
        {
            return RS.Bytes(await ResumeBlob.GetCoverLetter(postingId, token) ?? throw new InvalidDataException(), "application/pdf");
        }
    }
}
