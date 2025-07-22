using DynamicData.Binding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Programming.Team.Business.Core;
using Programming.Team.Core;
using Programming.Team.Templating.Core;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Programming.Team.ViewModels.Resume
{
    public class AddPositionViewModel : AddUserPartionedEntity<Guid, Position>, IPosition, ITextual
    {
        public SearchSelectCompanyViewModel CompanyViewModel { get; }
        public SmartTextEditorViewModel<AddPositionViewModel> SmartText { get; }
        protected readonly CompositeDisposable disposable = new CompositeDisposable();
        public AddPositionViewModel(IBusinessRepositoryFacade<Position, Guid> facade, IDocumentTemplator templator, IConfiguration config,
            ILogger<AddEntityViewModel<Guid, Position, IBusinessRepositoryFacade<Position, Guid>>> logger,
            SearchSelectCompanyViewModel companyViewModel) : base(facade, logger)
        {
            CompanyViewModel = companyViewModel;
            SmartText = new SmartTextEditorViewModel<AddPositionViewModel>(this, logger, templator, config);
            CompanyViewModel.WhenPropertyChanged(p => p.Selected).Subscribe(p =>
            {
                if (p.Sender != null && p.Sender.Selected != null)
                    CompanyId = p.Sender.Selected.Id;
                else
                    CompanyId = Guid.Empty;
            }).DisposeWith(disposable);
        }
        private TextType textTypeId = TextType.Text;
        public TextType TextTypeId
        {
            get => textTypeId;
            set => this.RaiseAndSetIfChanged(ref textTypeId, value);
        }
        public string? Text
        {
            get => Description;
            set => Description = value;
        }
        private Guid companyId;
        [Required]
        public Guid CompanyId
        {
            get => companyId;
            set
            {
                this.RaiseAndSetIfChanged(ref companyId, value);
                this.RaisePropertyChanged(nameof(CanAdd));
            }
        }
        public override bool CanAdd => CompanyViewModel.Selected != null;
        private DateOnly startDate;
        [Required]
        public DateOnly StartDate
        {
            get => startDate;
            set
            {
                this.RaiseAndSetIfChanged(ref startDate, value);
                this.RaisePropertyChanged(nameof(StartDateTime));
            }
        }
        public DateTime? StartDateTime
        {
            get => StartDate.ToDateTime(TimeOnly.MinValue);
            set
            {
                StartDate = DateOnly.FromDateTime(value ?? DateTime.Today);
            }
        }
        private DateOnly? endDate;
        public DateOnly? EndDate
        {
            get => endDate;
            set
            {
                this.RaiseAndSetIfChanged(ref endDate, value);
                this.RaisePropertyChanged(nameof(EndDateTime));
            }
        }
        public DateTime? EndDateTime
        {
            get => EndDate?.ToDateTime(TimeOnly.MinValue);
            set
            {
                if (value != null)
                    EndDate = DateOnly.FromDateTime(value.Value);
                else EndDate = null;
            }

        }
        private string? title;
        public string? Title
        {
            get => title;
            set => this.RaiseAndSetIfChanged(ref title, value);
        }

        private string? description;
        public string? Description
        {
            get => description;
            set => this.RaiseAndSetIfChanged(ref description, value);
        }

        private string? sortOrder;
        public string? SortOrder
        {
            get => sortOrder;
            set => this.RaiseAndSetIfChanged(ref sortOrder, value);
        }


        protected override Task Clear()
        {
            CompanyId = Guid.Empty;
            StartDate = DateOnly.FromDateTime(DateTime.Now);
            EndDate = null;
            Title = null;
            Description = null;
            SortOrder = null;
            CompanyViewModel.Selected = null;
            TextTypeId = TextType.Text;
            return Task.CompletedTask;
        }

        protected override Task<Position> ConstructEntity()
        {
            return Task.FromResult(new Position()
            {
                CompanyId = CompanyId,
                StartDate = StartDate,
                EndDate = EndDate,
                Title = Title,
                Description = Description,
                SortOrder = SortOrder,
                UserId = UserId,
                TextTypeId = TextTypeId
            });
        }
        ~AddPositionViewModel()
        {
            disposable.Dispose();
        }
    }
    public class PositionsViewModel : EntitiesDefaultViewModel<Guid, Position, PositionViewModel, AddPositionViewModel>
    {
        protected IServiceProvider ServiceProvider { get; }
        protected IDocumentTemplator Templator { get; }
        protected IConfiguration Config { get; }
        public PositionsViewModel(AddPositionViewModel addViewModel, IDocumentTemplator templator, IConfiguration config,
                IBusinessRepositoryFacade<Position, Guid> facade,
                ILogger<EntitiesViewModel<Guid, Position, PositionViewModel, IBusinessRepositoryFacade<Position, Guid>>> logger,
                IServiceProvider serviceProvider) : base(addViewModel, facade, logger)
        {
            Config= config;
            ServiceProvider = serviceProvider;
            Templator = templator;
        }
        protected override Func<IQueryable<Position>, IQueryable<Position>>? PropertiesToLoad()
        {
            return x => x.Include(e => e.Company);
        }
        protected override Func<IQueryable<Position>, IOrderedQueryable<Position>>? OrderBy()
        {
            return e => e.OrderByDescending(c => c.EndDate ?? DateOnly.MaxValue).ThenByDescending(c => c.SortOrder).ThenByDescending(c => c.StartDate);
        }
        protected override async Task<Expression<Func<Position, bool>>?> FilterCondition()
        {
            var userId = await Facade.GetCurrentUserId();
            return e => e.UserId == userId;
        }
        protected override Task<PositionViewModel> Construct(Position entity, CancellationToken token)
        {
            var vm = new PositionViewModel(Logger, Templator, Config, Facade,
                ServiceProvider.GetRequiredService<PositionSkillsViewModel>(), entity);

            return Task.FromResult(vm);
        }
    }
    public class PositionViewModel : EntityViewModel<Guid, Position>, IPosition, ITextual
    {
        private TextType textTypeId = TextType.Text;
        public TextType TextTypeId
        {
            get => textTypeId;
            set
            {
                this.RaiseAndSetIfChanged(ref textTypeId, value);
            }
        }
        public string? Text
        {
            get => Description;
            set => Description = value;
        }
        private Company? company;
        public Company? Company
        {
            get => company;
            set => this.RaiseAndSetIfChanged(ref company, value);
        }
        private Guid companyId;
        [Required]
        public Guid CompanyId
        {
            get => companyId;
            set => this.RaiseAndSetIfChanged(ref companyId, value);
        }

        private DateOnly startDate;
        [Required]
        public DateOnly StartDate
        {
            get => startDate;
            set
            {
                this.RaiseAndSetIfChanged(ref startDate, value);
                this.RaisePropertyChanged(nameof(StartDateTime));
            }
        }
        public DateTime? StartDateTime
        {
            get => StartDate.ToDateTime(TimeOnly.MinValue);
            set
            {
                StartDate = DateOnly.FromDateTime(value ?? DateTime.Today);
            }
        }
        private DateOnly? endDate;
        public DateOnly? EndDate
        {
            get => endDate;
            set
            {
                this.RaiseAndSetIfChanged(ref endDate, value);
                this.RaisePropertyChanged(nameof(EndDateTime));
            }
        }
        public DateTime? EndDateTime
        {
            get => EndDate?.ToDateTime(TimeOnly.MinValue);
            set
            {
                if (value != null)
                    EndDate = DateOnly.FromDateTime(value.Value);
                else EndDate = null;
            }

        }
        private string? title;
        public string? Title
        {
            get => title;
            set => this.RaiseAndSetIfChanged(ref title, value);
        }

        private string? description;
        public string? Description
        {
            get => description;
            set
            {
                bool isChanged = description != value;
                this.RaiseAndSetIfChanged(ref description, value);
                if(isChanged)
                    this.RaisePropertyChanged(nameof(Text));
            }
        }

        private string? sortOrder;
        public string? SortOrder
        {
            get => sortOrder;
            set => this.RaiseAndSetIfChanged(ref sortOrder, value);
        }
        private Guid userId;
        public Guid UserId
        {
            get => userId;
            set => this.RaiseAndSetIfChanged(ref userId, value);
        }
        protected readonly CompositeDisposable disposable = new CompositeDisposable();
        public PositionSkillsViewModel SkillsViewModel { get; }
        public SmartTextEditorViewModel<PositionViewModel> SmartText { get; }
        public PositionViewModel(ILogger logger, IDocumentTemplator templator, IConfiguration config, IBusinessRepositoryFacade<Position, Guid> facade, PositionSkillsViewModel skillsViewModel, Guid id) : base(logger, facade, id)
        {
            SkillsViewModel = skillsViewModel;
            SmartText = new SmartTextEditorViewModel<PositionViewModel>(this, logger, templator, config);
            WireupSkillsVM();
        }

        public PositionViewModel(ILogger logger, IDocumentTemplator templator, IConfiguration config, IBusinessRepositoryFacade<Position, Guid> facade, PositionSkillsViewModel skillsViewModel, Position entity) : base(logger, facade, entity)
        {
            SkillsViewModel = skillsViewModel;
            SmartText = new SmartTextEditorViewModel<PositionViewModel>(this, logger, templator, config);
            WireupSkillsVM();
        }
        protected void WireupSkillsVM()
        {
            SkillsViewModel.WhenPropertyChanged(p => p.Description).Subscribe(p =>
            {
                if (p.Sender != null)
                    SkillsViewModel.Description = p.Sender.Description ?? "";
            }).DisposeWith(disposable);

            this.WhenPropertyChanged(p => p.Description).Subscribe(p => {
                SkillsViewModel.CanExtractSkills =
                !string.IsNullOrWhiteSpace(p.Sender.Description);
            }).DisposeWith(disposable);
        }
        ~PositionViewModel()
        {
            disposable.Dispose();
        }
        protected override Func<IQueryable<Position>, IQueryable<Position>>? PropertiesToLoad()
        {
            return e => e.Include(x => x.Company).Include(x => x.PositionSkills).ThenInclude(c => c.Skill);
        }
        internal override Task<Position> Populate()
        {
            return Task.FromResult(new Position()
            {
                Id = Id,
                CompanyId = CompanyId,
                UserId = UserId,
                Description = Description,
                SortOrder = sortOrder,
                Title = Title,
                StartDate = StartDate,
                EndDate = EndDate,
                TextTypeId = TextTypeId,
            });
        }

        internal override async Task Read(Position entity)
        {
            Id = entity.Id;
            CompanyId = entity.CompanyId;
            Company = entity.Company;
            UserId = entity.UserId;
            Description = entity.Description;
            SortOrder = entity.SortOrder;
            Title = entity.Title;
            StartDate = entity.StartDate;
            EndDate = entity.EndDate;
            Company = entity.Company;
            TextTypeId = entity.TextTypeId;
            SkillsViewModel.PositionId = entity.Id;
            SkillsViewModel.InitialEntities = entity.PositionSkills;
            SkillsViewModel.Description = entity.Description ?? "";
            await SkillsViewModel.Load.Execute().GetAwaiter();
        }
    }
    public class SearchSelectPositionViewModel : EntitySelectSearchViewModel<Guid, Position, AddPositionViewModel>

    {
        public SearchSelectPositionViewModel(IBusinessRepositoryFacade<Position, Guid> facade, AddPositionViewModel addViewModel, ILogger<EntitySelectSearchViewModel<Guid, Position, IBusinessRepositoryFacade<Position, Guid>, AddPositionViewModel>> logger) : base(facade, addViewModel, logger)
        {
        }

        protected override async Task<IEnumerable<Position>> DoSearch(string? text, CancellationToken token = default)
        {
            if (string.IsNullOrWhiteSpace(text))
                return [];
            SearchString = text;
            var userId = await Facade.GetCurrentUserId();
            var result = await Facade.Get(page: new Pager() { Page = 1, Size = 5 },
                filter: q => q.Company.Name.StartsWith(text) && q.UserId == userId, properites: PropertiesToLoad(), token: token);
            if (result != null)
                return result.Entities;
            return [];
        }
        protected virtual Func<IQueryable<Position>, IQueryable<Position>>? PropertiesToLoad()
        {
            return e => e.Include(x => x.Company);
        }
    }
}
