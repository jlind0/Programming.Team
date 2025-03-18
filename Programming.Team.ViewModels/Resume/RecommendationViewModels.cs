using DynamicData.Binding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Programming.Team.Business.Core;
using Programming.Team.Core;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Programming.Team.ViewModels.Resume
{
    public class AddRecommendationViewModel : AddUserPartionedEntity<Guid, Recommendation>, IRecommendation
    {
        public SearchSelectPositionViewModel SelectPosition { get; }
        protected readonly CompositeDisposable disposable = new CompositeDisposable();
        ~AddRecommendationViewModel()
        {
            disposable.Dispose();
        }
        public AddRecommendationViewModel(SearchSelectPositionViewModel selectPosition, IBusinessRepositoryFacade<Recommendation, Guid> facade, ILogger<AddEntityViewModel<Guid, Recommendation, IBusinessRepositoryFacade<Recommendation, Guid>>> logger) : base(facade, logger)
        {
            SelectPosition = selectPosition;
            SelectPosition.WhenPropertyChanged(p => p.Selected).Subscribe(p =>
            {
                if (p.Sender != null && p.Sender.Selected != null)
                    PositionId = p.Sender.Selected.Id;
                else
                    PositionId = Guid.Empty;
            }).DisposeWith(disposable);
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
        public string Name
        {
            get => name;
            set => this.RaiseAndSetIfChanged(ref name, value);
        }

        private string body = string.Empty;
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
                Title = Title
            });
        }
    }
    public class RecommendationViewModel : EntityViewModel<Guid, Recommendation>, IRecommendation
    {
        private Guid positionId;
        public Guid PositionId
        {
            get => positionId;
            set => this.RaiseAndSetIfChanged(ref positionId, value);
        }

        private string name = string.Empty;
        public string Name
        {
            get => name;
            set => this.RaiseAndSetIfChanged(ref name, value);
        }

        private string body = string.Empty;
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
        public Guid UserId { get; set; }
        private Position? position;
        public Position? Position
        {
            get => position;
            set => this.RaiseAndSetIfChanged(ref position, value);
        }
        public RecommendationViewModel(ILogger logger, IBusinessRepositoryFacade<Recommendation, Guid> facade, Guid id) : base(logger, facade, id)
        {
        }

        public RecommendationViewModel(ILogger logger, IBusinessRepositoryFacade<Recommendation, Guid> facade, Recommendation entity) : base(logger, facade, entity)
        {
        }
        
        protected override Func<IQueryable<Recommendation>, IQueryable<Recommendation>>? PropertiesToLoad()
        {
            return e => e.Include(x => x.Position).ThenInclude(x => x.Company);
        }

        protected override Task<Recommendation> Populate()
        {
            return Task.FromResult(new Recommendation()
            {
                Id = Id,
                Name = Name,
                Body = body,
                SortOrder = sortOrder,
                PositionId = PositionId,
                UserId = UserId,
                Title = Title
            });
        }

        protected override Task Read(Recommendation entity)
        {
            Id = entity.Id;
            UserId = entity.UserId;
            Body = entity.Body;
            SortOrder = entity.SortOrder;
            Name = entity.Name;
            PositionId = entity.PositionId;
            Position = entity.Position;
            Title = entity.Title;
            return Task.CompletedTask;
        }
    }
    public class RecommendationsViewModel : EntitiesDefaultViewModel<Guid, Recommendation, RecommendationViewModel, AddRecommendationViewModel>
    {
        public RecommendationsViewModel(AddRecommendationViewModel addViewModel, IBusinessRepositoryFacade<Recommendation, Guid> facade, ILogger<EntitiesViewModel<Guid, Recommendation, RecommendationViewModel, IBusinessRepositoryFacade<Recommendation, Guid>>> logger) : base(addViewModel, facade, logger)
        {
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
            return Task.FromResult(new RecommendationViewModel(Logger, Facade, entity));
        }
    }
}
