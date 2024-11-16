﻿using DynamicData.Binding;
using Microsoft.Extensions.Logging;
using Programming.Team.Business.Core;
using Programming.Team.Core;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Disposables;
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

        protected override Task Clear()
        {
            PositionId = Guid.Empty;
            Name = string.Empty;
            Body = string.Empty;
            SortOrder = null;
            SelectPosition.Selected = null;
            return Task.CompletedTask;
        }

        protected override Task<Reccomendation> ConstructEntity()
        {
            return Task.FromResult(new Reccomendation()
            {
                PositionId = PositionId,
                Name = Name,
                Body = Body,
                SortOrder = SortOrder,
                UserId = UserId
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

        protected override IEnumerable<Expression<Func<Reccomendation, object>>>? PropertiesToLoad()
        {
            yield return e => e.Position;
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
                UserId = UserId
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
            return Task.CompletedTask;
        }
    }
    public class ReccomendationsViewModel : EntitiesDefaultViewModel<Guid, Reccomendation, ReccomendationViewModel, AddReccomendationViewModel>
    {
        public ReccomendationsViewModel(AddReccomendationViewModel addViewModel, IBusinessRepositoryFacade<Reccomendation, Guid> facade, ILogger<EntitiesViewModel<Guid, Reccomendation, ReccomendationViewModel, IBusinessRepositoryFacade<Reccomendation, Guid>>> logger) : base(addViewModel, facade, logger)
        {
        }
        protected override IEnumerable<Expression<Func<Reccomendation, object>>>? PropertiesToLoad()
        {
            yield return e => e.Position;
        }
        protected override async Task<Expression<Func<Reccomendation, bool>>?> FilterCondition()
        {
            var userid = await Facade.GetCurrentUserId();
            return e => e.UserId == userid;
        }
        protected override Func<IQueryable<Reccomendation>, IOrderedQueryable<Reccomendation>>? OrderBy()
        {
            return e => e.OrderBy(c => c.SortOrder).OrderBy(c => c.Name);
        }
        protected override Task<ReccomendationViewModel> Construct(Reccomendation entity, CancellationToken token)
        {
            return Task.FromResult(new ReccomendationViewModel(Logger, Facade, entity));
        }
    }
}