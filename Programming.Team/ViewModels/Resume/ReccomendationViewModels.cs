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
    public class AddReccomendationViewModel : AddUserPartionedEntity<Guid, Reccomendation>, IReccomendation
    {
        public SearchSelectPositionViewModel SelectPosition { get; }
        protected readonly CompositeDisposable disposable = new CompositeDisposable();
        ~AddReccomendationViewModel()
        {
            disposable.Dispose();
        }
        public AddReccomendationViewModel(SearchSelectPositionViewModel selectPosition, IBusinessRepositoryFacade<Reccomendation, Guid> facade, ILogger<AddEntityViewModel<Guid, Reccomendation, IBusinessRepositoryFacade<Reccomendation, Guid>>> logger) : base(facade, logger)
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
        protected override Task<Reccomendation> ConstructEntity()
        {
            return Task.FromResult(new Reccomendation()
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
    public class ReccomendationViewModel : EntityViewModel<Guid, Reccomendation>, IReccomendation
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
        public ReccomendationViewModel(ILogger logger, IBusinessRepositoryFacade<Reccomendation, Guid> facade, Guid id) : base(logger, facade, id)
        {
        }

        public ReccomendationViewModel(ILogger logger, IBusinessRepositoryFacade<Reccomendation, Guid> facade, Reccomendation entity) : base(logger, facade, entity)
        {
        }

        protected override Func<IQueryable<Reccomendation>, IQueryable<Reccomendation>>? PropertiesToLoad()
        {
            return e => e.Include(x => x.Position).ThenInclude(x => x.Company);
        }

        protected override Task<Reccomendation> Populate()
        {
            return Task.FromResult(new Reccomendation()
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

        protected override Task Read(Reccomendation entity)
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
    public class ReccomendationsViewModel : EntitiesDefaultViewModel<Guid, Reccomendation, ReccomendationViewModel, AddReccomendationViewModel>
    {
        public ReccomendationsViewModel(AddReccomendationViewModel addViewModel, IBusinessRepositoryFacade<Reccomendation, Guid> facade, ILogger<EntitiesViewModel<Guid, Reccomendation, ReccomendationViewModel, IBusinessRepositoryFacade<Reccomendation, Guid>>> logger) : base(addViewModel, facade, logger)
        {
        }
        protected override Func<IQueryable<Reccomendation>, IQueryable<Reccomendation>>? PropertiesToLoad()
        {
            return x => x.Include(e => e.Position).ThenInclude(e => e.Company);
        }
        protected override async Task<Expression<Func<Reccomendation, bool>>?> FilterCondition()
        {
            var userid = await Facade.GetCurrentUserId();
            return e => e.UserId == userid;
        }
        protected override Func<IQueryable<Reccomendation>, IOrderedQueryable<Reccomendation>>? OrderBy()
        {
            return e => e.OrderBy(c => c.SortOrder).ThenBy(c => c.Name);
        }
        protected override Task<ReccomendationViewModel> Construct(Reccomendation entity, CancellationToken token)
        {
            return Task.FromResult(new ReccomendationViewModel(Logger, Facade, entity));
        }
    }
}
