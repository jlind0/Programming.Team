using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Programming.Team.Business.Core;
using Programming.Team.Core;
using Programming.Team.Templating.Core;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Programming.Team.ViewModels.Resume
{
    public class AddPublicationViewModel : AddUserPartionedEntity<Guid, Publication>, IPublication, ITextual
    {
        public SmartTextEditorViewModel<AddPublicationViewModel> SmartTextEditor { get; }
        public AddPublicationViewModel(IBusinessRepositoryFacade<Publication, Guid> facade, IDocumentTemplator templator, IConfiguration config, ILogger<AddEntityViewModel<Guid, Publication, IBusinessRepositoryFacade<Publication, Guid>>> logger) : base(facade, logger)
        {
            SmartTextEditor = new SmartTextEditorViewModel<AddPublicationViewModel>(this, logger, templator, config);
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
            TextTypeId = TextType.Text;
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
                Id = Id,
                TextTypeId = TextTypeId
            });
        }
    }
    public class PublicationViewModel : EntityViewModel<Guid, Publication>, IPublication, ITextual
    {
        public SmartTextEditorViewModel<PublicationViewModel> SmartText { get; }
        public PublicationViewModel(ILogger logger, IDocumentTemplator templator, IConfiguration config, IBusinessRepositoryFacade<Publication, Guid> facade, Guid id) : base(logger, facade, id)
        {
            SmartText = new SmartTextEditorViewModel<PublicationViewModel>(this, logger, templator, config);
        }

        public PublicationViewModel(ILogger logger, IDocumentTemplator templator, IConfiguration config, IBusinessRepositoryFacade<Publication, Guid> facade, Publication entity) : base(logger, facade, entity)
        {
            SmartText = new SmartTextEditorViewModel<PublicationViewModel>(this, logger, templator, config);
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
            get => Description;
            set => Description = value;
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
            set
            {
                bool isChanged = description != value;
                this.RaiseAndSetIfChanged(ref description, value);
                if (isChanged)
                    this.RaisePropertyChanged(nameof(Text));
            }
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

        internal override Task<Publication> Populate()
        {
            return Task.FromResult(new Publication()
            {
                Title = Title,
                Description = Description,
                Url = Url,
                PublishDate = PublishDate,
                UserId = UserId,
                Id = Id,
                TextTypeId = TextTypeId
            });
        }

        internal override Task Read(Publication entity)
        {
            Title = entity.Title;
            Description = entity.Description;
            Url = entity.Url;
            PublishDate = entity.PublishDate;
            UserId = entity.UserId;
            Id = entity.Id;
            TextTypeId = entity.TextTypeId;
            return Task.CompletedTask;

        }
    }
    public class PublicationsViewModel : EntitiesDefaultViewModel<Guid, Publication, PublicationViewModel, AddPublicationViewModel>
    {
        protected IDocumentTemplator Templator { get; }
        protected IConfiguration Config { get; }
        public PublicationsViewModel(AddPublicationViewModel addViewModel, IDocumentTemplator templator, IConfiguration config, IBusinessRepositoryFacade<Publication, Guid> facade, ILogger<EntitiesViewModel<Guid, Publication, PublicationViewModel, IBusinessRepositoryFacade<Publication, Guid>>> logger) : base(addViewModel, facade, logger)
        {
            Config = config;
            Templator = templator;
        }
        protected override Func<IQueryable<Publication>, IOrderedQueryable<Publication>>? OrderBy()
        {
            return e => e.OrderByDescending(e => e.PublishDate).OrderBy(e => e.Title);
        }
        protected override Task<PublicationViewModel> Construct(Publication entity, CancellationToken token)
        {
            return Task.FromResult(new PublicationViewModel(Logger, Templator, Config, Facade, entity));
        }
        protected override async Task<Expression<Func<Publication, bool>>?> FilterCondition()
        {
            var userid = await Facade.GetCurrentUserId();
            return e => e.UserId == userid;
        }
    }
}
