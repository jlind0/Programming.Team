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
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Disposables;
using System.Text;
using System.Threading.Tasks;

namespace Programming.Team.ViewModels.Resume
{
    public class AddCertificateIssuerViewModel : AddEntityViewModel<Guid, CertificateIssuer>, ICertificateIssuer, ITextual
    {
        public SmartTextEditorViewModel<AddCertificateIssuerViewModel> SmartText { get; }
        public AddCertificateIssuerViewModel(IBusinessRepositoryFacade<CertificateIssuer, Guid> facade, IDocumentTemplator templator, IConfiguration config,
            ILogger<AddEntityViewModel<Guid, CertificateIssuer, IBusinessRepositoryFacade<CertificateIssuer, Guid>>> logger) : base(facade, logger)
        {
            SmartText = new SmartTextEditorViewModel<AddCertificateIssuerViewModel>(this, logger, templator, config);
        }

        private string name = string.Empty;
        public string Name
        {
            get => name;
            set => this.RaiseAndSetIfChanged(ref name, value);
        }

        private string? description;
        public string? Description
        {
            get => description;
            set => this.RaiseAndSetIfChanged(ref description, value);
        }

        private string? url;
        public string? Url
        {
            get => url;
            set => this.RaiseAndSetIfChanged(ref url, value);
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

        protected override Task Clear()
        {
            Name = string.Empty;
            Description = null;
            Url = null;
            TextTypeId = TextType.Text;
            return Task.CompletedTask;
        }
        public override void SetText(string text)
        {
            Name = text;
        }
        protected override Task<CertificateIssuer> ConstructEntity()
        {
            return Task.FromResult(new CertificateIssuer()
            {
                Id = Id,
                Name = Name,
                Description = Description,
                Url = Url,
                TextTypeId = TextTypeId

            });
        }
    }
    public class SearchSelectCertificateIssuerViewModel : EntitySelectSearchViewModel<Guid, CertificateIssuer, AddCertificateIssuerViewModel>
    {
        public SearchSelectCertificateIssuerViewModel(IBusinessRepositoryFacade<CertificateIssuer, Guid> facade, AddCertificateIssuerViewModel addViewModel, ILogger<EntitySelectSearchViewModel<Guid, CertificateIssuer, IBusinessRepositoryFacade<CertificateIssuer, Guid>, AddCertificateIssuerViewModel>> logger) : base(facade, addViewModel, logger)
        {
        }

        protected override async Task<IEnumerable<CertificateIssuer>> DoSearch(string? text, CancellationToken token = default)
        {
            if (string.IsNullOrWhiteSpace(text))
                return [];
            SearchString = text;
            var result = await Facade.Get(page: new Pager() { Page = 1, Size = 5 },
                filter: q => q.Name.StartsWith(text), token: token);
            if (result != null)
                return result.Entities;
            return [];
        }
    }
    public class CertificateIssuerViewModel : EntityViewModel<Guid, CertificateIssuer>, ICertificateIssuer, ITextual
    {
        public SmartTextEditorViewModel<CertificateIssuerViewModel> SmartText { get; }
        public CertificateIssuerViewModel(ILogger logger, IDocumentTemplator templator, IConfiguration config, IBusinessRepositoryFacade<CertificateIssuer, Guid> facade, Guid id) : base(logger, facade, id)
        {
            SmartText = new SmartTextEditorViewModel<CertificateIssuerViewModel>(this, logger, templator, config);
        }

        public CertificateIssuerViewModel(ILogger logger, IDocumentTemplator templator, IConfiguration config, IBusinessRepositoryFacade<CertificateIssuer, Guid> facade, CertificateIssuer entity) : base(logger, facade, entity)
        {
            SmartText = new SmartTextEditorViewModel<CertificateIssuerViewModel>(this, logger, templator, config);
        }

        private string name = string.Empty;
        public string Name
        {
            get => name;
            set => this.RaiseAndSetIfChanged(ref name, value);
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

        private string? url;
        public string? Url
        {
            get => url;
            set => this.RaiseAndSetIfChanged(ref url, value);
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
        internal override Task<CertificateIssuer> Populate()
        {
            return Task.FromResult(new CertificateIssuer()
            {
                Id = Id,
                Name = Name,
                Description = Description,
                Url = Url,
                TextTypeId = TextTypeId
            });
        }

        internal override Task Read(CertificateIssuer entity)
        {
            Description = entity.Description;
            Url = entity.Url;
            Name = entity.Name;
            Id = entity.Id;
            TextTypeId = entity.TextTypeId;
            return Task.CompletedTask;
        }
    }
    public class AddCertificateViewModel : AddUserPartionedEntity<Guid, Certificate>, ICertificate, ITextual
    {
        public SmartTextEditorViewModel<AddCertificateViewModel> SmartText { get; }
        public SearchSelectCertificateIssuerViewModel CertificateIssuer { get; }
        protected readonly CompositeDisposable disposable = new CompositeDisposable();
        ~AddCertificateViewModel()
        {
            disposable.Dispose();
        }
        public AddCertificateViewModel(SearchSelectCertificateIssuerViewModel certificateIssuer, IDocumentTemplator templator, IConfiguration config, IBusinessRepositoryFacade<Certificate, Guid> facade, ILogger<AddEntityViewModel<Guid, Certificate, IBusinessRepositoryFacade<Certificate, Guid>>> logger) : base(facade, logger)
        {
            SmartText = new SmartTextEditorViewModel<AddCertificateViewModel>(this, logger, templator, config);
            CertificateIssuer = certificateIssuer;
            CertificateIssuer.WhenPropertyChanged(p => p.Selected).Subscribe(p =>
            {
                if (p.Sender != null && p.Sender.Selected != null)
                    IssuerId = p.Sender.Selected.Id;
                else
                    IssuerId = Guid.Empty;
            }).DisposeWith(disposable);
        }
        public override bool CanAdd => CertificateIssuer.Selected != null;
        private Guid issuerId;
        public Guid IssuerId
        {
            get => issuerId;
            set
            {
                this.RaiseAndSetIfChanged(ref issuerId, value);
                this.RaisePropertyChanged(nameof(CanAdd));
            }
        }

        private string name = string.Empty;
        public string Name
        {
            get => name;
            set => this.RaiseAndSetIfChanged(ref name, value);
        }

        private DateOnly validFromDate = DateOnly.FromDateTime(DateTime.UtcNow);
        public DateOnly ValidFromDate
        {
            get => validFromDate;
            set => this.RaiseAndSetIfChanged(ref validFromDate, value);
        }
        public DateTime? ValidFromDateTime
        {
            get => ValidFromDate.ToDateTime(TimeOnly.MinValue);
            set
            {
                ValidFromDate = DateOnly.FromDateTime(value ?? DateTime.Today);
            }
        }
        private DateOnly? validToDate;
        public DateOnly? ValidToDate
        {
            get => validToDate;
            set => this.RaiseAndSetIfChanged(ref validToDate, value);
        }
        public DateTime? ValidToDateTime
        {
            get => ValidToDate == null ? null : ValidToDate.Value.ToDateTime(TimeOnly.MinValue);
            set
            {
                ValidToDate = value == null ? null : DateOnly.FromDateTime(value.Value);
            }
        }
        private string? url;
        public string? Url
        {
            get => url;
            set => this.RaiseAndSetIfChanged(ref url, value);
        }

        private string? description;
        public string? Description
        {
            get => description;
            set => this.RaiseAndSetIfChanged(ref description, value);
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

        protected override Task Clear()
        {
            IssuerId = Guid.Empty;
            Name = string.Empty;
            Description = null;
            Url = null;
            ValidFromDate = DateOnly.FromDateTime(DateTime.UtcNow);
            ValidToDate = null;
            CertificateIssuer.Selected = null;
            TextTypeId = TextType.Text;
            return Task.CompletedTask;
        }

        protected override Task<Certificate> ConstructEntity()
        {
            return Task.FromResult(new Certificate()
            {
                Id = Id,
                Name = Name,
                Description = Description,
                Url = Url,
                ValidFromDate = ValidFromDate,
                ValidToDate = ValidToDate,
                IssuerId = IssuerId,
                UserId = UserId,
                TextTypeId = TextTypeId
            });
        }
    }
    public class CertificateViewModel : EntityViewModel<Guid, Certificate>, ICertificate, ITextual
    {
        public SmartTextEditorViewModel<CertificateViewModel> SmartText { get; }
        public CertificateViewModel(ILogger logger, IDocumentTemplator templator, IConfiguration config, IBusinessRepositoryFacade<Certificate, Guid> facade, Guid id) : base(logger, facade, id)
        {
            SmartText = new SmartTextEditorViewModel<CertificateViewModel>(this, logger, templator, config);
        }

        public CertificateViewModel(ILogger logger, IDocumentTemplator templator, IConfiguration config, IBusinessRepositoryFacade<Certificate, Guid> facade, Certificate entity) : base(logger, facade, entity)
        {
            SmartText = new SmartTextEditorViewModel<CertificateViewModel>(this, logger, templator, config);
        }

        private Guid issuerId;
        public Guid IssuerId
        {
            get => issuerId;
            set => this.RaiseAndSetIfChanged(ref issuerId, value);
        }

        private string name = string.Empty;
        public string Name
        {
            get => name;
            set => this.RaiseAndSetIfChanged(ref name, value);
        }

        private DateOnly validFromDate = DateOnly.FromDateTime(DateTime.UtcNow);
        public DateOnly ValidFromDate
        {
            get => validFromDate;
            set => this.RaiseAndSetIfChanged(ref validFromDate, value);
        }
        public DateTime? ValidFromDateTime
        {
            get => ValidFromDate.ToDateTime(TimeOnly.MinValue);
            set
            {
                ValidFromDate = DateOnly.FromDateTime(value ?? DateTime.Today);
            }
        }
        private DateOnly? validToDate;
        public DateOnly? ValidToDate
        {
            get => validToDate;
            set => this.RaiseAndSetIfChanged(ref validToDate, value);
        }
        public DateTime? ValidToDateTime
        {
            get => ValidToDate == null ? null : ValidToDate.Value.ToDateTime(TimeOnly.MinValue);
            set
            {
                ValidToDate = value == null ? null : DateOnly.FromDateTime(value.Value);
            }
        }
        private string? url;
        public string? Url
        {
            get => url;
            set => this.RaiseAndSetIfChanged(ref url, value);
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
        public Guid UserId { get; set; }
        private CertificateIssuer? issuer;
        public CertificateIssuer? Issuer
        {
            get => issuer;
            set => this.RaiseAndSetIfChanged(ref issuer, value);
        }

        internal override Task<Certificate> Populate()
        {
            return Task.FromResult(new Certificate()
            {
                Id = Id,
                IssuerId = IssuerId,
                Name = Name,
                Description = Description,
                Url = Url,
                ValidFromDate = ValidFromDate,
                ValidToDate = ValidToDate,
                UserId = UserId,
                TextTypeId = TextTypeId,
            });
        }

        internal override Task Read(Certificate entity)
        {
            IssuerId = entity.IssuerId;
            Name = entity.Name;
            Description = entity.Description;
            Url = entity.Url;
            ValidFromDate = entity.ValidFromDate;
            ValidToDate = entity.ValidToDate;
            UserId = entity.UserId;
            Id = entity.Id;
            Issuer = entity.Issuer;
            TextTypeId = entity.TextTypeId;
            return Task.CompletedTask;
        }
        protected override Func<IQueryable<Certificate>, IQueryable<Certificate>>? PropertiesToLoad()
        {
            return e => e.Include(x => x.Issuer);
        }
    }
    public class CertificatesViewModel : EntitiesDefaultViewModel<Guid, Certificate, CertificateViewModel, AddCertificateViewModel>
    {
        protected IDocumentTemplator Templator { get; }
        protected IConfiguration Config { get; }
        public CertificatesViewModel(AddCertificateViewModel addViewModel, IDocumentTemplator templator, IConfiguration config, IBusinessRepositoryFacade<Certificate, Guid> facade, ILogger<EntitiesViewModel<Guid, Certificate, CertificateViewModel, IBusinessRepositoryFacade<Certificate, Guid>>> logger) : base(addViewModel, facade, logger)
        {
            Config = config;
            Templator = templator;
        }
        protected override async Task<Expression<Func<Certificate, bool>>?> FilterCondition()
        {
            var userid = await Facade.GetCurrentUserId();
            return e => e.UserId == userid;
        }
        protected override Func<IQueryable<Certificate>, IQueryable<Certificate>>? PropertiesToLoad()
        {
            return x => x.Include(e => e.Issuer);
        }
        protected override Func<IQueryable<Certificate>, IOrderedQueryable<Certificate>>? OrderBy()
        {
            return e => e.OrderByDescending(e => e.ValidToDate).OrderBy(e => e.ValidFromDate);
        }
        protected override Task<CertificateViewModel> Construct(Certificate entity, CancellationToken token)
        {
            return Task.FromResult(new CertificateViewModel(Logger, Templator, Config, Facade, entity));
        }
    }
}
