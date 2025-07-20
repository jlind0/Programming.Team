using DynamicData;
using DynamicData.Binding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Programming.Team.AI.Core;
using Programming.Team.Business.Core;
using Programming.Team.Core;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Programming.Team.ViewModels.Resume
{
    public class PostingsViewModel : ManageEntityViewModel<Guid, Posting>
    {
        public PostingsViewModel(IBusinessRepositoryFacade<Posting, Guid> facade, ILogger<ManageEntityViewModel<Guid, Posting, IBusinessRepositoryFacade<Posting, Guid>>> logger) : base(facade, logger)
        {
        }

        protected override Func<IQueryable<Posting>, IQueryable<Posting>>? PropertiesToLoad()
        {
            return e => e.Include(x => x.DocumentTemplate);
        }
        protected override async Task<Expression<Func<Posting, bool>>?> GetBaseFilterCondition()
        {
            var userId = await Facade.GetCurrentUserId();
            return e => e.UserId == userId;
        }
    }
    public class PostingLoaderViewModel : EntityLoaderViewModel<Guid, Posting, PostingViewModel, IBusinessRepositoryFacade<Posting, Guid>>
    {
        protected IResumeBuilder Builder { get; }
        protected ISectionTemplateBusinessFacade SectionFacade { get; }
        protected IDocumentTemplateBusinessFacade DocumentTemplateFacade { get; }
        protected IChatService ResumeEnricher { get; }
        public PostingLoaderViewModel(IChatService resumeEnricher, IResumeBuilder builder, IDocumentTemplateBusinessFacade docTemplateFacade, 
            ISectionTemplateBusinessFacade sectionFacade, IBusinessRepositoryFacade<Posting, Guid> facade, ILogger<EntityLoaderViewModel<Guid, Posting, PostingViewModel, IBusinessRepositoryFacade<Posting, Guid>>> logger) : base(facade, logger)
        {
            SectionFacade = sectionFacade;
            DocumentTemplateFacade = docTemplateFacade;
            Builder = builder;
            ResumeEnricher = resumeEnricher;
        }
        protected override async Task DoLoad(Guid key, CancellationToken token)
        { 

            var posting = await Facade.GetByID(key, token:token);
            var userID = await Facade.GetCurrentUserId();
            if (posting?.UserId != userID)
                throw new UnauthorizedAccessException();
            await base.DoLoad(key, token);
        }
        protected override PostingViewModel Construct(Posting entity)
        {
            return new PostingViewModel(ResumeEnricher, Builder, 
                new ResumeConfigurationViewModel(SectionFacade, DocumentTemplateFacade),
                new CoverLetterConfigurationViewModel(DocumentTemplateFacade), DocumentTemplateFacade, Logger, Facade, entity);
        }
    }
    public class PostingViewModel : EntityViewModel<Guid, Posting>, IPosting
    {
        protected readonly CompositeDisposable disposables = new CompositeDisposable();
        public ResumeConfigurationViewModel ConfigurationViewModel { get; }
        public CoverLetterConfigurationViewModel CoverLetterConfigurationViewModel { get; }
        public ObservableCollection<DocumentTemplate> DocumentTemplates { get; } = new ObservableCollection<DocumentTemplate>();
        protected IDocumentTemplateBusinessFacade DocumentTemplateFacade { get; }
        protected IResumeBuilder Builder { get; }
        protected IChatService Enricher { get; }
        public ReactiveCommand<Unit, Unit> Rebuild { get; }
        public ReactiveCommand<Unit, Unit> Render { get; }
        public ReactiveCommand<Unit, Unit> GenerateCoverLetter { get; }
        public ReactiveCommand<Unit, Unit> RenderCoverLetter { get; }
        public ReactiveCommand<Unit, Unit> RenderMarkdown { get; }
        public ReactiveCommand<Unit, Unit> ExtractCompanyName { get; }
        public ReactiveCommand<Unit, Unit> ResearchCompany { get; }
        ~PostingViewModel()
        {
            disposables.Dispose();
        }
        public PostingViewModel(IChatService resumeEnricher, IResumeBuilder builder, ResumeConfigurationViewModel config, 
            CoverLetterConfigurationViewModel coverConfig, IDocumentTemplateBusinessFacade documentTemplateFacade, 
            ILogger logger, IBusinessRepositoryFacade<Posting, Guid> facade, Guid id) : base(logger, facade, id)
        {
            ConfigurationViewModel = config;
            DocumentTemplateFacade = documentTemplateFacade;
            Builder = builder;
            Enricher = resumeEnricher;
            Rebuild = ReactiveCommand.CreateFromTask(DoRebuild);
            Render = ReactiveCommand.CreateFromTask(DoRender);
            GenerateCoverLetter = ReactiveCommand.CreateFromTask(DoGenerateCoverLetter);
            RenderCoverLetter = ReactiveCommand.CreateFromTask(DoRenderCoverLetter);
            RenderMarkdown = ReactiveCommand.CreateFromTask(DoRenderMarkdown);
            ExtractCompanyName = ReactiveCommand.CreateFromTask(DoExtractCompanyName);
            ResearchCompany = ReactiveCommand.CreateFromTask(DoResearchCompany);
            CoverLetterConfigurationViewModel = coverConfig;
            WireUpEvents();
        }

        public PostingViewModel(IChatService resumeEnricher, IResumeBuilder builder, ResumeConfigurationViewModel config, CoverLetterConfigurationViewModel coverConfig, IDocumentTemplateBusinessFacade documentTemplateFacade, ILogger logger, IBusinessRepositoryFacade<Posting, Guid> facade, Posting entity) : base(logger, facade, entity)
        {
            ConfigurationViewModel = config;
            DocumentTemplateFacade = documentTemplateFacade;
            Builder = builder;
            Enricher = resumeEnricher;
            Rebuild = ReactiveCommand.CreateFromTask(DoRebuild);
            Render = ReactiveCommand.CreateFromTask(DoRender);
            GenerateCoverLetter = ReactiveCommand.CreateFromTask(DoGenerateCoverLetter);
            RenderCoverLetter = ReactiveCommand.CreateFromTask(DoRenderCoverLetter);
            CoverLetterConfigurationViewModel = coverConfig;
            RenderMarkdown = ReactiveCommand.CreateFromTask(DoRenderMarkdown);
            ExtractCompanyName = ReactiveCommand.CreateFromTask(DoExtractCompanyName);
            ResearchCompany = ReactiveCommand.CreateFromTask(DoResearchCompany);
            WireUpEvents();
        }
        private bool isProcessing = false;
        public bool IsProcessing
        {
            get => isProcessing;
            set => this.RaiseAndSetIfChanged(ref isProcessing, value);
        }
        public bool CanResearchCompany
        {
            get => !string.IsNullOrWhiteSpace(CompanyName);
        }
        protected async Task DoExtractCompanyName(CancellationToken token)
        {
            try
            {
                IsProcessing = true;
                CompanyName = await Enricher.ExtractCompanyName(Details, token: token);
                await Update.Execute().GetAwaiter();
            }
            catch(Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                await Alert.Handle(ex.Message).GetAwaiter();
            }
            finally
            {
                IsProcessing = false;
            }
        }
        protected async Task DoResearchCompany(CancellationToken token)
        {
            try
            {
                if(!CanResearchCompany)
                    throw new InvalidDataException("No Company Name Set");
                IsProcessing = true;
                CompanyResearch = await Enricher.ResearchCompany(CompanyName ?? throw new InvalidDataException("No Company Name Set"), token: token);
                await Update.Execute().GetAwaiter();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                await Alert.Handle(ex.Message).GetAwaiter();
            }
            finally
            {
                IsProcessing = false;
            }
        }
        protected async Task DoRenderMarkdown(CancellationToken token)
        {
            try
            {
                if (CanRenderMarkdown)
                {
                    await Builder.RenderMarkdown(await Populate(), MarkdownTemplateId!.Value, token);
                    await Load.Execute().GetAwaiter();
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                await Alert.Handle(ex.Message).GetAwaiter();
            }
        }
        protected async Task DoRenderCoverLetter(CancellationToken token)
        {
            try
            {
                Progress<string> prog = new Progress<string>(str =>
                {
                    Progress = str;
                });
                await Update.Execute().GetAwaiter();
                await Builder.RenderCoverLetter(await Populate(), token);
                await Alert.Handle("Cover Letter Rendered!").GetAwaiter();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                await Alert.Handle(ex.Message).GetAwaiter();
            }
            finally
            {
                Progress = null;
            }
        }
        protected async Task DoGenerateCoverLetter(CancellationToken token)
        {
            try
            {
                Progress<string> prog = new Progress<string>(str =>
                {
                    Progress = str;
                });
                await Update.Execute().GetAwaiter();
                await Builder.BuildCoverLetter(await Populate(),
                    CoverLetterConfigurationViewModel.DocumentTemplateId ??
                    throw new InvalidDataException("No Document Template Selected"), prog, token: token);
                Progress = null;
                await Load.Execute().GetAwaiter();
                await Alert.Handle("Cover Letter Generated!").GetAwaiter();

            }
            catch(Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                await Alert.Handle(ex.Message).GetAwaiter();
            }
            finally
            {
                Progress = null;
            }
        }
        protected async Task DoRebuild(CancellationToken token)
        {
            try
            {
                Progress<string> prog = new Progress<string>(str =>
                {
                    Progress = str;
                });
                await Update.Execute().GetAwaiter();
                var userId = await Facade.GetCurrentUserId();
                if (userId == null)
                    throw new InvalidDataException();
                await Builder.RebuildPosting(await Populate(), await Builder.BuildResume(userId.Value, prog, token), Enrich, RenderPDF, prog, ConfigurationViewModel.GetConfiguration(), token);
                Progress = null;
                await Load.Execute().GetAwaiter();
                await Alert.Handle("Resume Rebuilt!").GetAwaiter();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                await Alert.Handle(ex.Message).GetAwaiter();
            }
            finally
            {
                Progress = null;
            }
        }
        protected async Task DoRender(CancellationToken token)
        {
            try
            {
                await Builder.RenderResume(await Populate(), token);
                await Alert.Handle("Resume Rendered!").GetAwaiter();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                await Alert.Handle(ex.Message).GetAwaiter();
            }
        }
        protected void WireUpEvents()
        {
            this.WhenPropertyChanged(p => p.SelectedTemplate).Subscribe(p =>
            {
                if (p.Sender.SelectedTemplate != null)
                    p.Sender.DocumentTemplateId = p.Sender.SelectedTemplate.Id;
            }).DisposeWith(disposables);
            this.WhenPropertyChanged(p => p.SelectedMarkdownTemplate).Subscribe(p =>
            {
                if (p.Sender.SelectedMarkdownTemplate != null)
                    p.Sender.MarkdownTemplateId = p.Sender.SelectedMarkdownTemplate.Id;
                else
                    p.Sender.MarkdownTemplateId = null;
                this.RaisePropertyChanged(nameof(CanRenderMarkdown));
            }).DisposeWith(disposables);
        }
        private bool enrich = true;
        public bool Enrich
        {
            get => enrich;
            set => this.RaiseAndSetIfChanged(ref enrich, value);
        }
        private bool renderPDF = true;
        public bool RenderPDF
        {
            get => renderPDF;
            set => this.RaiseAndSetIfChanged(ref renderPDF, value);
        }
        private DocumentTemplate? selectedTemplate;
        public DocumentTemplate? SelectedTemplate
        {
            get => selectedTemplate;
            set => this.RaiseAndSetIfChanged(ref selectedTemplate, value);
        }
        private Guid documentTemplateId;
        public Guid DocumentTemplateId
        {
            get => documentTemplateId;
            set => this.RaiseAndSetIfChanged(ref documentTemplateId, value);
        }
        private Guid? markdownTemplateId;
        public Guid? MarkdownTemplateId
        {
            get => markdownTemplateId;
            set => this.RaiseAndSetIfChanged(ref markdownTemplateId, value);
        }
        private DocumentTemplate? selectedMarkdownTemplate;
        public DocumentTemplate? SelectedMarkdownTemplate
        {
            get => selectedMarkdownTemplate;
            set => this.RaiseAndSetIfChanged(ref selectedMarkdownTemplate, value);
        }
        private string name = string.Empty;
        public string Name
        {
            get => name;
            set => this.RaiseAndSetIfChanged(ref name, value);
        }

        private string details = string.Empty;
        public string Details
        {
            get => details;
            set => this.RaiseAndSetIfChanged(ref details, value);
        }

        private string? renderedLaTex;
        public string? RenderedLaTex
        {
            get => renderedLaTex;
            set => this.RaiseAndSetIfChanged(ref renderedLaTex, value);
        }

        private string? configuration;
        public string? Configuration
        {
            get => configuration;
            set => this.RaiseAndSetIfChanged(ref configuration, value);
        }
        private string? progress;
        public string? Progress
        {
            get => progress;
            set
            {
                this.RaiseAndSetIfChanged(ref progress, value);
                this.RaisePropertyChanged(nameof(IsOverlayOpen));
            }
        }
        public bool CanRenderMarkdown
        {
            get => !string.IsNullOrWhiteSpace(ResumeJson) && this.SelectedMarkdownTemplate != null;
        }
        public bool IsOverlayOpen
        {
            get => Progress != null;
            set { }
        }
        public Guid UserId { get; set; }
        private string? coverLetterLaTeX;
        public string? CoverLetterLaTeX
        {
            get => coverLetterLaTeX;
            set => this.RaiseAndSetIfChanged(ref coverLetterLaTeX, value);
        }
        private string? coverLetterConfiguration;
        public string? CoverLetterConfiguration
        {
            get => coverLetterConfiguration;
            set => this.RaiseAndSetIfChanged(ref coverLetterConfiguration, value);
        }
        private string? resumeJson;
        public string? ResumeJson
        {
            get => resumeJson;
            set
            {
                this.RaiseAndSetIfChanged(ref resumeJson, value);
                this.RaisePropertyChanged(nameof(CanRenderMarkdown));
            }
        }
        private string? resumeMarkdown;
        public string? ResumeMarkdown
        {
            get => resumeMarkdown;
            set => this.RaiseAndSetIfChanged(ref resumeMarkdown, value);
        }
        public ObservableCollection<DocumentTemplate> MarkdownTemplates { get; } = new ObservableCollection<DocumentTemplate>();
        private string? companyName;
        public string? CompanyName
        {
            get => companyName;
            set
            {
                this.RaiseAndSetIfChanged(ref companyName, value);
                this.RaisePropertyChanging(nameof(CanResearchCompany));
            }
        }
        private string? companyResearch;
        public string? CompanyResearch
        {
            get => companyResearch;
            set => this.RaiseAndSetIfChanged(ref companyResearch, value);
        }

        protected override async Task<Posting?> DoLoad(CancellationToken token)
        {
            try
            {
                var userId = await Facade.GetCurrentUserId(fetchTrueUserId: true, token: token);
                if (userId == null)
                    throw new InvalidOperationException();
                DocumentTemplates.Clear();
                var dts = await DocumentTemplateFacade.GetForUser(userId.Value, DocumentTypes.Resume, token: token);
                DocumentTemplates.AddRange(dts);
                SelectedTemplate = dts.FirstOrDefault();
                MarkdownTemplates.Clear();
                var mds = await DocumentTemplateFacade.GetForUser(userId.Value, DocumentTypes.MarkdownResume, token: token);
                MarkdownTemplates.AddRange(mds);
                SelectedMarkdownTemplate = mds.FirstOrDefault();

            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                await Alert.Handle(ex.Message).GetAwaiter();
            }
            return await base.DoLoad(token);
        }
        internal override Task<Posting> Populate()
        {
            return Task.FromResult(new Posting()
            {
                Id = Id,
                Name = Name,
                Details = Details,
                RenderedLaTex = RenderedLaTex,
                DocumentTemplateId = DocumentTemplateId,
                Configuration = ConfigurationViewModel.GetSerializedConfiguration(),
                CoverLetterConfiguration = CoverLetterConfigurationViewModel.GetSerializedConfiguration(),
                CoverLetterLaTeX = CoverLetterLaTeX,
                ResumeJson = ResumeJson,
                ResumeMarkdown = ResumeMarkdown,
                UserId = UserId,
                CompanyName = CompanyName,
                CompanyResearch = CompanyResearch,
            });
        }

        internal override async Task Read(Posting entity)
        {
            Id = entity.Id;
            Name = entity.Name;
            Details = entity.Details;
            Configuration = entity.Configuration;
            RenderedLaTex = entity.RenderedLaTex;
            CoverLetterLaTeX = entity.CoverLetterLaTeX;
            SelectedTemplate = DocumentTemplates.SingleOrDefault(d => d.Id == entity.DocumentTemplateId);
            UserId = entity.UserId;
            ResumeJson = entity.ResumeJson;
            ResumeMarkdown = entity.ResumeMarkdown;
            CompanyName = entity.CompanyName;
            CompanyResearch = entity.CompanyResearch;
            await ConfigurationViewModel.Load(entity.Configuration);
            await CoverLetterConfigurationViewModel.Load(entity.CoverLetterConfiguration);
        }
    }
    public class ResumePartViewModel : ReactiveObject
    {
        public ObservableCollection<SectionTemplate> SectionTemplates { get; } = new ObservableCollection<SectionTemplate>();
        private SectionTemplate? selectedTemplate;
        public SectionTemplate? SelectedTemplate
        {
            get => selectedTemplate;
            set => this.RaiseAndSetIfChanged(ref selectedTemplate, value);
        }
        public ResumePartViewModel(ResumePart part, int order, bool selected, SectionTemplate[] templates, Guid? selectedTemplateId)
        {
            Part = part;
            Selected = selected;
            Order = order;
            SectionTemplates.AddRange(templates);
            SelectedTemplate = templates.SingleOrDefault(p => p.Id == selectedTemplateId);
        }
        private int order;
        public int Order
        {
            get => order;
            set
            {
                this.RaiseAndSetIfChanged(ref order, value);
            }
        }
        private bool selected;
        public bool Selected
        {
            get => selected;
            set => this.RaiseAndSetIfChanged(ref selected, value);
        }
        public ResumePart Part{ get; }
    }
    public class CoverLetterConfigurationViewModel : ReactiveObject, ICoverLetterConfiguration
    {
        private bool isLoaded = false;
        public bool IsLoaded
        {
            get => isLoaded;
            set => this.RaiseAndSetIfChanged(ref isLoaded, value);
        }
        protected IDocumentTemplateBusinessFacade DocumentTemplateFacade { get; }
        private int? targetLength;
        public int? TargetLength
        {
            get => targetLength;
            set => this.RaiseAndSetIfChanged(ref targetLength, value);
        }
        private Guid? defaultDocumentTemplateId;
        public Guid? DocumentTemplateId 
        {
            get => defaultDocumentTemplateId;
            set => this.RaiseAndSetIfChanged(ref defaultDocumentTemplateId, value);
        }
        private int? numberOfBullets;
        public int? NumberOfBullets
        {
            get => numberOfBullets;
            set => this.RaiseAndSetIfChanged(ref numberOfBullets, value);
        }
        public ObservableCollection<DocumentTemplate> DocumentTemplates { get; } = new ObservableCollection<DocumentTemplate>();
        private DocumentTemplate? selectedTemplate;
        public DocumentTemplate? SelectedTemplate
        {
            get => selectedTemplate;
            set => this.RaiseAndSetIfChanged(ref selectedTemplate, value);
        }
        private readonly CompositeDisposable disposables = new CompositeDisposable();
        public CoverLetterConfigurationViewModel(IDocumentTemplateBusinessFacade docTemplateFacade)
        {
            DocumentTemplateFacade = docTemplateFacade;
            this.WhenPropertyChanged(p => p.SelectedTemplate).Subscribe(p =>
            {
                DocumentTemplateId = p.Value?.Id;
            }).DisposeWith(disposables);
        }
        ~CoverLetterConfigurationViewModel()
        {
            disposables.Dispose();
        }
        public async Task Load(string? configuration)
        {
            IsLoaded = false;
            var config = configuration != null ? JsonSerializer.Deserialize<CoverLetterConfiguration>(configuration) ?? new CoverLetterConfiguration() : new CoverLetterConfiguration();
            TargetLength = config.TargetLength;
            NumberOfBullets = config.NumberOfBullets;
            DocumentTemplates.Clear();
            DocumentTemplates.AddRange(await DocumentTemplateFacade.GetForUser((await DocumentTemplateFacade.GetCurrentUserId(fetchTrueUserId: true))!.Value, DocumentTypes.CoverLetter));
            SelectedTemplate = DocumentTemplates.SingleOrDefault(dt => dt.Id == (config.DocumentTemplateId ?? DocumentTemplates.FirstOrDefault()?.Id));
            IsLoaded = true;
        }
        public CoverLetterConfiguration GetConfiguration()
        {
            var config = new CoverLetterConfiguration()
            {
                TargetLength = TargetLength,
                DocumentTemplateId = DocumentTemplateId,
                NumberOfBullets = NumberOfBullets
            };
            return config;
        }
        public string GetSerializedConfiguration()
        {
            return JsonSerializer.Serialize(GetConfiguration());
        }
    }
    public class ResumeConfigurationViewModel : ReactiveObject, IResumeConfiguration
    {
        private bool isLoaded = false;
        public bool IsLoaded
        {
            get => isLoaded;
            set => this.RaiseAndSetIfChanged(ref isLoaded, value);
        }
        protected ISectionTemplateBusinessFacade Facade { get; }
        protected IDocumentTemplateBusinessFacade DocumentTemplateFacade { get; }
        private readonly CompositeDisposable disposables = new CompositeDisposable();
        public ResumeConfigurationViewModel(ISectionTemplateBusinessFacade facade, IDocumentTemplateBusinessFacade docTemplateFacade)
        {
            Facade = facade;
            DocumentTemplateFacade = docTemplateFacade;
            this.WhenPropertyChanged(p => p.SelectedTemplate).Subscribe(async p =>
            {
                DefaultDocumentTemplateId = p.Value?.Id;
                await LoadSectionTemplates();
            }).DisposeWith(disposables);
        }
        ~ResumeConfigurationViewModel()
        {
            disposables.Dispose();
        }
        private readonly object lockObject = new object();
        protected async Task LoadSectionTemplates()
        {
            List<ResumePartViewModel> parts = [];
            if (DefaultDocumentTemplateId != null)
            {
                foreach (var part in Enum.GetValues<ResumePart>())
                {
                    SectionTemplates.TryGetValue(part, out var selectedId);
                    var pv = new ResumePartViewModel(part,
                        Parts.Contains(part) ? Array.IndexOf(Parts, part) : int.MaxValue, Parts.Contains(part),
                        await Facade.GetBySection(part, DefaultDocumentTemplateId.Value), selectedId);
                    parts.Add(pv);
                }
            }
            lock (lockObject)
            {
                IsLoaded = false;
                ResumeParts.Clear();
                foreach (var pv in parts.OrderBy(p => p.Order))
                {
                    ResumeParts.Add(pv);
                }
                IsLoaded = true;
            }
        }
        public async Task Load(string? configuration)
        {
            IsLoaded = false;
            var config = configuration != null ? JsonSerializer.Deserialize<ResumeConfiguration>(configuration) ?? new ResumeConfiguration() : new ResumeConfiguration();
            MatchThreshold = config.MatchThreshold;
            TargetLengthPer10Percent = config.TargetLengthPer10Percent;
            HideSkillsNotInJD = config.HideSkillsNotInJD;
            BulletsPer20Percent = config.BulletsPer20Percent;
            HidePositionsNotInJD = config.HidePositionsNotInJD;
            SkillsPer20Percent = config.SkillsPer20Percent;
            BioBullets = config.BioBullets;
            BioParagraphs = config.BioParagraphs;
            Parts = config.Parts;
            SectionTemplates = config.SectionTemplates;
            DocumentTemplates.Clear();
            DocumentTemplates.AddRange(await DocumentTemplateFacade.GetForUser((await Facade.GetCurrentUserId(fetchTrueUserId: true))!.Value));
            SelectedTemplate = DocumentTemplates.SingleOrDefault(dt => dt.Id == (config.DefaultDocumentTemplateId ?? DocumentTemplates.FirstOrDefault()?.Id));
            await LoadSectionTemplates();
        }
        public ResumeConfiguration GetConfiguration()
        {
            var config = new ResumeConfiguration()
            {
                MatchThreshold = MatchThreshold,
                TargetLengthPer10Percent = TargetLengthPer10Percent,
                HideSkillsNotInJD = HideSkillsNotInJD,
                BulletsPer20Percent = BulletsPer20Percent,
                HidePositionsNotInJD = HidePositionsNotInJD,
                Parts = ResumeParts.Where(p => p.Selected).OrderBy(p => p.Order).Select(p => p.Part).ToArray(),
                SectionTemplates = ResumeParts.Where(p => p.Selected).ToDictionary(p => p.Part, p => p.SelectedTemplate?.Id),
                SkillsPer20Percent = SkillsPer20Percent,
                DefaultDocumentTemplateId = DefaultDocumentTemplateId,
                BioBullets = BioBullets,
                BioParagraphs = BioParagraphs
            };
            return config;
        }
        public string GetSerializedConfiguration()
        {
            return JsonSerializer.Serialize(GetConfiguration());
        }
        private double? matchThreshold;
        public double? MatchThreshold
        {
            get => matchThreshold;
            set => this.RaiseAndSetIfChanged(ref matchThreshold, value);
        }

        private int? targetLengthPer10Percent;
        public int? TargetLengthPer10Percent
        {
            get => targetLengthPer10Percent;
            set => this.RaiseAndSetIfChanged(ref targetLengthPer10Percent, value);
        }

        private bool hideSkillsNotInJD = true;
        public bool HideSkillsNotInJD
        {
            get => hideSkillsNotInJD;
            set => this.RaiseAndSetIfChanged(ref hideSkillsNotInJD, value);
        }
        private double? bulletsPer20Percent;
        public double? BulletsPer20Percent
        {
            get => bulletsPer20Percent;
            set => this.RaiseAndSetIfChanged(ref  bulletsPer20Percent, value);
        }
        private bool hidePositionsNotInJD = false;
        public bool HidePositionsNotInJD
        {
            get => hidePositionsNotInJD; 
            set => this.RaiseAndSetIfChanged(ref hidePositionsNotInJD, value);
        }
        public ObservableCollection<ResumePartViewModel> ResumeParts { get; } = new ObservableCollection<ResumePartViewModel>();
        public ObservableCollection<DocumentTemplate> DocumentTemplates { get; } = new ObservableCollection<DocumentTemplate>();
        public ResumePart[] Parts { get; set; } = [ResumePart.Bio, ResumePart.Recommendations, ResumePart.Skills, ResumePart.Positions, ResumePart.Education, ResumePart.Certifications, ResumePart.Publications];
        public Dictionary<ResumePart, Guid?> SectionTemplates { get; set; } = [];
        private double? skillsPer20Percent;
        public double? SkillsPer20Percent 
        {
            get => skillsPer20Percent;
            set => this.RaiseAndSetIfChanged(ref skillsPer20Percent, value);
        }
        private Guid? defaultDocumentTemplateId;
        public Guid? DefaultDocumentTemplateId {
            get => defaultDocumentTemplateId;
            set => this.RaiseAndSetIfChanged(ref defaultDocumentTemplateId, value);
        }
        private DocumentTemplate? selectedTemplate;
        public DocumentTemplate? SelectedTemplate
        {
            get => selectedTemplate;
            set => this.RaiseAndSetIfChanged(ref selectedTemplate, value);
        }
        private int? bioParagraphs;
        public int? BioParagraphs
        {
            get => bioParagraphs;
            set => this.RaiseAndSetIfChanged(ref bioParagraphs, value);
        }
        private int? bioBullets;
        public int? BioBullets
        {
            get => bioBullets;
            set => this.RaiseAndSetIfChanged(ref bioBullets, value);
        }
    }
}
