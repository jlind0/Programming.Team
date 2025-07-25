﻿using DynamicData.Binding;
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
using System.ComponentModel.Design;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Disposables;
using System.Text;
using System.Threading.Tasks;

namespace Programming.Team.ViewModels.Resume
{
    public class AddInstitutionViewModel : AddEntityViewModel<Guid, Institution>, IInstitution, ITextual
    {
        public SmartTextEditorViewModel<AddInstitutionViewModel> SmartTextEditor { get; }
        public AddInstitutionViewModel(IBusinessRepositoryFacade<Institution, Guid> facade, IDocumentTemplator templator, IConfiguration config, ILogger<AddEntityViewModel<Guid, Institution, IBusinessRepositoryFacade<Institution, Guid>>> logger) : base(facade, logger)
        {
            SmartTextEditor = new SmartTextEditorViewModel<AddInstitutionViewModel>(this, logger, templator, config);
        }

        private string name = null!;

        [Required(ErrorMessage = "Name is required.")]
        [StringLength(100, ErrorMessage = "Name can't be longer than 100 characters.")]
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
            City = null;
            State = null;
            Country = null;
            Url = null;
            TextTypeId = TextType.Text;
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
                Url = Url,
                TextTypeId = TextTypeId
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
    public class AddEducationViewModel : AddUserPartionedEntity<Guid, Education>, IEducation, ITextual
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

        public SearchSelectInstiutionViewModel SelectInstiutionViewModel { get; }
        protected readonly CompositeDisposable disposable = new CompositeDisposable();
        ~AddEducationViewModel()
        {
            disposable.Dispose();
        }
        public SmartTextEditorViewModel<AddEducationViewModel> SmartTextEditor { get; }
        public AddEducationViewModel(SearchSelectInstiutionViewModel selectInstiutionViewModel, IDocumentTemplator templator, IConfiguration config, IBusinessRepositoryFacade<Education, Guid> facade, ILogger<AddEntityViewModel<Guid, Education, IBusinessRepositoryFacade<Education, Guid>>> logger) : base(facade, logger)
        {
            SelectInstiutionViewModel = selectInstiutionViewModel;
            SelectInstiutionViewModel.WhenPropertyChanged(p => p.Selected).Subscribe(p =>
            {
                if (p.Sender != null && p.Sender.Selected != null)
                    InstitutionId = p.Sender.Selected.Id;
                else
                    InstitutionId = Guid.Empty;
            }).DisposeWith(disposable);
            SmartTextEditor = new SmartTextEditorViewModel<AddEducationViewModel>(this, logger, templator, config);
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
                UserId = UserId,
                TextTypeId = TextTypeId
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
            TextTypeId = TextType.Text;
            return Task.CompletedTask;
        }
    }
    public class EducationViewModel : EntityViewModel<Guid, Education>, IEducation, ITextual
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
            set
            {
                bool isChanged = description != value;
                this.RaiseAndSetIfChanged(ref description, value);
                if (isChanged)
                    this.RaisePropertyChanged(nameof(Text));
            }
        }
        private bool graduated;
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
        public SmartTextEditorViewModel<EducationViewModel> SmartText { get; }
        public EducationViewModel(ILogger logger, IDocumentTemplator templator, IConfiguration config, IBusinessRepositoryFacade<Education, Guid> facade, Guid id) : base(logger, facade, id)
        {
            SmartText = new SmartTextEditorViewModel<EducationViewModel>(this, logger, templator, config);
        }

        public EducationViewModel(ILogger logger, IDocumentTemplator templator, IConfiguration config, IBusinessRepositoryFacade<Education, Guid> facade, Education entity) : base(logger, facade, entity)
        {
            SmartText = new SmartTextEditorViewModel<EducationViewModel>(this, logger, templator, config);
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
        internal override Task<Education> Populate()
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
                InstitutionId = InstitutionId,
                TextTypeId = TextTypeId,
            });
        }

        internal override Task Read(Education entity)
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
            TextTypeId = entity.TextTypeId;
            return Task.CompletedTask;
        }
    }
    public class EducationsViewModel : EntitiesDefaultViewModel<Guid, Education, EducationViewModel, AddEducationViewModel>
    {
        protected IDocumentTemplator Templator { get; }
        protected IConfiguration Config { get; }    
        public EducationsViewModel(AddEducationViewModel addViewModel, IDocumentTemplator templator, IConfiguration config, IBusinessRepositoryFacade<Education, Guid> facade, ILogger<EntitiesViewModel<Guid, Education, EducationViewModel, IBusinessRepositoryFacade<Education, Guid>>> logger) : base(addViewModel, facade, logger)
        {
            Templator = templator;
            Config = config;
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
            return Task.FromResult(new EducationViewModel(Logger, Templator, Config, Facade, entity));
        }
    }
}
