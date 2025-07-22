using DynamicData.Binding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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
    public class AddRecommendationViewModel : AddUserPartionedEntity<Guid, Recommendation>, IRecommendation, ITextual
    {
        public SearchSelectPositionViewModel SelectPosition { get; }
        protected readonly CompositeDisposable disposable = new CompositeDisposable();
        public SmartTextEditorViewModel<AddRecommendationViewModel> SmartTextEditor { get; }
        ~AddRecommendationViewModel()
        {
            disposable.Dispose();
        }
        public AddRecommendationViewModel(SearchSelectPositionViewModel selectPosition,IDocumentTemplator templator, IConfiguration config, IBusinessRepositoryFacade<Recommendation, Guid> facade, ILogger<AddEntityViewModel<Guid, Recommendation, IBusinessRepositoryFacade<Recommendation, Guid>>> logger) : base(facade, logger)
        {
            SmartTextEditor = new SmartTextEditorViewModel<AddRecommendationViewModel>(this, logger, templator, config);
            SelectPosition = selectPosition;
            SelectPosition.WhenPropertyChanged(p => p.Selected).Subscribe(p =>
            {
                if (p.Sender != null && p.Sender.Selected != null)
                    PositionId = p.Sender.Selected.Id;
                else
                    PositionId = Guid.Empty;
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
            get => Body;
            set => Body = value ?? string.Empty;
        }
        private Guid positionId;
        public Guid PositionId
        {
            get => positionId;
            set
            {
                this.RaiseAndSetIfChanged(ref positionId, value);
                this.RaisePropertyChanged(nameof(CanAdd));
            }
        }

        private string name = string.Empty;
        [Required(ErrorMessage = "Name is required")]
        public string Name
        {
            get => name;
            set => this.RaiseAndSetIfChanged(ref name, value);
        }

        private string body = string.Empty;
        [Required(ErrorMessage = "Body is required")]
        public string Body
        {
            get => body;
            set => this.RaiseAndSetIfChanged(ref body, value);
        }

        private string? sortOrder;
        public string? SortOrder
        {
            get => sortOrder;
            set => this.RaiseAndSetIfChanged(ref sortOrder, value);
        }

        private string? title;
        public string? Title
        {
            get => title;
            set => this.RaiseAndSetIfChanged(ref title, value);
        }

        protected override Task Clear()
        {
            PositionId = Guid.Empty;
            Name = string.Empty;
            Body = string.Empty;
            SortOrder = null;
            SelectPosition.Selected = null;
            Title = null;
            TextTypeId = TextType.Text;
            return Task.CompletedTask;
        }
        public override bool CanAdd => SelectPosition.Selected != null;
        protected override Task<Recommendation> ConstructEntity()
        {
            return Task.FromResult(new Recommendation()
            {
                PositionId = PositionId,
                Name = Name,
                Body = Body,
                SortOrder = SortOrder,
                UserId = UserId,
                Title = Title,
                TextTypeId = TextTypeId
            });
        }
    }
    public class RecommendationViewModel : EntityViewModel<Guid, Recommendation>, IRecommendation, ITextual
    {
        private Guid positionId;
        public Guid PositionId
        {
            get => positionId;
            set => this.RaiseAndSetIfChanged(ref positionId, value);
        }

        private string name = string.Empty;
        [Required(ErrorMessage = "Name is required")]
        public string Name
        {
            get => name;
            set => this.RaiseAndSetIfChanged(ref name, value);
        }

        private string body = string.Empty;
        [Required(ErrorMessage = "Body is required")]
        public string Body
        {
            get => body;
            set
            {
                bool isChanged = body != value;
                this.RaiseAndSetIfChanged(ref body, value);
                if (isChanged)
                    this.RaisePropertyChanged(nameof(Text));
            }
        }

        private string? sortOrder;
        public string? SortOrder
        {
            get => sortOrder;
            set => this.RaiseAndSetIfChanged(ref sortOrder, value);
        }
        private string? title;
        public string? Title
        {
            get => title;
            set => this.RaiseAndSetIfChanged(ref title, value);
        }
        public Guid UserId { get; set; }
        private Position? position;
        public Position? Position
        {
            get => position;
            set => this.RaiseAndSetIfChanged(ref position, value);
        }
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
            get => Body;
            set => Body = value ?? string.Empty;
        }
        public SmartTextEditorViewModel<RecommendationViewModel> SmartText { get; }
        public RecommendationViewModel(ILogger logger, IDocumentTemplator templator, IConfiguration config, IBusinessRepositoryFacade<Recommendation, Guid> facade, Guid id) : base(logger, facade, id)
        {
            SmartText = new SmartTextEditorViewModel<RecommendationViewModel>(this, logger, templator, config);
        }

        public RecommendationViewModel(ILogger logger, IDocumentTemplator templator, IConfiguration config, IBusinessRepositoryFacade<Recommendation, Guid> facade, Recommendation entity) : base(logger, facade, entity)
        {
            SmartText = new SmartTextEditorViewModel<RecommendationViewModel>(this, logger, templator, config);
        }
        
        protected override Func<IQueryable<Recommendation>, IQueryable<Recommendation>>? PropertiesToLoad()
        {
            return e => e.Include(x => x.Position).ThenInclude(x => x.Company);
        }

        internal override Task<Recommendation> Populate()
        {
            return Task.FromResult(new Recommendation()
            {
                Id = Id,
                Name = Name,
                Body = body,
                SortOrder = sortOrder,
                PositionId = PositionId,
                UserId = UserId,
                Title = Title,
                TextTypeId = TextTypeId
            });
        }

        internal override Task Read(Recommendation entity)
        {
            Id = entity.Id;
            UserId = entity.UserId;
            Body = entity.Body;
            SortOrder = entity.SortOrder;
            Name = entity.Name;
            PositionId = entity.PositionId;
            Position = entity.Position;
            Title = entity.Title;
            TextTypeId = entity.TextTypeId;
            return Task.CompletedTask;
        }
    }
    public class RecommendationsViewModel : EntitiesDefaultViewModel<Guid, Recommendation, RecommendationViewModel, AddRecommendationViewModel>
    {
        protected IDocumentTemplator Templator { get; }
        protected IConfiguration Config { get; }
        public RecommendationsViewModel(AddRecommendationViewModel addViewModel, IDocumentTemplator templator, IConfiguration config, IBusinessRepositoryFacade<Recommendation, Guid> facade, ILogger<EntitiesViewModel<Guid, Recommendation, RecommendationViewModel, IBusinessRepositoryFacade<Recommendation, Guid>>> logger) : base(addViewModel, facade, logger)
        {
            Templator = templator;
            Config = config;
        }
        protected override Func<IQueryable<Recommendation>, IQueryable<Recommendation>>? PropertiesToLoad()
        {
            return x => x.Include(e => e.Position).ThenInclude(e => e.Company);
        }
        protected override async Task<Expression<Func<Recommendation, bool>>?> FilterCondition()
        {
            var userid = await Facade.GetCurrentUserId();
            return e => e.UserId == userid;
        }
        protected override Func<IQueryable<Recommendation>, IOrderedQueryable<Recommendation>>? OrderBy()
        {
            return e => e.OrderBy(c => c.SortOrder).ThenBy(c => c.Name);
        }
        protected override Task<RecommendationViewModel> Construct(Recommendation entity, CancellationToken token)
        {
            return Task.FromResult(new RecommendationViewModel(Logger, Templator,Config,Facade, entity));
        }
    }
}
