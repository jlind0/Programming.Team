using Microsoft.Extensions.Logging;
using Moq;
using Programming.Team.AI.Core;
using Programming.Team.Business.Core;
using Programming.Team.Core;
using Programming.Team.Data.Core;
using Programming.Team.Templating.Core;
using System.Linq.Expressions;
using System.Text.Json;

namespace Programming.Team.Business.Tests;

[TestClass]
public class ResumeBuilderTests
{
    [TestMethod]
    public async Task ResumeBuilderTests_BuildResume()
    {
        var logger = new Mock<ILogger<ResumeBuilder>>();
        var resumeBlob = new Mock<IResumeBlob>();
        var userFacade = new Mock<IUserBusinessFacade>();
        var strUser = await File.ReadAllTextAsync("user.json");
        userFacade.Setup(u => u.GetByID(It.IsAny<Guid>(), It.IsAny<IUnitOfWork>(), 
            It.IsAny<Func<IQueryable<User>, IQueryable<User>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(
                JsonSerializer.Deserialize<User>(strUser));
        
        var positionFacade = new Mock<IBusinessRepositoryFacade<Position, Guid>>();
        var strPosition = await File.ReadAllTextAsync("positions.json");
        var positionData = JsonSerializer.Deserialize<RepositoryResultSet<Guid, Position>>(strPosition) ?? throw new InvalidDataException();
        positionFacade.Setup(p => p.Get(It.IsAny<IUnitOfWork>(), null, It.IsAny<Expression<Func<Position, bool>>>(),
            It.IsAny<Func<IQueryable<Position>, IOrderedQueryable<Position>>>(), 
            It.IsAny<Func<IQueryable<Position>, IQueryable<Position>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(positionData);
        
        var educationFacade = new Mock<IBusinessRepositoryFacade<Education, Guid>>();
        var strEducation = await File.ReadAllTextAsync("education.json");
        var educationData = JsonSerializer.Deserialize<RepositoryResultSet<Guid, Education>>(strEducation) ?? throw new InvalidDataException();
        educationFacade.Setup(e => e.Get(It.IsAny<IUnitOfWork>(), null, It.IsAny<Expression<Func<Education, bool>>>(),
            It.IsAny<Func<IQueryable<Education>, IOrderedQueryable<Education>>>(),
            It.IsAny<Func<IQueryable<Education>, IQueryable<Education>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(educationData);
        
        var publicationFacade = new Mock<IBusinessRepositoryFacade<Publication, Guid>>();
        var strPublication = await File.ReadAllTextAsync("publications.json");
        var publicationData = JsonSerializer.Deserialize<RepositoryResultSet<Guid, Publication>>(strPublication) ?? throw new InvalidDataException();
        publicationFacade.Setup(p => p.Get(It.IsAny<IUnitOfWork>(), null, It.IsAny<Expression<Func<Publication, bool>>>(),
            It.IsAny<Func<IQueryable<Publication>, IOrderedQueryable<Publication>>>(),
            It.IsAny<Func<IQueryable<Publication>, IQueryable<Publication>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(publicationData);
        
        var certificationFacade = new Mock<IBusinessRepositoryFacade<Certificate, Guid>>();
        var strCertification = await File.ReadAllTextAsync("certs.json");
        var certificationData = JsonSerializer.Deserialize<RepositoryResultSet<Guid,Certificate>>(strCertification) ?? throw new InvalidDataException();
        certificationFacade.Setup(c => c.Get(It.IsAny<IUnitOfWork>(), null, It.IsAny<Expression<Func<Certificate, bool>>>(),
            It.IsAny<Func<IQueryable<Certificate>, IOrderedQueryable<Certificate>>>(),
            It.IsAny<Func<IQueryable<Certificate>, IQueryable<Certificate>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(certificationData);
        
        var documentTemplateFacade = new Mock<IBusinessRepositoryFacade<DocumentTemplate, Guid>>();
        var postingFacade = new Mock<IBusinessRepositoryFacade<Posting, Guid>>();
        var documentTemplator = new Mock<IDocumentTemplator>();
        var resumeEnricher = new Mock<IResumeEnricher>();
        var sectionTemplateFacade = new Mock<ISectionTemplateBusinessFacade>();
        var resumeBuilder = new ResumeBuilder(logger.Object, userFacade.Object,
            positionFacade.Object, educationFacade.Object, publicationFacade.Object, certificationFacade.Object,
            documentTemplateFacade.Object, postingFacade.Object, documentTemplator.Object, resumeEnricher.Object,
            resumeBlob.Object, sectionTemplateFacade.Object);
        var token = new CancellationToken();
        var progress = new Mock<IProgress<string>>();
        progress.Setup(p => p.Report(It.IsAny<string>())).Verifiable(Times.AtLeastOnce());
        var resume = await resumeBuilder.BuildResume(Guid.NewGuid(), progress.Object, token);
        Assert.IsNotNull(resume);
        progress.Verify();
        Assert.AreNotEqual(0, resume.Skills.Count);

    }
    [TestMethod]
    public async Task ResumeBuilderTests_RebuildPosting()
    {
        var logger = new Mock<ILogger<ResumeBuilder>>();
        var resumeBlob = new Mock<IResumeBlob>();
        resumeBlob.Setup(r => r.UploadResume(It.IsAny<Guid>(), It.IsAny<byte[]>(), It.IsAny<CancellationToken>()));

        var userFacade = new Mock<IUserBusinessFacade>();
        userFacade.Setup(u => u.UtilizeResumeGeneration(It.IsAny<Guid>(), It.IsAny<IUnitOfWork>(), 
            It.IsAny<CancellationToken>())).ReturnsAsync(true);
        userFacade.Setup(u => u.GetCurrentUserId(It.IsAny<IUnitOfWork>(), It.IsAny<bool>(), 
            It.IsAny<CancellationToken>())).ReturnsAsync(Guid.NewGuid());

        var positionFacade = new Mock<IBusinessRepositoryFacade<Position, Guid>>();
        var educationFacade = new Mock<IBusinessRepositoryFacade<Education, Guid>>();
        var publicationFacade = new Mock<IBusinessRepositoryFacade<Publication, Guid>>();
        var certificationFacade = new Mock<IBusinessRepositoryFacade<Certificate, Guid>>();

        var documentTemplateFacade = new Mock<IBusinessRepositoryFacade<DocumentTemplate, Guid>>();
        documentTemplateFacade.Setup(d => d.GetByID(It.IsAny<Guid>(), It.IsAny<IUnitOfWork>(), 
                It.IsAny<Func<IQueryable<DocumentTemplate>, IQueryable<DocumentTemplate>>>(),
                It.IsAny<CancellationToken>())).ReturnsAsync(new DocumentTemplate() { Id = Guid.NewGuid(), Template = "some template"});
        
        var strPosting = await File.ReadAllTextAsync("posting.json");
        var posting = JsonSerializer.Deserialize<Posting>(strPosting) ?? throw new InvalidDataException();
        var postingFacade = new Mock<IBusinessRepositoryFacade<Posting, Guid>>();
        postingFacade.Setup(p => p.Update(It.IsAny<Posting>(), It.IsAny<IUnitOfWork>(),
            It.IsAny<Func<IQueryable<Posting>, IQueryable<Posting>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(posting);
        
        var documentTemplator = new Mock<IDocumentTemplator>();
        documentTemplator.Setup(d => d.ApplyTemplate(It.IsAny<string>(), It.IsAny<Resume>(), 
            It.IsAny<CancellationToken>())).ReturnsAsync("some template");
        documentTemplator.Setup(d => d.RenderLatex(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync([0x00]);

        var resumeEnricher = new Mock<IResumeEnricher>();
        resumeEnricher.Setup(r => r.EnrichResume(It.IsAny<Resume>(), It.IsAny<Posting>(), It.IsAny<IProgress<string>>(), It.IsAny<CancellationToken>()));
        var sectionTemplateFacade = new Mock<ISectionTemplateBusinessFacade>();
        sectionTemplateFacade.Setup(s => s.GetByID(It.IsAny<Guid>(), It.IsAny<IUnitOfWork>(), 
                It.IsAny<Func<IQueryable<SectionTemplate>, IQueryable<SectionTemplate>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(new SectionTemplate()
                {
                    Id = Guid.NewGuid(),
                    SectionId = ResumePart.Positions,
                    Template = "some template"
                });
        sectionTemplateFacade.Setup(s => s.GetDefaultSection(It.IsAny<ResumePart>(), It.IsAny<Guid>(), It.IsAny<IUnitOfWork>(),
                It.IsAny<CancellationToken>())).ReturnsAsync(new SectionTemplate()
                {
                    Id = Guid.NewGuid(),
                    SectionId = ResumePart.Positions,
                    Template = "some template"
                });
        var resumeBuilder = new ResumeBuilder(logger.Object, userFacade.Object,
            positionFacade.Object, educationFacade.Object, publicationFacade.Object, certificationFacade.Object,
            documentTemplateFacade.Object, postingFacade.Object, documentTemplator.Object, resumeEnricher.Object,
            resumeBlob.Object, sectionTemplateFacade.Object);

        var token = new CancellationToken();
        var progress = new Mock<IProgress<string>>();
        progress.Setup(p => p.Report(It.IsAny<string>())).Verifiable(Times.AtLeastOnce());
        var strResume = await File.ReadAllTextAsync("resume.json");
        var resume = JsonSerializer.Deserialize<Resume>(strResume) ?? throw new InvalidDataException();
        await resumeBuilder.RebuildPosting(posting, resume, progress: progress.Object, token: token);
        userFacade.Verify(userFacade => userFacade.UtilizeResumeGeneration(It.IsAny<Guid>(), It.IsAny<IUnitOfWork>(), It.IsAny<CancellationToken>()), Times.Once());
        resumeBlob.Verify(resumeBlob => resumeBlob.UploadResume(It.IsAny<Guid>(), It.IsAny<byte[]>(), It.IsAny<CancellationToken>()), Times.Once());
        postingFacade.Verify(postingFacade => postingFacade.Update(It.IsAny<Posting>(), It.IsAny<IUnitOfWork>(), It.IsAny<Func<IQueryable<Posting>, IQueryable<Posting>>>(), It.IsAny<CancellationToken>()), Times.Once());
        documentTemplator.Verify(documentTemplator => documentTemplator.ApplyTemplate(It.IsAny<string>(), It.IsAny<Resume>(), It.IsAny<CancellationToken>()), Times.Once());
        documentTemplator.Verify(documentTemplator => documentTemplator.RenderLatex(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once());
        userFacade.Verify(userFacade => userFacade.GetCurrentUserId(It.IsAny<IUnitOfWork>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Once());
        sectionTemplateFacade.Verify(sectionTemplateFacade => sectionTemplateFacade.GetDefaultSection(
            It.IsAny<ResumePart>(), It.IsAny<Guid>(), It.IsAny<IUnitOfWork>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce());
        resumeEnricher.Verify(resumeEnricher => resumeEnricher.EnrichResume(It.IsAny<Resume>(), It.IsAny<Posting>(), 
            It.IsAny<IProgress<string>>(), It.IsAny<CancellationToken>()), Times.Once());
        progress.Verify();
    }
    [TestMethod]
    public async Task ResumeBuilderTests_RebuildPosting_FailUtilize()
    {
        var logger = new Mock<ILogger<ResumeBuilder>>();
        var resumeBlob = new Mock<IResumeBlob>();
        resumeBlob.Setup(r => r.UploadResume(It.IsAny<Guid>(), It.IsAny<byte[]>(), It.IsAny<CancellationToken>()));

        var userFacade = new Mock<IUserBusinessFacade>();
        userFacade.Setup(u => u.UtilizeResumeGeneration(It.IsAny<Guid>(), It.IsAny<IUnitOfWork>(),
            It.IsAny<CancellationToken>())).ReturnsAsync(false);
        userFacade.Setup(u => u.GetCurrentUserId(It.IsAny<IUnitOfWork>(), It.IsAny<bool>(),
            It.IsAny<CancellationToken>())).ReturnsAsync(Guid.NewGuid());

        var positionFacade = new Mock<IBusinessRepositoryFacade<Position, Guid>>();
        var educationFacade = new Mock<IBusinessRepositoryFacade<Education, Guid>>();
        var publicationFacade = new Mock<IBusinessRepositoryFacade<Publication, Guid>>();
        var certificationFacade = new Mock<IBusinessRepositoryFacade<Certificate, Guid>>();

        var documentTemplateFacade = new Mock<IBusinessRepositoryFacade<DocumentTemplate, Guid>>();
        documentTemplateFacade.Setup(d => d.GetByID(It.IsAny<Guid>(), It.IsAny<IUnitOfWork>(),
                It.IsAny<Func<IQueryable<DocumentTemplate>, IQueryable<DocumentTemplate>>>(),
                It.IsAny<CancellationToken>())).ReturnsAsync(new DocumentTemplate() { Id = Guid.NewGuid(), Template = "some template" });

        var strPosting = await File.ReadAllTextAsync("posting.json");
        var posting = JsonSerializer.Deserialize<Posting>(strPosting) ?? throw new InvalidDataException();
        var postingFacade = new Mock<IBusinessRepositoryFacade<Posting, Guid>>();
        postingFacade.Setup(p => p.Update(It.IsAny<Posting>(), It.IsAny<IUnitOfWork>(),
            It.IsAny<Func<IQueryable<Posting>, IQueryable<Posting>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(posting);

        var documentTemplator = new Mock<IDocumentTemplator>();
        documentTemplator.Setup(d => d.ApplyTemplate(It.IsAny<string>(), It.IsAny<Resume>(),
            It.IsAny<CancellationToken>())).ReturnsAsync("some template");
        documentTemplator.Setup(d => d.RenderLatex(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync([0x00]);

        var resumeEnricher = new Mock<IResumeEnricher>();
        resumeEnricher.Setup(r => r.EnrichResume(It.IsAny<Resume>(), It.IsAny<Posting>(), It.IsAny<IProgress<string>>(), It.IsAny<CancellationToken>()));
        var sectionTemplateFacade = new Mock<ISectionTemplateBusinessFacade>();
        sectionTemplateFacade.Setup(s => s.GetByID(It.IsAny<Guid>(), It.IsAny<IUnitOfWork>(),
                It.IsAny<Func<IQueryable<SectionTemplate>, IQueryable<SectionTemplate>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(new SectionTemplate()
                {
                    Id = Guid.NewGuid(),
                    SectionId = ResumePart.Positions,
                    Template = "some template"
                });
        sectionTemplateFacade.Setup(s => s.GetDefaultSection(It.IsAny<ResumePart>(), It.IsAny<Guid>(), It.IsAny<IUnitOfWork>(),
                It.IsAny<CancellationToken>())).ReturnsAsync(new SectionTemplate()
                {
                    Id = Guid.NewGuid(),
                    SectionId = ResumePart.Positions,
                    Template = "some template"
                });
        var resumeBuilder = new ResumeBuilder(logger.Object, userFacade.Object,
            positionFacade.Object, educationFacade.Object, publicationFacade.Object, certificationFacade.Object,
            documentTemplateFacade.Object, postingFacade.Object, documentTemplator.Object, resumeEnricher.Object,
            resumeBlob.Object, sectionTemplateFacade.Object);

        var token = new CancellationToken();
        var progress = new Mock<IProgress<string>>();
        progress.Setup(p => p.Report(It.IsAny<string>())).Verifiable(Times.AtLeastOnce());
        var strResume = await File.ReadAllTextAsync("resume.json");
        var resume = JsonSerializer.Deserialize<Resume>(strResume) ?? throw new InvalidDataException();
        await resumeBuilder.RebuildPosting(posting, resume, progress: progress.Object, token: token);
        userFacade.Verify(userFacade => userFacade.UtilizeResumeGeneration(It.IsAny<Guid>(), It.IsAny<IUnitOfWork>(), It.IsAny<CancellationToken>()), Times.Once());
        resumeBlob.Verify(resumeBlob => resumeBlob.UploadResume(It.IsAny<Guid>(), It.IsAny<byte[]>(), It.IsAny<CancellationToken>()), Times.Once());
        postingFacade.Verify(postingFacade => postingFacade.Update(It.IsAny<Posting>(), It.IsAny<IUnitOfWork>(), It.IsAny<Func<IQueryable<Posting>, IQueryable<Posting>>>(), It.IsAny<CancellationToken>()), Times.Once());
        documentTemplator.Verify(documentTemplator => documentTemplator.ApplyTemplate(It.IsAny<string>(), It.IsAny<Resume>(), It.IsAny<CancellationToken>()), Times.Once());
        documentTemplator.Verify(documentTemplator => documentTemplator.RenderLatex(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once());
        userFacade.Verify(userFacade => userFacade.GetCurrentUserId(It.IsAny<IUnitOfWork>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Once());
        sectionTemplateFacade.Verify(sectionTemplateFacade => sectionTemplateFacade.GetDefaultSection(
            It.IsAny<ResumePart>(), It.IsAny<Guid>(), It.IsAny<IUnitOfWork>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce());
        resumeEnricher.Verify(resumeEnricher => resumeEnricher.EnrichResume(It.IsAny<Resume>(), It.IsAny<Posting>(),
            It.IsAny<IProgress<string>>(), It.IsAny<CancellationToken>()), Times.Never());
        progress.Verify();
    }
    [TestMethod]
    public async Task ResumeBuilderTests_RebuildPosting_DoNotEnrich()
    {
        var logger = new Mock<ILogger<ResumeBuilder>>();
        var resumeBlob = new Mock<IResumeBlob>();
        resumeBlob.Setup(r => r.UploadResume(It.IsAny<Guid>(), It.IsAny<byte[]>(), It.IsAny<CancellationToken>()));

        var userFacade = new Mock<IUserBusinessFacade>();
        userFacade.Setup(u => u.UtilizeResumeGeneration(It.IsAny<Guid>(), It.IsAny<IUnitOfWork>(),
            It.IsAny<CancellationToken>())).ReturnsAsync(false);
        userFacade.Setup(u => u.GetCurrentUserId(It.IsAny<IUnitOfWork>(), It.IsAny<bool>(),
            It.IsAny<CancellationToken>())).ReturnsAsync(Guid.NewGuid());

        var positionFacade = new Mock<IBusinessRepositoryFacade<Position, Guid>>();
        var educationFacade = new Mock<IBusinessRepositoryFacade<Education, Guid>>();
        var publicationFacade = new Mock<IBusinessRepositoryFacade<Publication, Guid>>();
        var certificationFacade = new Mock<IBusinessRepositoryFacade<Certificate, Guid>>();

        var documentTemplateFacade = new Mock<IBusinessRepositoryFacade<DocumentTemplate, Guid>>();
        documentTemplateFacade.Setup(d => d.GetByID(It.IsAny<Guid>(), It.IsAny<IUnitOfWork>(),
                It.IsAny<Func<IQueryable<DocumentTemplate>, IQueryable<DocumentTemplate>>>(),
                It.IsAny<CancellationToken>())).ReturnsAsync(new DocumentTemplate() { Id = Guid.NewGuid(), Template = "some template" });

        var strPosting = await File.ReadAllTextAsync("posting.json");
        var posting = JsonSerializer.Deserialize<Posting>(strPosting) ?? throw new InvalidDataException();
        var postingFacade = new Mock<IBusinessRepositoryFacade<Posting, Guid>>();
        postingFacade.Setup(p => p.Update(It.IsAny<Posting>(), It.IsAny<IUnitOfWork>(),
            It.IsAny<Func<IQueryable<Posting>, IQueryable<Posting>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(posting);

        var documentTemplator = new Mock<IDocumentTemplator>();
        documentTemplator.Setup(d => d.ApplyTemplate(It.IsAny<string>(), It.IsAny<Resume>(),
            It.IsAny<CancellationToken>())).ReturnsAsync("some template");
        documentTemplator.Setup(d => d.RenderLatex(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync([0x00]);

        var resumeEnricher = new Mock<IResumeEnricher>();
        resumeEnricher.Setup(r => r.EnrichResume(It.IsAny<Resume>(), It.IsAny<Posting>(), It.IsAny<IProgress<string>>(), It.IsAny<CancellationToken>()));
        var sectionTemplateFacade = new Mock<ISectionTemplateBusinessFacade>();
        sectionTemplateFacade.Setup(s => s.GetByID(It.IsAny<Guid>(), It.IsAny<IUnitOfWork>(),
                It.IsAny<Func<IQueryable<SectionTemplate>, IQueryable<SectionTemplate>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(new SectionTemplate()
                {
                    Id = Guid.NewGuid(),
                    SectionId = ResumePart.Positions,
                    Template = "some template"
                });
        sectionTemplateFacade.Setup(s => s.GetDefaultSection(It.IsAny<ResumePart>(), It.IsAny<Guid>(), It.IsAny<IUnitOfWork>(),
                It.IsAny<CancellationToken>())).ReturnsAsync(new SectionTemplate()
                {
                    Id = Guid.NewGuid(),
                    SectionId = ResumePart.Positions,
                    Template = "some template"
                });
        var resumeBuilder = new ResumeBuilder(logger.Object, userFacade.Object,
            positionFacade.Object, educationFacade.Object, publicationFacade.Object, certificationFacade.Object,
            documentTemplateFacade.Object, postingFacade.Object, documentTemplator.Object, resumeEnricher.Object,
            resumeBlob.Object, sectionTemplateFacade.Object);

        var token = new CancellationToken();
        var progress = new Mock<IProgress<string>>();
        progress.Setup(p => p.Report(It.IsAny<string>())).Verifiable(Times.AtLeastOnce());
        var strResume = await File.ReadAllTextAsync("resume.json");
        var resume = JsonSerializer.Deserialize<Resume>(strResume) ?? throw new InvalidDataException();

        await resumeBuilder.RebuildPosting(posting, resume,enrich: false, progress: progress.Object, token: token);
        
        userFacade.Verify(userFacade => userFacade.UtilizeResumeGeneration(It.IsAny<Guid>(), It.IsAny<IUnitOfWork>(), It.IsAny<CancellationToken>()), Times.Never());
        resumeBlob.Verify(resumeBlob => resumeBlob.UploadResume(It.IsAny<Guid>(), It.IsAny<byte[]>(), It.IsAny<CancellationToken>()), Times.Once());
        postingFacade.Verify(postingFacade => postingFacade.Update(It.IsAny<Posting>(), It.IsAny<IUnitOfWork>(), It.IsAny<Func<IQueryable<Posting>, IQueryable<Posting>>>(), It.IsAny<CancellationToken>()), Times.Once());
        documentTemplator.Verify(documentTemplator => documentTemplator.ApplyTemplate(It.IsAny<string>(), It.IsAny<Resume>(), It.IsAny<CancellationToken>()), Times.Once());
        documentTemplator.Verify(documentTemplator => documentTemplator.RenderLatex(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once());
        userFacade.Verify(userFacade => userFacade.GetCurrentUserId(It.IsAny<IUnitOfWork>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Never());
        sectionTemplateFacade.Verify(sectionTemplateFacade => sectionTemplateFacade.GetDefaultSection(
            It.IsAny<ResumePart>(), It.IsAny<Guid>(), It.IsAny<IUnitOfWork>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce());
        resumeEnricher.Verify(resumeEnricher => resumeEnricher.EnrichResume(It.IsAny<Resume>(), It.IsAny<Posting>(),
            It.IsAny<IProgress<string>>(), It.IsAny<CancellationToken>()), Times.Never());
        progress.Verify();
    }
    [TestMethod]
    public async Task ResumeBuilderTests_RebuildPosting_DoNotRender()
    {
        var logger = new Mock<ILogger<ResumeBuilder>>();
        var resumeBlob = new Mock<IResumeBlob>();
        resumeBlob.Setup(r => r.UploadResume(It.IsAny<Guid>(), It.IsAny<byte[]>(), It.IsAny<CancellationToken>()));

        var userFacade = new Mock<IUserBusinessFacade>();
        userFacade.Setup(u => u.UtilizeResumeGeneration(It.IsAny<Guid>(), It.IsAny<IUnitOfWork>(),
            It.IsAny<CancellationToken>())).ReturnsAsync(true);
        userFacade.Setup(u => u.GetCurrentUserId(It.IsAny<IUnitOfWork>(), It.IsAny<bool>(),
            It.IsAny<CancellationToken>())).ReturnsAsync(Guid.NewGuid());

        var positionFacade = new Mock<IBusinessRepositoryFacade<Position, Guid>>();
        var educationFacade = new Mock<IBusinessRepositoryFacade<Education, Guid>>();
        var publicationFacade = new Mock<IBusinessRepositoryFacade<Publication, Guid>>();
        var certificationFacade = new Mock<IBusinessRepositoryFacade<Certificate, Guid>>();

        var documentTemplateFacade = new Mock<IBusinessRepositoryFacade<DocumentTemplate, Guid>>();
        documentTemplateFacade.Setup(d => d.GetByID(It.IsAny<Guid>(), It.IsAny<IUnitOfWork>(),
                It.IsAny<Func<IQueryable<DocumentTemplate>, IQueryable<DocumentTemplate>>>(),
                It.IsAny<CancellationToken>())).ReturnsAsync(new DocumentTemplate() { Id = Guid.NewGuid(), Template = "some template" });

        var strPosting = await File.ReadAllTextAsync("posting.json");
        var posting = JsonSerializer.Deserialize<Posting>(strPosting) ?? throw new InvalidDataException();
        var postingFacade = new Mock<IBusinessRepositoryFacade<Posting, Guid>>();
        postingFacade.Setup(p => p.Update(It.IsAny<Posting>(), It.IsAny<IUnitOfWork>(),
            It.IsAny<Func<IQueryable<Posting>, IQueryable<Posting>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(posting);

        var documentTemplator = new Mock<IDocumentTemplator>();
        documentTemplator.Setup(d => d.ApplyTemplate(It.IsAny<string>(), It.IsAny<Resume>(),
            It.IsAny<CancellationToken>())).ReturnsAsync("some template");
        documentTemplator.Setup(d => d.RenderLatex(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync([0x00]);

        var resumeEnricher = new Mock<IResumeEnricher>();
        resumeEnricher.Setup(r => r.EnrichResume(It.IsAny<Resume>(), It.IsAny<Posting>(), It.IsAny<IProgress<string>>(), It.IsAny<CancellationToken>()));
        var sectionTemplateFacade = new Mock<ISectionTemplateBusinessFacade>();
        sectionTemplateFacade.Setup(s => s.GetByID(It.IsAny<Guid>(), It.IsAny<IUnitOfWork>(),
                It.IsAny<Func<IQueryable<SectionTemplate>, IQueryable<SectionTemplate>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(new SectionTemplate()
                {
                    Id = Guid.NewGuid(),
                    SectionId = ResumePart.Positions,
                    Template = "some template"
                });
        sectionTemplateFacade.Setup(s => s.GetDefaultSection(It.IsAny<ResumePart>(), It.IsAny<Guid>(), It.IsAny<IUnitOfWork>(),
                It.IsAny<CancellationToken>())).ReturnsAsync(new SectionTemplate()
                {
                    Id = Guid.NewGuid(),
                    SectionId = ResumePart.Positions,
                    Template = "some template"
                });
        var resumeBuilder = new ResumeBuilder(logger.Object, userFacade.Object,
            positionFacade.Object, educationFacade.Object, publicationFacade.Object, certificationFacade.Object,
            documentTemplateFacade.Object, postingFacade.Object, documentTemplator.Object, resumeEnricher.Object,
            resumeBlob.Object, sectionTemplateFacade.Object);

        var token = new CancellationToken();
        var progress = new Mock<IProgress<string>>();
        progress.Setup(p => p.Report(It.IsAny<string>())).Verifiable(Times.AtLeastOnce());
        var strResume = await File.ReadAllTextAsync("resume.json");
        var resume = JsonSerializer.Deserialize<Resume>(strResume) ?? throw new InvalidDataException();
        await resumeBuilder.RebuildPosting(posting, resume,renderPDF: false, progress: progress.Object, token: token);
        userFacade.Verify(userFacade => userFacade.UtilizeResumeGeneration(It.IsAny<Guid>(), It.IsAny<IUnitOfWork>(), It.IsAny<CancellationToken>()), Times.Once());
        resumeBlob.Verify(resumeBlob => resumeBlob.UploadResume(It.IsAny<Guid>(), It.IsAny<byte[]>(), It.IsAny<CancellationToken>()), Times.Never());
        postingFacade.Verify(postingFacade => postingFacade.Update(It.IsAny<Posting>(), It.IsAny<IUnitOfWork>(), It.IsAny<Func<IQueryable<Posting>, IQueryable<Posting>>>(), It.IsAny<CancellationToken>()), Times.Once());
        documentTemplator.Verify(documentTemplator => documentTemplator.ApplyTemplate(It.IsAny<string>(), It.IsAny<Resume>(), It.IsAny<CancellationToken>()), Times.Once());
        documentTemplator.Verify(documentTemplator => documentTemplator.RenderLatex(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never());
        userFacade.Verify(userFacade => userFacade.GetCurrentUserId(It.IsAny<IUnitOfWork>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Once());
        sectionTemplateFacade.Verify(sectionTemplateFacade => sectionTemplateFacade.GetDefaultSection(
            It.IsAny<ResumePart>(), It.IsAny<Guid>(), It.IsAny<IUnitOfWork>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce());
        resumeEnricher.Verify(resumeEnricher => resumeEnricher.EnrichResume(It.IsAny<Resume>(), It.IsAny<Posting>(),
            It.IsAny<IProgress<string>>(), It.IsAny<CancellationToken>()), Times.Once());
        progress.Verify();
    }
    [TestMethod]
    public async Task ResumeBuilderTests_RenderResume()
    {
        var logger = new Mock<ILogger<ResumeBuilder>>();
        var resumeBlob = new Mock<IResumeBlob>();
        resumeBlob.Setup(r => r.UploadResume(It.IsAny<Guid>(), It.IsAny<byte[]>(), It.IsAny<CancellationToken>()));

        var userFacade = new Mock<IUserBusinessFacade>();

        var positionFacade = new Mock<IBusinessRepositoryFacade<Position, Guid>>();
        var educationFacade = new Mock<IBusinessRepositoryFacade<Education, Guid>>();
        var publicationFacade = new Mock<IBusinessRepositoryFacade<Publication, Guid>>();
        var certificationFacade = new Mock<IBusinessRepositoryFacade<Certificate, Guid>>();
        var documentTemplateFacade = new Mock<IBusinessRepositoryFacade<DocumentTemplate, Guid>>();

        var strPosting = await File.ReadAllTextAsync("posting.json");
        var posting = JsonSerializer.Deserialize<Posting>(strPosting) ?? throw new InvalidDataException();
        var postingFacade = new Mock<IBusinessRepositoryFacade<Posting, Guid>>();

        var documentTemplator = new Mock<IDocumentTemplator>();
        documentTemplator.Setup(d => d.RenderLatex(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync([0x00]);

        var resumeEnricher = new Mock<IResumeEnricher>();
        var sectionTemplateFacade = new Mock<ISectionTemplateBusinessFacade>();
        var resumeBuilder = new ResumeBuilder(logger.Object, userFacade.Object,
            positionFacade.Object, educationFacade.Object, publicationFacade.Object, certificationFacade.Object,
            documentTemplateFacade.Object, postingFacade.Object, documentTemplator.Object, resumeEnricher.Object,
            resumeBlob.Object, sectionTemplateFacade.Object);
        await resumeBuilder.RenderResume(posting, CancellationToken.None);
        resumeBlob.Verify(resumeBlob => resumeBlob.UploadResume(It.IsAny<Guid>(), It.IsAny<byte[]>(), It.IsAny<CancellationToken>()), Times.Once());
        documentTemplator.Verify(documentTemplator => documentTemplator.RenderLatex(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once());
    }
    [TestMethod]
    public async Task ResumeBuilderTests_BuildPosting()
    {
        var logger = new Mock<ILogger<ResumeBuilder>>();
        var resumeBlob = new Mock<IResumeBlob>();
        resumeBlob.Setup(r => r.UploadResume(It.IsAny<Guid>(), It.IsAny<byte[]>(), It.IsAny<CancellationToken>()));

        var userFacade = new Mock<IUserBusinessFacade>();
        var strUser = await File.ReadAllTextAsync("user.json");
        var user = JsonSerializer.Deserialize<User>(strUser) ?? throw new InvalidDataException();
        userFacade.Setup(u => u.UtilizeResumeGeneration(It.IsAny<Guid>(), It.IsAny<IUnitOfWork>(),
            It.IsAny<CancellationToken>())).ReturnsAsync(true);
        userFacade.Setup(u => u.GetCurrentUserId(It.IsAny<IUnitOfWork>(), It.IsAny<bool>(),
            It.IsAny<CancellationToken>())).ReturnsAsync(Guid.NewGuid());
        userFacade.Setup(u => u.GetByID(It.IsAny<Guid>(), It.IsAny<IUnitOfWork>(),
            It.IsAny<Func<IQueryable<User>, IQueryable<User>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var positionFacade = new Mock<IBusinessRepositoryFacade<Position, Guid>>();
        var educationFacade = new Mock<IBusinessRepositoryFacade<Education, Guid>>();
        var publicationFacade = new Mock<IBusinessRepositoryFacade<Publication, Guid>>();
        var certificationFacade = new Mock<IBusinessRepositoryFacade<Certificate, Guid>>();

        var documentTemplateFacade = new Mock<IBusinessRepositoryFacade<DocumentTemplate, Guid>>();
        documentTemplateFacade.Setup(d => d.GetByID(It.IsAny<Guid>(), It.IsAny<IUnitOfWork>(),
                It.IsAny<Func<IQueryable<DocumentTemplate>, IQueryable<DocumentTemplate>>>(),
                It.IsAny<CancellationToken>())).ReturnsAsync(new DocumentTemplate() { Id = Guid.NewGuid(), Template = "some template" });

        var postingFacade = new Mock<IBusinessRepositoryFacade<Posting, Guid>>();
        postingFacade.Setup(p => p.Update(It.IsAny<Posting>(), It.IsAny<IUnitOfWork>(),
            It.IsAny<Func<IQueryable<Posting>, IQueryable<Posting>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(
                (Posting posting, IUnitOfWork uow, Func<IQueryable<Posting>, IQueryable<Posting>> filter, CancellationToken token) 
                    => posting);
        postingFacade.Setup(p => p.Add(It.IsAny<Posting>(), It.IsAny<IUnitOfWork>(), It.IsAny<CancellationToken>()));

        var documentTemplator = new Mock<IDocumentTemplator>();
        documentTemplator.Setup(d => d.ApplyTemplate(It.IsAny<string>(), It.IsAny<Resume>(),
            It.IsAny<CancellationToken>())).ReturnsAsync("some template");
        documentTemplator.Setup(d => d.RenderLatex(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync([0x00]);

        var resumeEnricher = new Mock<IResumeEnricher>();
        resumeEnricher.Setup(r => r.EnrichResume(It.IsAny<Resume>(), It.IsAny<Posting>(), It.IsAny<IProgress<string>>(), It.IsAny<CancellationToken>()));
        var sectionTemplateFacade = new Mock<ISectionTemplateBusinessFacade>();
        sectionTemplateFacade.Setup(s => s.GetByID(It.IsAny<Guid>(), It.IsAny<IUnitOfWork>(),
                It.IsAny<Func<IQueryable<SectionTemplate>, IQueryable<SectionTemplate>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(new SectionTemplate()
                {
                    Id = Guid.NewGuid(),
                    SectionId = ResumePart.Positions,
                    Template = "some template"
                });
        sectionTemplateFacade.Setup(s => s.GetDefaultSection(It.IsAny<ResumePart>(), It.IsAny<Guid>(), It.IsAny<IUnitOfWork>(),
                It.IsAny<CancellationToken>())).ReturnsAsync(new SectionTemplate()
                {
                    Id = Guid.NewGuid(),
                    SectionId = ResumePart.Positions,
                    Template = "some template"
                });
        var resumeBuilder = new ResumeBuilder(logger.Object, userFacade.Object,
            positionFacade.Object, educationFacade.Object, publicationFacade.Object, certificationFacade.Object,
            documentTemplateFacade.Object, postingFacade.Object, documentTemplator.Object, resumeEnricher.Object,
            resumeBlob.Object, sectionTemplateFacade.Object);
        var strResume = await File.ReadAllTextAsync("resume.json");
        var resume = JsonSerializer.Deserialize<Resume>(strResume) ?? throw new InvalidDataException();
        var progress = new Mock<IProgress<string>>();
        progress.Setup(p => p.Report(It.IsAny<string>())).Verifiable(Times.AtLeastOnce());
        var posting = await resumeBuilder.BuildPosting(Guid.NewGuid(), Guid.NewGuid(), "Some Name", "Some Description", resume, progress.Object);
        Assert.IsNotNull(posting);
        userFacade.Verify(s => s.GetByID(It.IsAny<Guid>(), It.IsAny<IUnitOfWork>(),
            It.IsAny<Func<IQueryable<User>,IQueryable<User>>>(), It.IsAny<CancellationToken>()), Times.Once());
        userFacade.Verify(userFacade => userFacade.UtilizeResumeGeneration(It.IsAny<Guid>(), It.IsAny<IUnitOfWork>(), It.IsAny<CancellationToken>()), Times.Once());
        resumeBlob.Verify(resumeBlob => resumeBlob.UploadResume(It.IsAny<Guid>(), It.IsAny<byte[]>(), It.IsAny<CancellationToken>()), Times.Once());
        postingFacade.Verify(postingFacade => postingFacade.Update(It.IsAny<Posting>(), It.IsAny<IUnitOfWork>(), It.IsAny<Func<IQueryable<Posting>, IQueryable<Posting>>>(), It.IsAny<CancellationToken>()), Times.Once());
        documentTemplator.Verify(documentTemplator => documentTemplator.ApplyTemplate(It.IsAny<string>(), It.IsAny<Resume>(), It.IsAny<CancellationToken>()), Times.Once());
        documentTemplator.Verify(documentTemplator => documentTemplator.RenderLatex(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once());
        userFacade.Verify(userFacade => userFacade.GetCurrentUserId(It.IsAny<IUnitOfWork>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Once());
        sectionTemplateFacade.Verify(sectionTemplateFacade => sectionTemplateFacade.GetDefaultSection(
            It.IsAny<ResumePart>(), It.IsAny<Guid>(), It.IsAny<IUnitOfWork>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce());
        resumeEnricher.Verify(resumeEnricher => resumeEnricher.EnrichResume(It.IsAny<Resume>(), It.IsAny<Posting>(),
            It.IsAny<IProgress<string>>(), It.IsAny<CancellationToken>()), Times.Once());
        progress.Verify();
        postingFacade.Verify(postingFacade => postingFacade.Add(It.IsAny<Posting>(), It.IsAny<IUnitOfWork>(), It.IsAny<CancellationToken>()), Times.Once());
    }
}
