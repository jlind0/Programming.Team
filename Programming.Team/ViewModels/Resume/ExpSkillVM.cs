using DynamicData.Binding;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualBasic;
using Programming.Team.Business.Core;
using Programming.Team.Core;
using Programming.Team.Data.Core;
using Programming.Team.ViewModels;
using Programming.Team.ViewModels.Admin;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq.Expressions;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows.Input;
using System.Xml.Serialization;

using DynamicData.Binding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Programming.Team.Business.Core;
using Programming.Team.Core;
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

// TODO: After completion, bring all back to C#9
namespace Programming.Team.ViewModels.Resume;

public class ExpSkillVM : EntityViewModel<Guid, PositionSkill>, IPositionSkill
{
    private bool isOpen;
    public bool IsOpen
    {
        get => isOpen;
        set => this.RaiseAndSetIfChanged(ref isOpen, value);
    }
    private Guid positionId;
    public Guid PositionId
    {
        get => positionId;
        set => this.RaiseAndSetIfChanged(ref positionId, value);
    }

    private Guid skillId;
    public Guid SkillId
    {
        get => skillId;
        set => this.RaiseAndSetIfChanged(ref skillId, value);
    }

    private string? description;
    public string? Description
    {
        get => description;
        set => this.RaiseAndSetIfChanged(ref description, value);
    }

    public ReactiveCommand<Unit, Unit> Cancel { get; }
    public ReactiveCommand<Unit, Unit> Edit { get; }

    public ExpSkillVM(ILogger logger, IBusinessRepositoryFacade<PositionSkill, Guid> facade, Guid id) : base(logger, facade, id)
    {
        Cancel = ReactiveCommand.CreateFromTask(DoCancel);
        Edit = ReactiveCommand.Create(() => { IsOpen = true; });
    }

    public ExpSkillVM(ILogger logger, IBusinessRepositoryFacade<PositionSkill, Guid> facade, PositionSkill entity) : base(logger, facade, entity)
    {
        Cancel = ReactiveCommand.CreateFromTask(DoCancel);
        Edit = ReactiveCommand.Create(() => { IsOpen = true; });
    }
    protected async Task DoCancel(CancellationToken token)
    {
        IsOpen = false;
        await Load.Execute().GetAwaiter();
    }

    protected override Func<IQueryable<PositionSkill>, IQueryable<PositionSkill>>? PropertiesToLoad()
    {
        return q => q.Include(e => e.Skill).Include(e => e.Position);
    }

    private Skill skill = null!;
    public Skill Skill
    {
        get => skill;
        set => this.RaiseAndSetIfChanged(ref skill, value);
    }
    protected override Task<PositionSkill> Populate()
    {
        return Task.FromResult(new PositionSkill()
        {
            Id = Id,
            PositionId = PositionId,
            SkillId = SkillId,
            Description = Description
        });
    }

    protected override Task Read(PositionSkill entity)
    {
        Id = entity.Id;
        PositionId = entity.PositionId;
        SkillId = entity.SkillId;
        Description = entity.Description;
        Skill = entity.Skill;
        return Task.CompletedTask;
    }
}

public class AddExpVM : AddUserPartionedEntity<Guid, Position>, IPosition
{
    public SearchSelectCompanyViewModel CompanyViewModel { get; }
    protected readonly CompositeDisposable disposable = new CompositeDisposable();
    public AddExpVM(IBusinessRepositoryFacade<Position, Guid> facade,
        ILogger<AddEntityViewModel<Guid, Position, IBusinessRepositoryFacade<Position, Guid>>> logger,
        SearchSelectCompanyViewModel companyViewModel) : base(facade, logger)
    {
        CompanyViewModel = companyViewModel;
        CompanyViewModel.WhenPropertyChanged(p => p.Selected).Subscribe(p =>
        {
            if (p.Sender != null && p.Sender.Selected != null)
                CompanyId = p.Sender.Selected.Id;
            else
                CompanyId = Guid.Empty;
        }).DisposeWith(disposable);
    }

    private Guid companyId;
    [Required]
    public Guid CompanyId
    {
        get => companyId;
        set
        {
            this.RaiseAndSetIfChanged(ref companyId, value);
            this.RaisePropertyChanged(nameof(CanAdd));
        }
    }
    public override bool CanAdd => CompanyViewModel.Selected != null;
    private DateOnly startDate;
    [Required]
    public DateOnly StartDate
    {
        get => startDate;
        set
        {
            this.RaiseAndSetIfChanged(ref startDate, value);
            this.RaisePropertyChanged(nameof(StartDateTime));
        }
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
        set
        {
            this.RaiseAndSetIfChanged(ref endDate, value);
            this.RaisePropertyChanged(nameof(EndDateTime));
        }
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
    private string? title;
    public string? Title
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

    private string? sortOrder;
    public string? SortOrder
    {
        get => sortOrder;
        set => this.RaiseAndSetIfChanged(ref sortOrder, value);
    }


    protected override Task Clear()
    {
        CompanyId = Guid.Empty;
        StartDate = DateOnly.FromDateTime(DateTime.Now);
        EndDate = null;
        Title = null;
        Description = null;
        SortOrder = null;
        CompanyViewModel.Selected = null;
        return Task.CompletedTask;
    }

    protected override Task<Position> ConstructEntity()
    {
        return Task.FromResult(new Position()
        {
            CompanyId = CompanyId,
            StartDate = StartDate,
            EndDate = EndDate,
            Title = Title,
            Description = Description,
            SortOrder = SortOrder,
            UserId = UserId
        });
    }
    ~AddExpVM()
    {
        disposable.Dispose();
    }
}
public class PositionsViewModel : EntitiesDefaultViewModel<Guid, Position, ExpVM, AddExpVM>
{
    protected IServiceProvider ServiceProvider { get; }
    public PositionsViewModel(AddExpVM addViewModel,
            IBusinessRepositoryFacade<Position, Guid> facade,
            ILogger<EntitiesViewModel<Guid, Position, ExpVM, IBusinessRepositoryFacade<Position, Guid>>> logger,
            IServiceProvider serviceProvider) : base(addViewModel, facade, logger)
    {
        ServiceProvider = serviceProvider;
    }
    protected override Func<IQueryable<Position>, IQueryable<Position>>? PropertiesToLoad()
    {
        return x => x.Include(e => e.Company);
    }
    protected override Func<IQueryable<Position>, IOrderedQueryable<Position>>? OrderBy()
    {
        return e => e.OrderByDescending(c => c.EndDate ?? DateOnly.MaxValue).ThenByDescending(c => c.SortOrder).ThenByDescending(c => c.StartDate);
    }
    protected override async Task<Expression<Func<Position, bool>>?> FilterCondition()
    {
        var userId = await Facade.GetCurrentUserId();
        return e => e.UserId == userId;
    }
    protected override Task<ExpVM> Construct(Position entity, CancellationToken token)
    {
        var vm = new ExpVM(Logger, Facade,
            ServiceProvider.GetRequiredService<PositionSkillsViewModel>(), entity);

        return Task.FromResult(vm);
    }
}


public class ExpVM : EntityViewModel<Guid, Position>, IPosition
{
    private Company? company;
    public Company? Company
    {
        get => company;
        set => this.RaiseAndSetIfChanged(ref company, value);
    }
    private Guid companyId;
    [Required]
    public Guid CompanyId
    {
        get => companyId;
        set => this.RaiseAndSetIfChanged(ref companyId, value);
    }

    private DateOnly startDate;
    [Required]
    public DateOnly StartDate
    {
        get => startDate;
        set
        {
            this.RaiseAndSetIfChanged(ref startDate, value);
            this.RaisePropertyChanged(nameof(StartDateTime));
        }
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
        set
        {
            this.RaiseAndSetIfChanged(ref endDate, value);
            this.RaisePropertyChanged(nameof(EndDateTime));
        }
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
    private string? title;
    public string? Title
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

    private string? sortOrder;
    public string? SortOrder
    {
        get => sortOrder;
        set => this.RaiseAndSetIfChanged(ref sortOrder, value);
    }
    private Guid userId;
    public Guid UserId
    {
        get => userId;
        set => this.RaiseAndSetIfChanged(ref userId, value);
    }
    protected readonly CompositeDisposable disposable = new CompositeDisposable();
    public PositionSkillsViewModel SkillsViewModel { get; }
    public ExpVM(ILogger logger, IBusinessRepositoryFacade<Position, Guid> facade, PositionSkillsViewModel skillsViewModel, Guid id) : base(logger, facade, id)
    {
        SkillsViewModel = skillsViewModel;
        WireupSkillsVM();
    }

    public ExpVM(ILogger logger, IBusinessRepositoryFacade<Position, Guid> facade, PositionSkillsViewModel skillsViewModel, Position entity) : base(logger, facade, entity)
    {
        SkillsViewModel = skillsViewModel;

        WireupSkillsVM();
    }
    protected void WireupSkillsVM()
    {
        SkillsViewModel.WhenPropertyChanged(p => p.Description).Subscribe(p =>
        {
            if (p.Sender != null)
                SkillsViewModel.Description = p.Sender.Description ?? "";
        }).DisposeWith(disposable);
    }
    ~ExpVM()
    {
        disposable.Dispose();
    }
    protected override Func<IQueryable<Position>, IQueryable<Position>>? PropertiesToLoad()
    {
        return e => e.Include(x => x.Company).Include(x => x.PositionSkills).ThenInclude(c => c.Skill);
    }
    protected override Task<Position> Populate()
    {
        return Task.FromResult(new Position()
        {
            Id = Id,
            CompanyId = CompanyId,
            UserId = UserId,
            Description = Description,
            SortOrder = sortOrder,
            Title = Title,
            StartDate = StartDate,
            EndDate = EndDate,

        });
    }

    protected override async Task Read(Position entity)
    {
        Id = entity.Id;
        CompanyId = entity.CompanyId;
        Company = entity.Company;
        UserId = entity.UserId;
        Description = entity.Description;
        SortOrder = entity.SortOrder;
        Title = entity.Title;
        StartDate = entity.StartDate;
        EndDate = entity.EndDate;
        Company = entity.Company;
        SkillsViewModel.PositionId = entity.Id;
        SkillsViewModel.InitialEntities = entity.PositionSkills;
        SkillsViewModel.Description = entity.Description ?? "";
        await SkillsViewModel.Load.Execute().GetAwaiter();
    }
}
public class SearchSelectPositionViewModel : EntitySelectSearchViewModel<Guid, Position, AddExpVM>

{
    public SearchSelectPositionViewModel(IBusinessRepositoryFacade<Position, Guid> facade, AddExpVM addViewModel, ILogger<EntitySelectSearchViewModel<Guid, Position, IBusinessRepositoryFacade<Position, Guid>, AddExpVM>> logger) : base(facade, addViewModel, logger)
    {
    }

    protected override async Task<IEnumerable<Position>> DoSearch(string? text, CancellationToken token = default)
    {
        if (string.IsNullOrWhiteSpace(text))
            return [];
        SearchString = text;
        var result = await Facade.Get(page: new Pager() { Page = 1, Size = 5 },
            filter: q => q.Company.Name.StartsWith(text), properites: PropertiesToLoad(), token: token);
        if (result != null)
            return result.Entities;
        return [];
    }
    protected virtual Func<IQueryable<Position>, IQueryable<Position>>? PropertiesToLoad()
    {
        return e => e.Include(x => x.Company);
    }
}
