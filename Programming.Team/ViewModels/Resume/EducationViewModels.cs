using DynamicData.Binding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Programming.Team.Business.Core;
using Programming.Team.Core;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Disposables;
using System.Text;
using System.Threading.Tasks;

namespace Programming.Team.ViewModels.Resume
{
    public class AddInstitutionViewModel : AddEntityViewModel<Guid, Institution>, IInstitution
    {
        public AddInstitutionViewModel(IBusinessRepositoryFacade<Institution, Guid> facade, ILogger<AddEntityViewModel<Guid, Institution, IBusinessRepositoryFacade<Institution, Guid>>> logger) : base(facade, logger)
        {
        }

        private string name = null!;
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

        private string? city;
        public string? City
        {
            get => city;
            set => this.RaiseAndSetIfChanged(ref city, value);
        }

        private string? state;
        public string? State
        {
            get => state;
            set => this.RaiseAndSetIfChanged(ref state, value);
        }

        private string? country;
        public string? Country
        {
            get => country;
            set => this.RaiseAndSetIfChanged(ref country, value);
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
            City = null;
            State = null;
            Country = null;
            Url = null;
            return Task.CompletedTask;
        }

        protected override Task<Institution> ConstructEntity()
        {
            return Task.FromResult(new Institution()
            {
                Name = Name,
                Description = Description,
                City = City,
                State = State,
                Country = Country,
                Url = Url
            });
        }
        public override void SetText(string text)
        {
            Name = text;
        }
    }
    public class SearchSelectInstiutionViewModel : EntitySelectSearchViewModel<Guid, Institution, AddInstitutionViewModel>
    {
        public SearchSelectInstiutionViewModel(IBusinessRepositoryFacade<Institution, Guid> facade, AddInstitutionViewModel addViewModel, ILogger<EntitySelectSearchViewModel<Guid, Institution, IBusinessRepositoryFacade<Institution, Guid>, AddInstitutionViewModel>> logger) : base(facade, addViewModel, logger)
        {
        }

        protected override async Task<IEnumerable<Institution>> DoSearch(string? text, CancellationToken token = default)
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
    public class AddEducationViewModel : AddUserPartionedEntity<Guid, Education>, IEducation
    {
        private Guid institutionId;
        public Guid InstitutionId
        {
            get => institutionId;
            set
            {
                this.RaiseAndSetIfChanged(ref institutionId, value);
                this.RaisePropertyChanged(nameof(CanAdd));
            }
        }
        public override bool CanAdd => SelectInstiutionViewModel.Selected != null;
        private string? major;
        public string? Major
        {
            get => major;
            set => this.RaiseAndSetIfChanged(ref major, value);
        }

        private DateOnly startDate;
        public DateOnly StartDate
        {
            get => startDate;
            set => this.RaiseAndSetIfChanged(ref startDate, value);
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
            set => this.RaiseAndSetIfChanged(ref endDate, value);
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
        private string? description;
        public string? Description
        {
            get => description;
            set => this.RaiseAndSetIfChanged(ref description, value);
        }


        public SearchSelectInstiutionViewModel SelectInstiutionViewModel { get; }
        protected readonly CompositeDisposable disposable = new CompositeDisposable();
        ~AddEducationViewModel()
        {
            disposable.Dispose();
        }
        public AddEducationViewModel(SearchSelectInstiutionViewModel selectInstiutionViewModel, IBusinessRepositoryFacade<Education, Guid> facade, ILogger<AddEntityViewModel<Guid, Education, IBusinessRepositoryFacade<Education, Guid>>> logger) : base(facade, logger)
        {
            SelectInstiutionViewModel = selectInstiutionViewModel;
            SelectInstiutionViewModel.WhenPropertyChanged(p => p.Selected).Subscribe(p =>
            {
                if (p.Sender != null && p.Sender.Selected != null)
                    InstitutionId = p.Sender.Selected.Id;
                else
                    InstitutionId = Guid.Empty;
            }).DisposeWith(disposable);
        }
        private bool graduated;
        public bool Graduated
        {
            get => graduated;
            set => this.RaiseAndSetIfChanged(ref graduated, value);
        }

        protected override Task<Education> ConstructEntity()
        {
            return Task.FromResult(new Education()
            {
                InstitutionId = InstitutionId,
                Graduated = Graduated,
                Description = Description,
                Major = Major,
                StartDate = StartDate,
                EndDate = EndDate,
                UserId = UserId
            });
        }

        protected override Task Clear()
        {
            InstitutionId = Guid.Empty;
            SelectInstiutionViewModel.Selected = null;
            Description = null;
            EndDate = null;
            StartDate = DateOnly.FromDateTime(DateTime.Today);
            Major = null;
            Graduated = false;
            return Task.CompletedTask;
        }
    }
    public class EducationViewModel : EntityViewModel<Guid, Education>, IEducation
    {
        private Institution institution = null!;
        public Institution Institution
        {
            get => institution;
            set => this.RaiseAndSetIfChanged(ref institution, value);
        }
        private Guid institutionId;
        public Guid InstitutionId
        {
            get => institutionId;
            set => this.RaiseAndSetIfChanged(ref institutionId, value);
        }

        private string? major;
        public string? Major
        {
            get => major;
            set => this.RaiseAndSetIfChanged(ref major, value);
        }

        private DateOnly startDate;
        public DateOnly StartDate
        {
            get => startDate;
            set => this.RaiseAndSetIfChanged(ref startDate, value);
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
            set => this.RaiseAndSetIfChanged(ref endDate, value);
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
        private string? description;
        public string? Description
        {
            get => description;
            set => this.RaiseAndSetIfChanged(ref description, value);
        }
        private bool graduated;

        public EducationViewModel(ILogger logger, IBusinessRepositoryFacade<Education, Guid> facade, Guid id) : base(logger, facade, id)
        {
        }

        public EducationViewModel(ILogger logger, IBusinessRepositoryFacade<Education, Guid> facade, Education entity) : base(logger, facade, entity)
        {
        }

        public bool Graduated
        {
            get => graduated;
            set => this.RaiseAndSetIfChanged(ref graduated, value);
        }
        public Guid UserId { get; set; }
        protected override Func<IQueryable<Education>, IQueryable<Education>>? PropertiesToLoad()
        {
            return x => x.Include(x => x.Institution);
        }
        protected override Task<Education> Populate()
        {
            return Task.FromResult(new Education()
            {
                Id = Id,
                UserId = UserId,
                Description = Description,
                Graduated = graduated,
                Major = Major,
                EndDate = EndDate,
                StartDate = StartDate,
                InstitutionId = InstitutionId
            });
        }

        protected override Task Read(Education entity)
        {
            Id = entity.Id;
            Description = entity.Description;
            InstitutionId = entity.InstitutionId;
            StartDate = entity.StartDate;
            EndDate = entity.EndDate;
            Major = entity.Major;
            Graduated = entity.Graduated;
            Institution = entity.Institution;
            UserId = entity.UserId;
            return Task.CompletedTask;
        }
    }
    public class EducationsViewModel : EntitiesDefaultViewModel<Guid, Education, EducationViewModel, AddEducationViewModel>
    {
        public EducationsViewModel(AddEducationViewModel addViewModel, IBusinessRepositoryFacade<Education, Guid> facade, ILogger<EntitiesViewModel<Guid, Education, EducationViewModel, IBusinessRepositoryFacade<Education, Guid>>> logger) : base(addViewModel, facade, logger)
        {
        }

        protected override async Task<Expression<Func<Education, bool>>?> FilterCondition()
        {
            var userid = await Facade.GetCurrentUserId();
            return e => e.UserId == userid;
        }
        protected override Func<IQueryable<Education>, IOrderedQueryable<Education>>? OrderBy()
        {
            return e => e.OrderByDescending(c => c.EndDate).ThenBy(c => c.StartDate).ThenBy(c => c.Institution.Name);
        }
        protected override Func<IQueryable<Education>, IQueryable<Education>>? PropertiesToLoad()
        {
            return e => e.Include(x => x.Institution);
        }
        protected override Task<EducationViewModel> Construct(Education entity, CancellationToken token)
        {
            return Task.FromResult(new EducationViewModel(Logger, Facade, entity));
        }
    }
}
