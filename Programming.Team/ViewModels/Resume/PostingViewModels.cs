using DynamicData;
using DynamicData.Binding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
        protected IBusinessRepositoryFacade<DocumentTemplate, Guid> DocumentTemplateFacade { get; }
        protected IResumeBuilder Builder { get; }
        protected ISectionTemplateBusinessFacade SectionFacade { get; }
        public PostingLoaderViewModel(IResumeBuilder builder, ISectionTemplateBusinessFacade sectionFacade, IBusinessRepositoryFacade<DocumentTemplate, Guid> documentTemplateFacade, IBusinessRepositoryFacade<Posting, Guid> facade, ILogger<EntityLoaderViewModel<Guid, Posting, PostingViewModel, IBusinessRepositoryFacade<Posting, Guid>>> logger) : base(facade, logger)
        {
            SectionFacade = sectionFacade;
            DocumentTemplateFacade = documentTemplateFacade;
            Builder = builder;
        }
        protected override async Task DoLoad(Guid key, CancellationToken token)
        {

            var posting = await Facade.GetByID(key, token: token);
            var userID = await Facade.GetCurrentUserId();
            if (posting?.UserId != userID)
                throw new UnauthorizedAccessException();
            await base.DoLoad(key, token);
        }
        protected override PostingViewModel Construct(Posting entity)
        {
            return new PostingViewModel(Builder, new ResumeConfigurationViewModel(SectionFacade), DocumentTemplateFacade, Logger, Facade, entity);
        }
    }
    public class PostingViewModel : EntityViewModel<Guid, Posting>, IPosting
    {
        protected readonly CompositeDisposable disposables = new CompositeDisposable();
        public ResumeConfigurationViewModel ConfigurationViewModel { get; }
        public ObservableCollection<DocumentTemplate> DocumentTemplates { get; } = new ObservableCollection<DocumentTemplate>();
        protected IBusinessRepositoryFacade<DocumentTemplate, Guid> DocumentTemplateFacade { get; }
        protected IResumeBuilder Builder { get; }
        public ReactiveCommand<Unit, Unit> Rebuild { get; }
        public ReactiveCommand<Unit, Unit> Render { get; }
        ~PostingViewModel()
        {
            disposables.Dispose();
        }
        public PostingViewModel(IResumeBuilder builder, ResumeConfigurationViewModel config, IBusinessRepositoryFacade<DocumentTemplate, Guid> documentTemplateFacade, ILogger logger, IBusinessRepositoryFacade<Posting, Guid> facade, Guid id) : base(logger, facade, id)
        {
            ConfigurationViewModel = config;
            DocumentTemplateFacade = documentTemplateFacade;
            Builder = builder;
            Rebuild = ReactiveCommand.CreateFromTask(DoRebuild);
            Render = ReactiveCommand.CreateFromTask(DoRender);
            WireUpEvents();
        }

        public PostingViewModel(IResumeBuilder builder, ResumeConfigurationViewModel config, IBusinessRepositoryFacade<DocumentTemplate, Guid> documentTemplateFacade, ILogger logger, IBusinessRepositoryFacade<Posting, Guid> facade, Posting entity) : base(logger, facade, entity)
        {
            ConfigurationViewModel = config;
            DocumentTemplateFacade = documentTemplateFacade;
            Builder = builder;
            Rebuild = ReactiveCommand.CreateFromTask(DoRebuild);
            Render = ReactiveCommand.CreateFromTask(DoRender);
            WireUpEvents();
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
        public bool IsOverlayOpen
        {
            get => Progress != null;
            set { }
        }
        public Guid UserId { get; set; }
        protected override async Task<Posting?> DoLoad(CancellationToken token)
        {
            try
            {
                DocumentTemplates.Clear();
                var dts = await DocumentTemplateFacade.Get(orderBy: o => o.OrderBy(e => e.Name), token: token);
                DocumentTemplates.AddRange(dts.Entities);

            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                await Alert.Handle(ex.Message).GetAwaiter();
            }
            return await base.DoLoad(token);
        }
        protected override Task<Posting> Populate()
        {
            return Task.FromResult(new Posting()
            {
                Id = Id,
                Name = Name,
                Details = Details,
                RenderedLaTex = RenderedLaTex,
                DocumentTemplateId = DocumentTemplateId,
                Configuration = ConfigurationViewModel.GetSerializedConfiguration(),
                UserId = UserId
            });
        }

        protected override async Task Read(Posting entity)
        {
            Id = entity.Id;
            Name = entity.Name;
            Details = entity.Details;
            Configuration = entity.Configuration;
            RenderedLaTex = entity.RenderedLaTex;
            SelectedTemplate = DocumentTemplates.SingleOrDefault(d => d.Id == entity.DocumentTemplateId);
            UserId = entity.UserId;
            await ConfigurationViewModel.Load(entity.Configuration);
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
        private Guid? InitalSelectedTemplateId { get; }
        public ResumePartViewModel(ResumePart part, int order, bool selected, SectionTemplate[] templates, Guid? selectedTemplateId)
        {
            Part = part;
            Selected = selected;
            Order = order;
            InitalSelectedTemplateId = selectedTemplateId;
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
        public ResumePart Part { get; }
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
        public ResumeConfigurationViewModel(ISectionTemplateBusinessFacade facade)
        {
            Facade = facade;
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
            Parts = config.Parts;
            SectionTemplates = config.SectionTemplates;
            ResumeParts.Clear();
            List<ResumePartViewModel> parts = [];
            foreach (var part in Enum.GetValues<ResumePart>())
            {
                SectionTemplates.TryGetValue(part, out var selectedId);
                var pv = new ResumePartViewModel(part,
                    Parts.Contains(part) ? Array.IndexOf(Parts, part) : int.MaxValue, Parts.Contains(part),
                    await Facade.GetBySection(part), selectedId);
                parts.Add(pv);
            }
            foreach (var pv in parts.OrderBy(p => p.Order))
            {
                ResumeParts.Add(pv);
            }
            IsLoaded = true;
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
                SkillsPer20Percent = SkillsPer20Percent
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
            set => this.RaiseAndSetIfChanged(ref bulletsPer20Percent, value);
        }
        private bool hidePositionsNotInJD = false;
        public bool HidePositionsNotInJD
        {
            get => hidePositionsNotInJD;
            set => this.RaiseAndSetIfChanged(ref hidePositionsNotInJD, value);
        }
        public ObservableCollection<ResumePartViewModel> ResumeParts { get; } = new ObservableCollection<ResumePartViewModel>();
        public ResumePart[] Parts { get; set; } = [ResumePart.Bio, ResumePart.Reccomendations, ResumePart.Skills, ResumePart.Positions, ResumePart.Education, ResumePart.Certifications, ResumePart.Publications];
        public Dictionary<ResumePart, Guid?> SectionTemplates { get; set; } = [];
        private double? skillsPer20Percent;
        public double? SkillsPer20Percent
        {
            get => skillsPer20Percent;
            set => this.RaiseAndSetIfChanged(ref skillsPer20Percent, value);
        }
    }
}
