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
using System.Text;
using System.Threading.Tasks;

namespace Programming.Team.ViewModels.Resume
{
    public class AddCertificateIssuerViewModel : AddEntityViewModel<Guid, CertificateIssuer>, ICertificateIssuer
    {
        public AddCertificateIssuerViewModel(IBusinessRepositoryFacade<CertificateIssuer, Guid> facade, ILogger<AddEntityViewModel<Guid, CertificateIssuer, IBusinessRepositoryFacade<CertificateIssuer, Guid>>> logger) : base(facade, logger)
        {
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


        protected override Task Clear()
        {
            Name = string.Empty;
            Description = null;
            Url = null;
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
                Url = Url

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
    public class CertificateIssuerViewModel : EntityViewModel<Guid, CertificateIssuer>, ICertificateIssuer
    {
        public CertificateIssuerViewModel(ILogger logger, IBusinessRepositoryFacade<CertificateIssuer, Guid> facade, Guid id) : base(logger, facade, id)
        {
        }

        public CertificateIssuerViewModel(ILogger logger, IBusinessRepositoryFacade<CertificateIssuer, Guid> facade, CertificateIssuer entity) : base(logger, facade, entity)
        {
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

        protected override Task<CertificateIssuer> Populate()
        {
            return Task.FromResult(new CertificateIssuer()
            {
                Id = Id,
                Name = Name,
                Description = Description,
                Url = Url,
            });
        }

        protected override Task Read(CertificateIssuer entity)
        {
            Description = entity.Description;
            Url = entity.Url;
            Name = entity.Name;
            Id = entity.Id;
            return Task.CompletedTask;
        }
    }
    public class AddCertificateViewModel : AddUserPartionedEntity<Guid, Certificate>, ICertificate
    {
        public SearchSelectCertificateIssuerViewModel CertificateIssuer { get; }
        protected readonly CompositeDisposable disposable = new CompositeDisposable();
        ~AddCertificateViewModel()
        {
            disposable.Dispose();
        }
        public AddCertificateViewModel(SearchSelectCertificateIssuerViewModel certificateIssuer, IBusinessRepositoryFacade<Certificate, Guid> facade, ILogger<AddEntityViewModel<Guid, Certificate, IBusinessRepositoryFacade<Certificate, Guid>>> logger) : base(facade, logger)
        {
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


        protected override Task Clear()
        {
            IssuerId = Guid.Empty;
            Name = string.Empty;
            Description = null;
            Url = null;
            ValidFromDate = DateOnly.FromDateTime(DateTime.UtcNow);
            ValidToDate = null;
            CertificateIssuer.Selected = null;
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
                UserId = UserId
            });
        }
    }
    public class CertificateViewModel : EntityViewModel<Guid, Certificate>, ICertificate
    {
        public CertificateViewModel(ILogger logger, IBusinessRepositoryFacade<Certificate, Guid> facade, Guid id) : base(logger, facade, id)
        {
        }

        public CertificateViewModel(ILogger logger, IBusinessRepositoryFacade<Certificate, Guid> facade, Certificate entity) : base(logger, facade, entity)
        {
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
            set => this.RaiseAndSetIfChanged(ref description, value);
        }

        public Guid UserId { get; set; }
        private CertificateIssuer? issuer;
        public CertificateIssuer? Issuer
        {
            get => issuer;
            set => this.RaiseAndSetIfChanged(ref issuer, value);
        }

        protected override Task<Certificate> Populate()
        {
            return Task.FromResult(new Certificate()
            {
                IssuerId = IssuerId,
                Name = Name,
                Description = Description,
                Url = Url,
                ValidFromDate = ValidFromDate,
                ValidToDate = ValidToDate,
                UserId = UserId,
            });
        }

        protected override Task Read(Certificate entity)
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
            return Task.CompletedTask;
        }
        protected override Func<IQueryable<Certificate>, IQueryable<Certificate>>? PropertiesToLoad()
        {
            return e => e.Include(x => x.Issuer);
        }
    }
    public class CertificatesViewModel : EntitiesDefaultViewModel<Guid, Certificate, CertificateViewModel, AddCertificateViewModel>
    {
        public CertificatesViewModel(AddCertificateViewModel addViewModel, IBusinessRepositoryFacade<Certificate, Guid> facade, ILogger<EntitiesViewModel<Guid, Certificate, CertificateViewModel, IBusinessRepositoryFacade<Certificate, Guid>>> logger) : base(addViewModel, facade, logger)
        {
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
            return Task.FromResult(new CertificateViewModel(Logger, Facade, entity));
        }
    }
}
