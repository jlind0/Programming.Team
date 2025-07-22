using DynamicData;
using DynamicData.Binding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Programming.Team.AI.Core;
using Programming.Team.Business.Core;
using Programming.Team.Core;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Programming.Team.ViewModels.Resume
{
    public class AddProjectViewModel : AddUserPartionedEntity<Guid, Project>, IProject
    {
        public SearchSelectPositionViewModel PositionSelector { get; }
        protected readonly CompositeDisposable disposable = new CompositeDisposable();
        public AddProjectViewModel(SearchSelectPositionViewModel positionSelector, IBusinessRepositoryFacade<Project, Guid> facade, ILogger<AddEntityViewModel<Guid, Project, IBusinessRepositoryFacade<Project, Guid>>> logger) : base(facade, logger)
        {
            PositionSelector = positionSelector;
            PositionSelector.WhenPropertyChanged(p => p.Selected).Subscribe(p =>
            {
                if (p.Sender?.Selected != null)
                    PositionId = p.Sender.Selected.Id;
                else
                    PositionId = Guid.Empty;
            }).DisposeWith(disposable);
            
        }
        ~AddProjectViewModel()
        {
            disposable.Dispose();
        }
        private Guid positionId;
        public Guid PositionId
        {
            get => positionId;
            set => this.RaiseAndSetIfChanged(ref positionId, value);
        }

        private string? description;
        public string? Description
        {
            get => description;
            set => this.RaiseAndSetIfChanged(ref description, value);
        }

        private string? projectUrl;
        [Url(ErrorMessage = "Project URL must be a valid URL")]
        public string? ProjectUrl
        {
            get => projectUrl;
            set => this.RaiseAndSetIfChanged(ref projectUrl, value);
        }

        private string? sourceUrl;
        [Url(ErrorMessage = "Source URL must be a valid URL")]
        public string? SourceUrl
        {
            get => sourceUrl;
            set => this.RaiseAndSetIfChanged(ref sourceUrl, value);
        }

        private string? license;
        public string? License
        {
            get => license;
            set => this.RaiseAndSetIfChanged(ref license, value);
        }

        private string? sortOrder;
        public string? SortOrder
        {
            get => sortOrder;
            set => this.RaiseAndSetIfChanged(ref sortOrder, value);
        }

        private string name = string.Empty;
        [Required(ErrorMessage = "Name is required")]
        public string Name
        {
            get => name;
            set => this.RaiseAndSetIfChanged(ref name, value);
        }
        private string? liscence;
        public string? Liscence
        {
            get => liscence;
            set => this.RaiseAndSetIfChanged(ref liscence, value);
        }

        protected override Task Clear()
        {
            Name = string.Empty;
            SortOrder = null;
            License = null;
            SourceUrl = null;
            ProjectUrl = null;
            PositionSelector.Selected = null;
            Description = null;
            return Task.CompletedTask;
        }

        protected override Task<Project> ConstructEntity()
        {
            return Task.FromResult(new Project()
            {
                Name = Name,
                SortOrder = SortOrder,
                License = License,
                SourceUrl = SourceUrl,
                ProjectUrl = ProjectUrl,
                PositionId = PositionId,
                Description = Description,
                UserId = UserId,
            });
        }
    }
    public class ProjectsViewModel : EntitiesDefaultViewModel<Guid, Project, ProjectViewModel, AddProjectViewModel>
    {
        protected IServiceProvider ServiceProvider { get; }
        public ProjectsViewModel(AddProjectViewModel addViewModel,
                IBusinessRepositoryFacade<Project, Guid> facade,
                ILogger<EntitiesViewModel<Guid, Project, ProjectViewModel, IBusinessRepositoryFacade<Project, Guid>>> logger,
                IServiceProvider serviceProvider) : base(addViewModel, facade, logger)
        {
            ServiceProvider = serviceProvider;
        }
        protected override Func<IQueryable<Project>, IQueryable<Project>>? PropertiesToLoad()
        {
            return e => e.Include(x => x.Position).Include(x => x.ProjectSkills).ThenInclude(x => x.Skill);
        }
        protected override Func<IQueryable<Project>, IOrderedQueryable<Project>>? OrderBy()
        {
            // return e => e.OrderByDescending(c => c.SortOrder);
            return e => e.OrderBy(c => c.SortOrder);
        }
        protected override async Task<Expression<Func<Project, bool>>?> FilterCondition()
        {
            var userId = await Facade.GetCurrentUserId();
            return e => e.UserId == userId;
        }
        protected override Task<ProjectViewModel> Construct(Project entity, CancellationToken token)
        {
            var vm = new ProjectViewModel(ServiceProvider.GetRequiredService<ProjectSkillsViewModel>(), Logger, Facade,
                 entity);

            return Task.FromResult(vm);
        }
    }
    public class ProjectViewModel : EntityViewModel<Guid, Project>, IProject
    {
        public ProjectSkillsViewModel SkillsViewModel { get; }
        public ProjectViewModel(ProjectSkillsViewModel skillsViewModel, ILogger logger, IBusinessRepositoryFacade<Project, Guid> facade, Guid id) : base(logger, facade, id)
        {
            SkillsViewModel = skillsViewModel;
            WireUpSkillsVM();
        }

        public ProjectViewModel(ProjectSkillsViewModel skillsViewModel, ILogger logger, IBusinessRepositoryFacade<Project, Guid> facade, Project entity) : base(logger, facade, entity)
        {
            SkillsViewModel = skillsViewModel;
            WireUpSkillsVM();
        }
        protected void WireUpSkillsVM()
        {
            SkillsViewModel.WhenPropertyChanged(p => p.Description).Subscribe(p =>
            {
                if (p.Sender != null)
                    SkillsViewModel.Description = p.Sender.Description ?? "";
            }).DisposeWith(disposable);
        }
        private Guid positionId;
        public Guid PositionId
        {
            get => positionId;
            set => this.RaiseAndSetIfChanged(ref positionId, value);
        }
        public Position? position;
        public Position? Position
        {
            get => position;
            set => this.RaiseAndSetIfChanged(ref position, value);
        }
        private string? description;
        public string? Description
        {
            get => description;
            set => this.RaiseAndSetIfChanged(ref description, value);
        }

        private string? projectUrl;
        public string? ProjectUrl
        {
            get => projectUrl;
            set => this.RaiseAndSetIfChanged(ref projectUrl, value);
        }

        private string? sourceUrl;
        public string? SourceUrl
        {
            get => sourceUrl;
            set => this.RaiseAndSetIfChanged(ref sourceUrl, value);
        }

        private string? license;
        public string? License
        {
            get => license;
            set => this.RaiseAndSetIfChanged(ref license, value);
        }

        private string? sortOrder;
        public string? SortOrder
        {
            get => sortOrder;
            set => this.RaiseAndSetIfChanged(ref sortOrder, value);
        }

        private string name = string.Empty;
        public string Name
        {
            get => name;
            set => this.RaiseAndSetIfChanged(ref name, value);
        }

        private Guid userId;
        public Guid UserId
        {
            get => userId;
            set => this.RaiseAndSetIfChanged(ref userId, value);
        }
        protected readonly CompositeDisposable disposable = new CompositeDisposable();
        ~ProjectViewModel()
        {
            disposable.Dispose();
        }
        protected override Func<IQueryable<Project>, IQueryable<Project>>? PropertiesToLoad()
        {
            return e => e.Include(x => x.Position).Include(x => x.ProjectSkills).ThenInclude(x => x.Skill);
        }
        internal override Task<Project> Populate()
        {
            return Task.FromResult(new Project()
            {
                Id = Id,
                Name = Name,
                UserId = UserId,
                Description = Description,
                SortOrder = SortOrder,
                License = License,
                SourceUrl = SourceUrl,
                ProjectUrl = ProjectUrl,
                PositionId = PositionId,
            });
        }

        internal override async Task Read(Project entity)
        {
            Id = entity.Id;
            Name = entity.Name;
            UserId = entity.UserId;
            Description = entity.Description;
            SortOrder = entity.SortOrder;
            License = entity.License;
            SourceUrl = entity.SourceUrl;
            ProjectUrl = entity.ProjectUrl;
            PositionId = entity.PositionId;
            SkillsViewModel.ProjectId = entity.Id;
            SkillsViewModel.PositionId = entity.PositionId;
            SkillsViewModel.InitialEntities = entity.ProjectSkills;
            SkillsViewModel.Description = entity.Description ?? "";
            Position = entity.Position;

       

            await SkillsViewModel.Load.Execute().GetAwaiter();
        }
    }
    public class ProjectSkillViewModel : EntityViewModel<Guid, ProjectSkill>, IProjectSkill
    {
        private bool isOpen;
        public bool IsOpen
        {
            get => isOpen;
            set => this.RaiseAndSetIfChanged(ref isOpen, value);
        }
        private Guid projectId;
        public Guid ProjectId
        {
            get => projectId;
            set => this.RaiseAndSetIfChanged(ref projectId, value);
        }
        public Guid PositionId { get; set; }
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

        public ProjectSkillViewModel(ILogger logger, IBusinessRepositoryFacade<ProjectSkill, Guid> facade, Guid id) : base(logger, facade, id)
        {
            Cancel = ReactiveCommand.CreateFromTask(DoCancel);
            Edit = ReactiveCommand.Create(() => { IsOpen = true; });
        }

        public ProjectSkillViewModel(ILogger logger, IBusinessRepositoryFacade<ProjectSkill, Guid> facade, ProjectSkill entity) : base(logger, facade, entity)
        {
            Cancel = ReactiveCommand.CreateFromTask(DoCancel);
            Edit = ReactiveCommand.Create(() => { IsOpen = true; });
        }
        protected async Task DoCancel(CancellationToken token)
        {
            IsOpen = false;
            await Load.Execute().GetAwaiter();
        }

        protected override Func<IQueryable<ProjectSkill>, IQueryable<ProjectSkill>>? PropertiesToLoad()
        {
            return q => q.Include(e => e.Skill).Include(e => e.Project);
        }

        private Skill skill = null!;
        public Skill Skill
        {
            get => skill;
            set => this.RaiseAndSetIfChanged(ref skill, value);
        }
        internal override Task<ProjectSkill> Populate()
        {
            return Task.FromResult(new ProjectSkill()
            {
                Id = Id,
                ProjectId = ProjectId,
                SkillId = SkillId,
                Description = Description
            });
        }

        internal override Task Read(ProjectSkill entity)
        {
            Id = entity.Id;
            ProjectId = entity.ProjectId;
            SkillId = entity.SkillId;
            Description = entity.Description;
            Skill = entity.Skill;
            PositionId = entity.Project.PositionId;
            return Task.CompletedTask;
        }
    }
    public class AddProjectSkillViewModel : AddEntityViewModel<Guid, ProjectSkill>, IProjectSkill
    {
        public SearchSelectSkillViewModel SkillSelectorViewModel { get; }
        protected IBusinessRepositoryFacade<PositionSkill, Guid> PositionSkillFacade { get; }
        private readonly CompositeDisposable disposables = new CompositeDisposable();
        public AddProjectSkillViewModel(IBusinessRepositoryFacade<PositionSkill, Guid> positionFacade, SearchSelectSkillViewModel skillViewModel, IBusinessRepositoryFacade<ProjectSkill, Guid> facade, ILogger<AddEntityViewModel<Guid, ProjectSkill, IBusinessRepositoryFacade<ProjectSkill, Guid>>> logger) : base(facade, logger)
        {
            SkillSelectorViewModel = skillViewModel;
            PositionSkillFacade = positionFacade;
            skillViewModel.WhenPropertyChanged(p => p.Selected).Subscribe(p =>
            {
                if (p.Sender.Selected == null)
                    SkillId = Guid.Empty;
                else
                    SkillId = p.Sender.Selected.Id;
            }).DisposeWith(disposables);
        }
        public override bool CanAdd => SkillSelectorViewModel.Selected != null;
        private Guid projectId;
        public Guid ProjectId
        {
            get => projectId;
            set => this.RaiseAndSetIfChanged(ref projectId, value);
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
            set
            {
                this.RaiseAndSetIfChanged(ref skillId, value);
                this.RaisePropertyChanged(nameof(CanAdd));
            }
        }

        private string? description;
        public string? Description
        {
            get => description;
            set => this.RaiseAndSetIfChanged(ref description, value);
        }


        protected override Task Clear()
        {
            SkillId = Guid.Empty;
            Description = null;
            SkillSelectorViewModel.Selected = null;
            return Task.CompletedTask;
        }
        protected override async Task<ProjectSkill?> DoAdd(CancellationToken token)
        {
            try
            {
                var rs = await PositionSkillFacade.Get(page: new Pager() { Page = 1, Size = 1 },
                    filter: f => f.PositionId == PositionId && f.SkillId == SkillId, token: token);
                if (rs.Count == 0)
                    await PositionSkillFacade.Add(new PositionSkill()
                    {
                        SkillId = SkillId,
                        PositionId = PositionId
                    }, token: token);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                await Alert.Handle(ex.Message).GetAwaiter();
                return null;
            }
            return await base.DoAdd(token);
        }

        protected override Task<ProjectSkill> ConstructEntity()
        {
            return Task.FromResult(new ProjectSkill()
            {
                Id = Id,
                ProjectId = ProjectId,
                SkillId = SkillId,
                Description = Description
            });
        }
        ~AddProjectSkillViewModel()
        {
            disposables.Dispose();
        }
    }
    public class SuggestAddSkillsForProjectViewModel : ReactiveObject
    {
        public Interaction<string, bool> Alert { get; } = new Interaction<string, bool>();
        public Guid ProjectId { get; set; }
        public ReactiveCommand<Unit, Unit> SuggestSkills { get; }
        public ReactiveCommand<Unit, Unit> AddSelectedSkills { get; }
        public ObservableCollection<SkillViewModel> Skills { get; } = new ObservableCollection<SkillViewModel>();
        protected ISkillsBusinessFacade SkillFacade { get; }
        protected IBusinessRepositoryFacade<ProjectSkill, Guid> ProjectSkillFacade { get; }
        protected ILogger Logger { get; }
        public SuggestAddSkillsForProjectViewModel(ISkillsBusinessFacade skillFacade, IBusinessRepositoryFacade<ProjectSkill, Guid> positionSkillFacade, ILogger<SuggestAddSkillsForPositionViewModel> logger)
        {
            SuggestSkills = ReactiveCommand.CreateFromTask(DoSuggestSkills);
            AddSelectedSkills = ReactiveCommand.CreateFromTask(DoAddSelectedSkills);
            SkillFacade = skillFacade;
            ProjectSkillFacade = positionSkillFacade;
            Logger = logger;
        }
        protected async Task DoSuggestSkills(CancellationToken token)
        {
            try
            {
                Skills.Clear();
                foreach (var skill in await SkillFacade.GetSkillsExcludingProject(ProjectId, token: token))
                {
                    var vm = new SkillViewModel(Logger, SkillFacade, skill);
                    await vm.Load.Execute().GetAwaiter();
                    Skills.Add(vm);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                await Alert.Handle(ex.Message).GetAwaiter();
            }
        }
        protected async Task DoAddSelectedSkills(CancellationToken token)
        {
            try
            {
                foreach (var skill in Skills.Where(s => s.IsSelected).ToArray())
                {
                    var ps = new ProjectSkill()
                    {
                        ProjectId = ProjectId,
                        SkillId = skill.Id
                    };
                    await ProjectSkillFacade.Add(ps, token: token);
                    Skills.Remove(skill);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                await Alert.Handle(ex.Message).GetAwaiter();
            }
        }
    }
    public class ProjectSkillsViewModel : EntitiesDefaultViewModel<Guid, ProjectSkill, ProjectSkillViewModel, AddProjectSkillViewModel>
    {
        public ReactiveCommand<Unit, Unit> ExtractSkills { get; }
        public ReactiveCommand<Unit, Unit> AssociateSkills { get; }
        public ICommand ToggleOpen => ReactiveCommand.Create(() => IsOpen = !IsOpen);
        protected IResumeEnricher Enricher { get; }
        protected IBusinessRepositoryFacade<Skill, Guid> SkillFacade { get; }
        public Guid ProjectId
        {
            get => AddViewModel.ProjectId;
            set
            {
                AddViewModel.ProjectId = value;
                SuggestAddSkillsVM.ProjectId = value;
            }
        }
        public Guid PositionId
        {
            get => AddViewModel.PositionId;
            set
            {
                AddViewModel.PositionId = value;
            }
        }
        private string description = string.Empty;
        public string Description
        {
            get => description;
            set => this.RaiseAndSetIfChanged(ref description, value);
        }
        public SuggestAddSkillsForProjectViewModel SuggestAddSkillsVM { get; }
        protected IBusinessRepositoryFacade<PositionSkill, Guid> PositionSkillFacade { get; }
        public ProjectSkillsViewModel(AddProjectSkillViewModel addViewModel, SuggestAddSkillsForProjectViewModel suggestAddSkillsVM,
            IBusinessRepositoryFacade<ProjectSkill, Guid> facade, IBusinessRepositoryFacade<Skill, Guid> skillFacade,
            ILogger<EntitiesViewModel<Guid, ProjectSkill, ProjectSkillViewModel, IBusinessRepositoryFacade<ProjectSkill, Guid>>> logger,
            IResumeEnricher enricher, IBusinessRepositoryFacade<PositionSkill, Guid> positionSkillFacade) : base(addViewModel, facade, logger)
        {
            ExtractSkills = ReactiveCommand.CreateFromTask(DoExtractSkills);
            AssociateSkills = ReactiveCommand.CreateFromTask(DoAssociateSkills);
            Enricher = enricher;
            SkillFacade = skillFacade;
            SuggestAddSkillsVM = suggestAddSkillsVM;
            PositionSkillFacade = positionSkillFacade;

        }
        private bool isOpen;
        public bool IsOpen
        {
            get => isOpen;
            set => this.RaiseAndSetIfChanged(ref isOpen, value);
        }
        public ObservableCollection<RawSkillViewModel> RawSkills { get; } = new ObservableCollection<RawSkillViewModel>();
        protected async Task DoAssociateSkills(CancellationToken token)
        {
            try
            {
                foreach (var raw in RawSkills.Where(r => r.IsSelected).ToArray())
                {
                    var skillRes = await SkillFacade.Get(page: new Pager() { Page = 1, Size = 1 }, filter: q => q.Name == raw.Name, token: token);
                    var skill = skillRes.Entities.FirstOrDefault();
                    if (skill == null)
                    {
                        skill = new Skill()
                        {
                            Name = raw.Name
                        };
                        await SkillFacade.Add(skill, token: token);
                        await PositionSkillFacade.Add(new PositionSkill()
                        {
                            PositionId = PositionId,
                            SkillId = skill.Id,
                        }, token: token);
                    }
                    var ps = new ProjectSkill()
                    {
                        ProjectId = ProjectId,
                        SkillId = skill.Id
                    };
                    await Facade.Add(ps, token: token);
                    RawSkills.Remove(raw);
                }
                await Load.Execute().GetAwaiter();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                await Alert.Handle(ex.Message);
            }
        }
        protected async Task DoExtractSkills(CancellationToken token)
        {
            RawSkills.Clear();
            try
            {
                var skills = await Enricher.ExtractSkills(Description, token);
                if (skills?.Length > 0)
                {
                    var sks = skills.ToList();
                    sks.RemoveAll(s => Entities.Any(e => string.Compare(e.Skill.Name, s, StringComparison.OrdinalIgnoreCase) == 0));
                    RawSkills.AddRange(sks.Select(s => new RawSkillViewModel(Logger, ProjectId, s)));
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                await Alert.Handle(ex.Message);
            }
        }
        protected override async Task<ProjectSkillViewModel> Construct(ProjectSkill entity, CancellationToken token)
        {
            var vm = new ProjectSkillViewModel(Logger, Facade, entity);
            PositionId = entity.Project.PositionId;
            return vm;
        }
        protected override Func<IQueryable<ProjectSkill>, IOrderedQueryable<ProjectSkill>>? OrderBy()
        {
            return e => e.OrderBy(c => c.Skill.Name);
        }
        protected override Func<IQueryable<ProjectSkill>, IQueryable<ProjectSkill>>? PropertiesToLoad()
        {
            return e => e.Include(x => x.Skill).Include(x => x.Project);
        }
        protected override async Task<Expression<Func<ProjectSkill, bool>>?> FilterCondition()
        {
            return e => e.ProjectId == ProjectId;
        }
    }
}
