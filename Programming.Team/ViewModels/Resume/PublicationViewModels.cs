using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;
using Microsoft.Extensions.Logging;
using Programming.Team.Business.Core;
using Programming.Team.Core;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Programming.Team.ViewModels.Resume
{
    public class AddPublicationViewModel : AddUserPartionedEntity<Guid, Publication>, IPublication
    {
        public AddPublicationViewModel(IBusinessRepositoryFacade<Publication, Guid> facade, ILogger<AddEntityViewModel<Guid, Publication, IBusinessRepositoryFacade<Publication, Guid>>> logger) : base(facade, logger)
        {
        }

        private string title = string.Empty;
        public string Title
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

        private string url = string.Empty;
        public string Url
        {
            get => url;
            set => this.RaiseAndSetIfChanged(ref url, value);
        }

        private DateOnly? publishDate;
        public DateOnly? PublishDate
        {
            get => publishDate;
            set => this.RaiseAndSetIfChanged(ref publishDate, value);
        }

        public DateTime? PublishDateTime
        {
            get => PublishDate == null ? null : PublishDate.Value.ToDateTime(TimeOnly.MinValue);
            set
            {
                PublishDate = value != null ? DateOnly.FromDateTime(value.Value) : null;
            }
        }

        protected override Task Clear()
        {
            Title = string.Empty;
            Description = null;
            Url = string.Empty;
            PublishDate = null;
            return Task.CompletedTask;
        }

        protected override Task<Publication> ConstructEntity()
        {
            return Task.FromResult(new Publication()
            {
                Title = Title,
                Description = Description,
                Url = Url,
                PublishDate = PublishDate,
                UserId = UserId,
                Id = Id
            });
        }
    }
    public class PublicationViewModel : EntityViewModel<Guid, Publication>, IPublication
    {
        public PublicationViewModel(ILogger logger, IBusinessRepositoryFacade<Publication, Guid> facade, Guid id) : base(logger, facade, id)
        {
        }

        public PublicationViewModel(ILogger logger, IBusinessRepositoryFacade<Publication, Guid> facade, Publication entity) : base(logger, facade, entity)
        {
        }

        private string title = string.Empty;
        public string Title
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

        private string url = string.Empty;
        public string Url
        {
            get => url;
            set => this.RaiseAndSetIfChanged(ref url, value);
        }

        private DateOnly? publishDate;
        public DateOnly? PublishDate
        {
            get => publishDate;
            set => this.RaiseAndSetIfChanged(ref publishDate, value);
        }

        public DateTime? PublishDateTime
        {
            get => PublishDate == null ? null : PublishDate.Value.ToDateTime(TimeOnly.MinValue);
            set
            {
                PublishDate = value != null ? DateOnly.FromDateTime(value.Value) : null;
            }
        }
        public Guid UserId { get; set; }

        protected override Task<Publication> Populate()
        {
            return Task.FromResult(new Publication()
            {
                Title = Title,
                Description = Description,
                Url = Url,
                PublishDate = PublishDate,
                UserId = UserId,
                Id = Id
            });
        }

        protected override Task Read(Publication entity)
        {
            Title = entity.Title;
            Description = entity.Description;
            Url = entity.Url;
            PublishDate = entity.PublishDate;
            UserId = entity.UserId;
            Id = entity.Id;
            return Task.CompletedTask;

        }
    }
    public class PublicationsViewModel : EntitiesDefaultViewModel<Guid, Publication, PublicationViewModel, AddPublicationViewModel>
    {
        public PublicationsViewModel(AddPublicationViewModel addViewModel, IBusinessRepositoryFacade<Publication, Guid> facade, ILogger<EntitiesViewModel<Guid, Publication, PublicationViewModel, IBusinessRepositoryFacade<Publication, Guid>>> logger) : base(addViewModel, facade, logger)
        {
        }
        protected override Func<IQueryable<Publication>, IOrderedQueryable<Publication>>? OrderBy()
        {
            return e => e.OrderByDescending(e => e.PublishDate).OrderBy(e => e.Title);
        }
        protected override Task<PublicationViewModel> Construct(Publication entity, CancellationToken token)
        {
            return Task.FromResult(new PublicationViewModel(Logger, Facade, entity));
        }
        protected override async Task<Expression<Func<Publication, bool>>?> FilterCondition()
        {
            var userid = await Facade.GetCurrentUserId();
            return e => e.UserId == userid;
        }
    }
}
