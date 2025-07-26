using DynamicData;
using DynamicData.Binding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Programming.Team.AI.Core;
using Programming.Team.Business.Core;
using Programming.Team.Core;
using Programming.Team.Templating.Core;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Programming.Team.ViewModels.Resume
{
    public class AddSkillViewModel : AddEntityViewModel<Guid, Skill>, ISkill
    {
        public AddSkillViewModel(IBusinessRepositoryFacade<Skill, Guid> facade, ILogger<AddEntityViewModel<Guid, Skill, IBusinessRepositoryFacade<Skill, Guid>>> logger) : base(facade, logger)
        {
        }
        private string name = null!;
        public string Name
        {
            get => name;
            set => this.RaiseAndSetIfChanged(ref name, value);
        }

        protected override Task Clear()
        {
            Name = string.Empty;
            return Task.CompletedTask;
        }

        protected override Task<Skill> ConstructEntity()
        {
            return Task.FromResult(new Skill()
            {
                Name = Name
            });
        }
        public override void SetText(string text)
        {
            Name = text;
        }
    }
    public class SkillViewModel : EntityViewModel<Guid, Skill>, ISkill
    {
        public SkillViewModel(ILogger logger, IBusinessRepositoryFacade<Skill, Guid> facade, Guid id) : base(logger, facade, id)
        {
        }

        public SkillViewModel(ILogger logger, IBusinessRepositoryFacade<Skill, Guid> facade, Skill entity) : base(logger, facade, entity)
        {
        }
        private string name = null!;
        public string Name
        {
            get => name;
            set => this.RaiseAndSetIfChanged(ref name, value);
        }

        internal override Task<Skill> Populate()
        {
            return Task.FromResult(new Skill()
            {
                Id = Id,
                Name = Name
            });
        }

        internal override Task Read(Skill entity)
        {
            Id = entity.Id;
            Name = entity.Name;
            return Task.CompletedTask;
        }
    }
    public class AddPositionSkillViewModel : AddEntityViewModel<Guid, PositionSkill>, IPositionSkill, ITextual
    {
        public SearchSelectSkillViewModel SkillSelectorViewModel { get; }
        private readonly CompositeDisposable disposables = new CompositeDisposable();
        public SmartTextEditorViewModel<AddPositionSkillViewModel> SmartTextEditor { get; }
        public AddPositionSkillViewModel(SearchSelectSkillViewModel skillViewModel, IDocumentTemplator templator, IConfiguration config, IBusinessRepositoryFacade<PositionSkill, Guid> facade, ILogger<AddEntityViewModel<Guid, PositionSkill, IBusinessRepositoryFacade<PositionSkill, Guid>>> logger) : base(facade, logger)
        {
            SkillSelectorViewModel = skillViewModel;
            SmartTextEditor = new SmartTextEditorViewModel<AddPositionSkillViewModel>(this, logger, templator, config);
            skillViewModel.WhenPropertyChanged(p => p.Selected).Subscribe(p =>
            {
                if (p.Sender.Selected == null)
                    SkillId = Guid.Empty;
                else
                    SkillId = p.Sender.Selected.Id;
            }).DisposeWith(disposables);
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
        public override bool CanAdd => SkillSelectorViewModel.Selected != null;
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
            TextTypeId = TextType.Text;
            return Task.CompletedTask;
        }

        protected override Task<PositionSkill> ConstructEntity()
        {
            return Task.FromResult(new PositionSkill()
            {
                Id = Id,
                PositionId = PositionId,
                SkillId = SkillId,
                Description = Description,
                TextTypeId = TextTypeId
            });
        }
        ~AddPositionSkillViewModel()
        {
            disposables.Dispose();
        }
    }
    public class PositionSkillViewModel : EntityViewModel<Guid, PositionSkill>, IPositionSkill, ITextual
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
        public ReactiveCommand<Unit, Unit> Cancel { get; }
        public ReactiveCommand<Unit, Unit> Edit { get; }
        public SmartTextEditorViewModel<PositionSkillViewModel> SmartText { get; }
        public PositionSkillViewModel(ILogger logger, IDocumentTemplator templator, IConfiguration config, IBusinessRepositoryFacade<PositionSkill, Guid> facade, Guid id) : base(logger, facade, id)
        {
            SmartText = new SmartTextEditorViewModel<PositionSkillViewModel>(this, logger, templator, config);
            Cancel = ReactiveCommand.CreateFromTask(DoCancel);
            Edit = ReactiveCommand.Create(() => { IsOpen = true; });
        }

        public PositionSkillViewModel(ILogger logger, IDocumentTemplator templator, IConfiguration config, IBusinessRepositoryFacade<PositionSkill, Guid> facade, PositionSkill entity) : base(logger, facade, entity)
        {
            SmartText = new SmartTextEditorViewModel<PositionSkillViewModel>(this, logger, templator, config);
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
        internal override Task<PositionSkill> Populate()
        {
            return Task.FromResult(new PositionSkill()
            {
                Id = Id,
                PositionId = PositionId,
                SkillId = SkillId,
                Description = Description,
                TextTypeId = TextTypeId,
            });
        }

        internal override Task Read(PositionSkill entity)
        {
            Id = entity.Id;
            PositionId = entity.PositionId;
            SkillId = entity.SkillId;
            Description = entity.Description;
            Skill = entity.Skill;
            TextTypeId = entity.TextTypeId;
            return Task.CompletedTask;
        }
    }
    public class SuggestAddSkillsForPositionViewModel : ReactiveObject
    {
        public Interaction<string, bool> Alert { get; } = new Interaction<string, bool>();
        public Guid PositionId { get; set; }
        public ReactiveCommand<Unit, Unit> SuggestSkills { get; }
        public ReactiveCommand<Unit, Unit> AddSelectedSkills { get; }
        public ObservableCollection<SkillViewModel> Skills { get; } = new ObservableCollection<SkillViewModel>();
        protected ISkillsBusinessFacade SkillFacade { get; }
        protected IBusinessRepositoryFacade<PositionSkill, Guid> PositionSkillFacade { get; }
        protected ILogger Logger { get; }
        public SuggestAddSkillsForPositionViewModel(ISkillsBusinessFacade skillFacade, IBusinessRepositoryFacade<PositionSkill, Guid> positionSkillFacade, ILogger<SuggestAddSkillsForPositionViewModel> logger)
        {
            SuggestSkills = ReactiveCommand.CreateFromTask(DoSuggestSkills);
            AddSelectedSkills = ReactiveCommand.CreateFromTask(DoAddSelectedSkills);
            SkillFacade = skillFacade;
            PositionSkillFacade = positionSkillFacade;
            Logger = logger;
        }
        protected async Task DoSuggestSkills(CancellationToken token)
        {
            try
            {
                Skills.Clear();
                foreach(var skill in await SkillFacade.GetSkillsExcludingPosition(PositionId, token: token))
                {
                    var vm = new SkillViewModel(Logger, SkillFacade, skill);
                    await vm.Load.Execute().GetAwaiter();
                    Skills.Add(vm);
                }
            }
            catch(Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                await Alert.Handle(ex.Message).GetAwaiter();
            }
        }
        protected async Task DoAddSelectedSkills(CancellationToken token)
        {
            try
            {
                foreach(var skill in Skills.Where(s => s.IsSelected).ToArray())
                {
                    var ps = new PositionSkill()
                    {
                        PositionId = PositionId,
                        SkillId = skill.Id
                    };
                    await PositionSkillFacade.Add(ps, token: token);
                    Skills.Remove(skill);
                }
            }
            catch(Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                await Alert.Handle(ex.Message).GetAwaiter();
            }
        }
    }
    public class PositionSkillsViewModel : EntitiesDefaultViewModel<Guid, PositionSkill, PositionSkillViewModel, AddPositionSkillViewModel>
    {
        public ReactiveCommand<Unit, Unit> ExtractSkills { get; }
        public ReactiveCommand<RawSkillViewModel, Unit> AddRawSkill { get; }
        public ICommand ToggleOpen => ReactiveCommand.Create(() => IsOpen = !IsOpen);
        protected IResumeEnricher Enricher { get; }
        protected IBusinessRepositoryFacade<Skill, Guid> SkillFacade { get; }
        public Guid PositionId
        {
            get => AddViewModel.PositionId;
            set
            {
                AddViewModel.PositionId = value;
                SuggestAddSkillsVM.PositionId = value;
            }
        }
        private string description = string.Empty;
        public string Description
        {
            get => description;
            set => this.RaiseAndSetIfChanged(ref description, value);
        }
        private bool canExtractSkills;
        public bool CanExtractSkills
        {
            get => canExtractSkills;
            set => this.RaiseAndSetIfChanged(ref canExtractSkills, value);
        }
        private bool isLoading;
        public bool IsLoading
        {
            get => isLoading;
            set => this.RaiseAndSetIfChanged(ref isLoading, value);
        }
        public SuggestAddSkillsForPositionViewModel SuggestAddSkillsVM { get; }
        protected IDocumentTemplator Templator { get; }
        protected IConfiguration Config { get; }
        public PositionSkillsViewModel(AddPositionSkillViewModel addViewModel, IDocumentTemplator templator, IConfiguration config,
            SuggestAddSkillsForPositionViewModel suggestAddSkillsVM, 
            IBusinessRepositoryFacade<PositionSkill, Guid> facade, IBusinessRepositoryFacade<Skill, Guid> skillFacade, 
            ILogger<EntitiesViewModel<Guid, PositionSkill, PositionSkillViewModel, IBusinessRepositoryFacade<PositionSkill, Guid>>> logger, 
            IResumeEnricher enricher) : base(addViewModel, facade, logger)
        {
            Templator = templator;
            Config = config;
            ExtractSkills = ReactiveCommand.CreateFromTask(DoExtractSkills);
            Enricher = enricher;
            SkillFacade = skillFacade;
            SuggestAddSkillsVM = suggestAddSkillsVM;
            AddRawSkill = ReactiveCommand.CreateFromTask<RawSkillViewModel>(DoAddRawSkill);

        }
        private bool isOpen;
        public bool IsOpen
        {
            get => isOpen;
            set => this.RaiseAndSetIfChanged(ref isOpen, value);
        }
        protected override Pager? GetPager()
        {
            return null;
        }
        public ObservableCollection<RawSkillViewModel> RawSkills { get; } = new ObservableCollection<RawSkillViewModel>();
        protected async Task DoAddRawSkill(RawSkillViewModel raw, CancellationToken token)
        {
            try
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
                }
                var ps = new PositionSkill()
                {
                    PositionId = PositionId,
                    SkillId = skill.Id
                };
                await Facade.Add(ps, token: token);
                RawSkills.Remove(raw);
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
            try
            {
                IsLoading = true;
                if (!CanExtractSkills)
                {
                    throw new InvalidOperationException();
                }
                RawSkills.Clear();
                CanExtractSkills = false;
                var skills = await Enricher.ExtractSkills(Description, token);
                if (skills?.Length > 0)
                {
                    var sks = skills.ToList();
                    sks.RemoveAll(s => Entities.Any(e => string.Compare(e.Skill.Name, s, StringComparison.OrdinalIgnoreCase) == 0));
                    RawSkills.AddRange(sks.Select(s => new RawSkillViewModel(Logger, PositionId, s, AddRawSkill)));
                }
            }
            catch(Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                await Alert.Handle(ex.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }
        protected override async Task<PositionSkillViewModel> Construct(PositionSkill entity, CancellationToken token)
        {
            var vm = new PositionSkillViewModel(Logger, Templator, Config, Facade, entity);
           
            return vm;
        }
        protected override Func<IQueryable<PositionSkill>, IOrderedQueryable<PositionSkill>>? OrderBy()
        {
            return e => e.OrderBy(c => c.Skill.Name);
        }
        protected override Func<IQueryable<PositionSkill>, IQueryable<PositionSkill>>? PropertiesToLoad()
        {
            return e => e.Include(x => x.Skill);
        }
        protected override async Task<Expression<Func<PositionSkill, bool>>?> FilterCondition()
        {
            return e => e.PositionId == PositionId;
        }
    }
    public class RawSkillViewModel : ReactiveObject
    {
        public ReactiveCommand<Unit, Unit> AddSkill { get; }
        private string name = string.Empty;
        public string Name
        {
            get => name;
            set => this.RaiseAndSetIfChanged(ref name, value);
        }
        private bool isSelected;
        public bool IsSelected
        {
            get => isSelected;
            set => this.RaiseAndSetIfChanged(ref isSelected, value);
        }
        public Guid PositionId { get; set; }
        protected ReactiveCommand<RawSkillViewModel, Unit> Add { get; }
        public RawSkillViewModel(ILogger logger, Guid positionId, string name, ReactiveCommand<RawSkillViewModel, Unit> addCommand)
        {
            Name = name.Trim();
            PositionId = positionId;
            Add = addCommand;
            AddSkill = ReactiveCommand.CreateFromTask(async() => await Add.Execute(this).GetAwaiter());
        }
    }
    public class SearchSelectSkillViewModel : EntitySelectSearchViewModel<Guid, Skill, AddSkillViewModel>
    {
        public SearchSelectSkillViewModel(IBusinessRepositoryFacade<Skill, Guid> facade, AddSkillViewModel addViewModel, ILogger<EntitySelectSearchViewModel<Guid, Skill, IBusinessRepositoryFacade<Skill, Guid>, AddSkillViewModel>> logger) : base(facade, addViewModel, logger)
        {
        }

        protected override async Task<IEnumerable<Skill>> DoSearch(string? text, CancellationToken token = default)
        {
            if (string.IsNullOrWhiteSpace(text))
                return [];
            SearchString = text;
            var result = await Facade.Get(page: new Pager() { Page = 1, Size = 5 },
                filter: q => q.Name.StartsWith(text), token: token);
            if (result != null)
                return result.Entities.ToArray();
            return [];
        }
    }
}
