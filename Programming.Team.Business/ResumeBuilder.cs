using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Programming.Team.AI.Core;
using Programming.Team.Business.Core;
using Programming.Team.Core;
using Programming.Team.Templating.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Programming.Team.Business
{
    public class ResumeBuilder : IResumeBuilder
    {
        protected IUserBusinessFacade UserFacade { get; }
        protected IBusinessRepositoryFacade<Position, Guid> PositionFacade { get; }
        protected IBusinessRepositoryFacade<Education, Guid> EducationFacade { get; }
        protected IBusinessRepositoryFacade<Publication, Guid> PublicationFacade { get; }
        protected IBusinessRepositoryFacade<Certificate, Guid> CertificateFacade { get; }
        protected IBusinessRepositoryFacade<DocumentTemplate, Guid> DocumentTemplateFacade { get; }
        protected IBusinessRepositoryFacade<Posting, Guid> PostingFacade { get; }
        protected ISectionTemplateBusinessFacade SectionFacade { get; }
        protected IDocumentTemplator Templator { get; }
        protected IResumeEnricher Enricher { get; }
        protected ILogger Logger { get; }
        protected IResumeBlob ResumeBlob { get; }
        public ResumeBuilder(ILogger<ResumeBuilder> logger, 
            IUserBusinessFacade userFacade,
            IBusinessRepositoryFacade<Position, Guid> positionFacade,
            IBusinessRepositoryFacade<Education, Guid> educationFacade,
            IBusinessRepositoryFacade<Publication, Guid> publicationFacade,
            IBusinessRepositoryFacade<Certificate, Guid> certificateFacade,
            IBusinessRepositoryFacade<DocumentTemplate, Guid> documentTemplateFacade,
            IBusinessRepositoryFacade<Posting, Guid> postingFacade,
            IDocumentTemplator templator,
            IResumeEnricher enricher,
            IResumeBlob resumeBlob,
            ISectionTemplateBusinessFacade sectionFacade
            )
        {
            Logger = logger;
            UserFacade = userFacade;
            PositionFacade = positionFacade;
            EducationFacade = educationFacade;
            PublicationFacade = publicationFacade;
            CertificateFacade = certificateFacade;
            Templator = templator;
            Enricher = enricher;
            DocumentTemplateFacade = documentTemplateFacade;
            PostingFacade = postingFacade;
            ResumeBlob = resumeBlob;
            SectionFacade = sectionFacade;
        }
        public async Task<Resume> BuildResume(Guid userId, IProgress<string>? progress = null, CancellationToken token = default)
        {
            try
            {
                Resume resume = new Resume();
                Dictionary<Guid, SkillRollup> rollups = new Dictionary<Guid, SkillRollup>();
                await using (var uow = UserFacade.CreateUnitOfWork())
                {
                    progress?.Report("Building Resume");
                    resume.User = await UserFacade.GetByID(userId, work: uow, token: token) ?? throw new InvalidDataException();
                    var positions = await PositionFacade.Get(work: uow, properites: GetPositionProperties(), filter: q => q.UserId == userId, 
                        orderBy: e => e.OrderByDescending(c => c.EndDate ?? DateOnly.MaxValue).ThenByDescending(c => c.SortOrder).ThenByDescending(c => c.StartDate),
                        token: token);
                    resume.Positions.AddRange(positions.Entities);
                    var education = await EducationFacade.Get(work: uow, properites: GetEducationProperties(),
                        orderBy: q => q.OrderByDescending(e => e.EndDate ?? DateOnly.MaxValue).ThenByDescending(e => e.StartDate),
                        filter: q => q.UserId == userId, token: token);
                    resume.Educations.AddRange(education.Entities);
                    var publications = await PublicationFacade.Get(work: uow, orderBy: q => q.OrderByDescending(e => e.PublishDate).ThenBy(e => e.Title),
                        filter: q => q.UserId == userId, token: token);
                    resume.Publications.AddRange(publications.Entities);
                    var certs = await CertificateFacade.Get(work: uow, properites: GetCertificateProperties(), orderBy: q => q.OrderByDescending(e => e.ValidToDate ?? DateOnly.MaxValue).ThenByDescending(e => e.ValidFromDate),
                        filter: q => q.UserId == userId, token: token);
                    resume.Certificates.AddRange(certs.Entities);
                    resume.Recommendations = resume.Positions.SelectMany(e => e.Recommendations.Where(r => r.UserId == resume.User.Id)).OrderBy(c => c.SortOrder).ThenBy(c => c.Name).ToList();
                    
                    foreach(var position in resume.Positions)
                    {
                        foreach(var posskill in position.PositionSkills)
                        {
                            if (!rollups.TryGetValue(posskill.SkillId, out var skill))
                            {
                                skill = new SkillRollup()
                                {
                                    Skill = posskill.Skill,
                                    YearsOfExperience = (position.EndDate ?? DateOnly.FromDateTime(DateTime.Today)).ToDateTime(TimeOnly.MinValue).Subtract(position.StartDate.ToDateTime(TimeOnly.MinValue)).TotalDays / 365d
                                };
                                skill.Positions.Add(position);
                            }
                            else
                            {
                                skill.YearsOfExperience += (position.EndDate ?? DateOnly.FromDateTime(DateTime.Today)).ToDateTime(TimeOnly.MinValue).Subtract(position.StartDate.ToDateTime(TimeOnly.MinValue)).TotalDays / 365d;
                                skill.Positions.Add(position);
                            }
                            rollups[posskill.SkillId] = skill;
                        }
                    }
                    resume.Skills = rollups.Values.OrderByDescending(e => e.YearsOfExperience).ToList();
                }
                foreach(var position in resume.Positions)
                {
                    position.PositionSkills = position.PositionSkills.OrderByDescending(e => rollups[e.SkillId].YearsOfExperience).ThenBy(e => e.Skill.Name).ToList();
                }
                return resume;
            }
            catch(Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                throw;
            }
        }
        protected Func<IQueryable<Position>, IQueryable<Position>> GetPositionProperties()
        {
            return e => e.Include(x => x.PositionSkills).ThenInclude(x => x.Skill).Include(x => x.Company).Include(x => x.Recommendations);
        }
        protected Func<IQueryable<Education>, IQueryable<Education>> GetEducationProperties() 
        {
            return e => e.Include(x => x.Institution);
        }
        protected Func<IQueryable<Certificate>, IQueryable<Certificate>> GetCertificateProperties()
        {
            return e => e.Include(x => x.Issuer);
        }
        public async Task<Posting> BuildPosting(Guid userId, Guid documentTemplateId, string name, string positionText, Resume resume, IProgress<string>? progress = null, ResumeConfiguration? config = null, CancellationToken token = default)
        {
            try
            {
                var user = await UserFacade.GetByID(userId, token: token);
                if (user == null)
                    throw new InvalidDataException();
                
                Posting posting = new Posting()
                {
                    UserId = userId,
                    DocumentTemplateId = documentTemplateId,
                    Configuration = config != null ? JsonSerializer.Serialize(config) : null,
                    Details = positionText,
                    Name = name
                };
                await PostingFacade.Add(posting, token: token);
                posting = await RebuildPosting(posting, resume,progress: progress, config: config, token: token);
                return posting;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                throw;
            }
        }

        public async Task<Posting> RebuildPosting(Posting posting, Resume resume, bool enrich = true, bool renderPDF = true, IProgress<string>? progress = null, ResumeConfiguration? config = null, CancellationToken token = default)
        {
            try
            {
                var docTemplate = await DocumentTemplateFacade.GetByID(posting.DocumentTemplateId, token: token);
                if (docTemplate == null)
                    throw new InvalidDataException();
                config ??= new ResumeConfiguration();
                resume.Parts = config.Parts;
                if (enrich && await UserFacade.UtilizeResumeGeneration(
                    await UserFacade.GetCurrentUserId(fetchTrueUserId: true, token: token) ?? throw new InvalidOperationException(), token: token))
                    await Enricher.EnrichResume(resume, posting, progress, token);
                progress?.Report("Preparing Resume Style");
                foreach (var part in (ResumePart[])Enum.GetValues(typeof(ResumePart)))
                {
                    config.SectionTemplates.TryGetValue(part, out var sectionTemplateId);
                    var section = sectionTemplateId != null ? await SectionFacade.GetByID(sectionTemplateId.Value, token: token) : await SectionFacade.GetDefaultSection(part, posting.DocumentTemplateId, token: token);
                    if (section == null) continue;
                    docTemplate.Template = docTemplate.Template.Replace($"%==={Enum.GetName(typeof(ResumePart), part)}===", section.Template);
                }
                posting.RenderedLaTex = await Templator.ApplyTemplate(docTemplate.Template, resume, token);
                posting.ResumeJson = JsonSerializer.Serialize(resume, new JsonSerializerOptions()
                {
                    ReferenceHandler = ReferenceHandler.IgnoreCycles
                });
                posting = await PostingFacade.Update(posting, token: token);
                if (posting.RenderedLaTex != null && renderPDF)
                    await RenderResume(posting, token);
                return posting;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                throw;
            }
        }

        public async Task RenderResume(Posting posting, CancellationToken token = default)
        {
            try
            {
                if (posting.RenderedLaTex != null)
                {
                    await ResumeBlob.UploadResume(posting.Id, await Templator.RenderLatex(posting.RenderedLaTex, token), token);
                }
            }
            catch(Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                throw;
            }
        }

        public async Task BuildCoverLetter(Posting posting, Guid documentTemplateId, IProgress<string>? progress = null, bool renderPDF = true, CancellationToken token = default)
        {
            try
            {
                progress?.Report("Preparing Cover Letter");
                var doucmentTemplate = await DocumentTemplateFacade.GetByID(documentTemplateId, token: token);
                if (doucmentTemplate == null)
                    throw new InvalidDataException("Document Template not found.");
                var cl = await Enricher.GenerateCoverLetter(posting, progress, token);
                if(cl == null)
                    throw new InvalidDataException("Cover Letter could not be generated.");
                cl.User = await UserFacade.GetByID(posting.UserId, token: token) ?? throw new InvalidDataException("User not found.");
                posting.CoverLetterLaTeX = await Templator.ApplyTemplate(doucmentTemplate.Template, cl, token);
                posting = await PostingFacade.Update(posting, token: token);
                if (!string.IsNullOrWhiteSpace(posting.CoverLetterLaTeX) && renderPDF)
                    await RenderCoverLetter(posting, token);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                throw;
            }
        }

        public async Task RenderCoverLetter(Posting posting, CancellationToken token = default)
        {
            try
            {
                if (posting.CoverLetterLaTeX != null)
                {
                    await ResumeBlob.UploadCoverLetter(posting.Id, await Templator.RenderLatex(posting.CoverLetterLaTeX, token), token);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                throw;
            }
        }

        public async Task RenderMarkdown(Posting posting, Guid templateId, CancellationToken token = default)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(posting.ResumeJson))
                {
                    Resume? res = JsonSerializer.Deserialize<Resume>(posting.ResumeJson);
                    if (res == null)
                        throw new InvalidDataException();
                    var template = await DocumentTemplateFacade.GetByID(templateId, token: token);
                    if(template == null)
                        throw new InvalidDataException();
                    posting.ResumeMarkdown = await Templator.ApplyTemplate(template.Template, res, token);
                    await PostingFacade.Update(posting, token: token);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                throw;
            }
        }
    }
}
