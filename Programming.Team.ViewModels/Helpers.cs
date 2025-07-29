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

namespace Programming.Team.ViewModels
{
    public interface ITextual : IHasTextType
    {
        string? Text { get; set; }
    }
    public class DataGridRequest<TKey, TEntity>
        where TKey : struct
        where TEntity : Entity<TKey>, new()
    {
        public Pager? Pager { get; set; }
        public Expression<Func<TEntity, bool>>? Filter { get; set; }
        public Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? OrderBy { get; set; }
    }
    public class ViewModelResult<TKey, TEntity, TViewModel> : IPagedResult<TViewModel>
        where TKey : struct
        where TEntity : Entity<TKey>, new()
        where TViewModel : IEntityViewModel<TKey, TEntity>
    {
        public IEnumerable<TViewModel> Entities { get; set; } = null!;
        public int? Count { get; set; }
        public int? PageSize { get; set; }
        public int? Page { get; set; }
    }
    public interface IEntityViewModel<TKey, TEntity> : IEntity<TKey>, IHandleObservableErrors, IReactiveObject, INotifyPropertyChanged, INotifyPropertyChanging, Splat.IEnableLogger
        where TKey : struct
        where TEntity : Entity<TKey>, new()
    {
        Interaction<string, bool> Alert { get; }
        ReactiveCommand<Unit, TEntity?> Load { get; }
        ReactiveCommand<Unit, Unit> Delete { get; }
        event EventHandler<TKey>? Deleted;
        ReactiveCommand<Unit, TEntity> Update { get; }
        event EventHandler<TEntity>? Updated;
        bool IsSelected { get; set; }
    }
    public abstract class EntityViewModel<TKey, TEntity, TFacade> : ReactiveObject, IEntityViewModel<TKey, TEntity>
        where TKey : struct
        where TEntity : Entity<TKey>, new()
        where TFacade : IBusinessRepositoryFacade<TEntity, TKey>
    {
        public Interaction<string, bool> Alert { get; } = new Interaction<string, bool>();
        protected TFacade Facade { get; }
        public ReactiveCommand<Unit, TEntity?> Load { get; }
        public ReactiveCommand<Unit, Unit> Delete { get; }
        public ReactiveCommand<Unit, TEntity> Update { get; }
        protected ILogger Logger { get; }
        protected EntityViewModel(ILogger logger, TFacade facade)
        {
            Logger = logger;
            Facade = facade;
            Load = ReactiveCommand.CreateFromTask(DoLoad);
            Delete = ReactiveCommand.CreateFromTask(DoDelete);
            Update = ReactiveCommand.CreateFromTask(DoUpdate);
        }
        public EntityViewModel(ILogger logger, TFacade facade, TKey id)
            : this(logger, facade)
        {
            Id = id;
        }
        protected TEntity? InitializedEntity { get; set; }
        public EntityViewModel(ILogger logger, TFacade facade, TEntity entity)
            :this(logger, facade, entity.Id)
        {
            InitializedEntity = entity;
        }
        protected virtual Func<IQueryable<TEntity>, IQueryable<TEntity>>? PropertiesToLoad()
        {
            return null;
        }
        protected virtual async Task<TEntity?> DoLoad(CancellationToken token)
        {
            try
            {

                var entity = InitializedEntity ?? await Facade.GetByID(Id, properites: PropertiesToLoad(), token: token);
                InitializedEntity = null;
                if (entity != null)
                    await Read(entity);
                return entity;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                await Alert.Handle(ex.Message).GetAwaiter();
            }
            return null;
        }
        protected virtual async Task DoDelete(CancellationToken token)
        {
            try
            {
                await Facade.Delete(Id, token: token);
                Deleted?.Invoke(this, Id);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                await Alert.Handle(ex.Message).GetAwaiter();
            }
        }
        protected virtual async Task<TEntity> DoUpdate(CancellationToken token)
        {
            try
            {
                var entity = await Populate();
                entity = await Facade.Update(entity, properites: PropertiesToLoad(), token: token);
                entity.Id = Id;
                await Read(entity);
                Updated?.Invoke(this, entity);
                return entity;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                await Alert.Handle(ex.Message).GetAwaiter();
            }
            return new TEntity();
        }
        private TKey id;
        public TKey Id
        {
            get => id;
            set => this.RaiseAndSetIfChanged(ref id, value);
        }
        private bool isSelected;

        public event EventHandler<TKey>? Deleted;
        public event EventHandler<TEntity>? Updated;

        public bool IsSelected
        {
            get => isSelected;
            set => this.RaiseAndSetIfChanged(ref isSelected, value);
        }

        internal abstract Task<TEntity> Populate();
        internal abstract Task Read(TEntity entity);
    }
    public abstract class EntityViewModel<TKey, TEntity> : EntityViewModel<TKey, TEntity, IBusinessRepositoryFacade<TEntity, TKey>>
        where TKey : struct
        where TEntity : Entity<TKey>, new()
    {
        public EntityViewModel(ILogger logger, IBusinessRepositoryFacade<TEntity, TKey> facade, TKey id)
            : base(logger, facade, id)
        { }
        public EntityViewModel(ILogger logger, IBusinessRepositoryFacade<TEntity, TKey> facade, TEntity entity)
            :base(logger, facade, entity) { }
    }

    public abstract class SelectEntitiesViewModel<TKey, TEntity, TViewModel, TFacade> : ReactiveObject
        where TKey : struct
        where TEntity : Entity<TKey>, new()
        where TViewModel : IEntityViewModel<TKey, TEntity>
        where TFacade : IBusinessRepositoryFacade<TEntity, TKey>
    {
        public Interaction<string, bool> Alert { get; } = new Interaction<string, bool>();
        protected ILogger Logger { get; }
        protected TFacade Facade { get; }
        public ReactiveCommand<DataGridRequest<TKey, TEntity>, ViewModelResult<TKey, TEntity, TViewModel>?> Fetch { get; }
        protected readonly CompositeDisposable disposable = new CompositeDisposable();
        public ObservableCollection<TViewModel> Selected { get; } = new ObservableCollection<TViewModel>();
        public SelectEntitiesViewModel(TFacade facade, ILogger<SelectEntitiesViewModel<TKey, TEntity, TViewModel, TFacade>> logger)
        {
            Logger = logger;
            Facade = facade;
            Fetch = ReactiveCommand.CreateFromTask<DataGridRequest<TKey, TEntity>, ViewModelResult<TKey, TEntity, TViewModel>?>(DoFetch);
        }
        protected virtual Func<IQueryable<TEntity>, IQueryable<TEntity>>? PropertiesToLoad()
        {
            return null;
        }
        protected virtual Task<Expression<Func<TEntity, bool>>?> FilterCondition()
        {
            return Task.FromResult<Expression<Func<TEntity, bool>>?>(null);
        }
        protected virtual async Task<ViewModelResult<TKey, TEntity, TViewModel>?> DoFetch(DataGridRequest<TKey, TEntity> request, CancellationToken token = default)
        {
            try
            {
                var filter = await FilterCondition();
                if (filter != null)
                {
                    if (request.Filter == null)
                    {
                        request.Filter = filter;
                    }
                    else
                    {
                        request.Filter = (filter).CombineWithAnd(request.Filter);
                    }
                }
                else
                    filter = request.Filter;
                
                var resp = await Facade.Get(page: request.Pager, filter: filter, orderBy: request.OrderBy, properites: PropertiesToLoad(), token: token);
                if (resp == null)
                    return null;
                ViewModelResult<TKey, TEntity, TViewModel> vmr = new ViewModelResult<TKey, TEntity, TViewModel>();
                vmr.Page = resp.Page;
                vmr.PageSize = resp.PageSize;
                vmr.Count = resp.Count;
                List<TViewModel> vms = new List<TViewModel>();
                foreach (var en in resp.Entities)
                {
                    var existing = Selected.SingleOrDefault(e => e.Id.Equals(en.Id));
                    if (existing == null)
                    {
                        var vm = await ConstructViewModel(en);
                        await WireupViewModel(vm);
                        vms.Add(vm);
                    }
                    else
                    {
                        vms.Add(existing);
                    }
                }
                vmr.Entities = vms;
                return vmr;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                await Alert.Handle(ex.Message).GetAwaiter();
            }
            return null;
        }
        private bool isInitialzed;
        public bool IsInitialzed
        {
            get => isInitialzed;
            protected set => this.RaiseAndSetIfChanged(ref isInitialzed, value);
        }
        public virtual async Task SetSelected(TKey[] ids, CancellationToken token = default)
        {
            try
            {
                foreach (var item in Selected.Where(e => !ids.Any(i => e.Id.Equals(i))).ToArray())
                {
                    Selected.Remove(item);
                }
                if (ids.Length == 0)
                {
                    IsInitialzed = true;
                    return;
                }
                    
                var missingIds = ids.Where(i => !Selected.Any(e => e.Id.Equals(i))).ToArray();
                var rs = await Facade.Get(filter: q => missingIds.Contains(q.Id), properites: PropertiesToLoad(), token: token);
                foreach (var e in rs.Entities)
                {
                    var vm = await ConstructViewModel(e);
                    vm.IsSelected = true;
                    await WireupViewModel(vm);
                    Insert(vm);
                }
                IsInitialzed = true;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                await Alert.Handle(ex.Message).GetAwaiter();
            }
        }
        protected abstract Task<TViewModel> ConstructViewModel(TEntity entity);
        protected virtual async Task WireupViewModel(TViewModel viewModel)
        {
            viewModel.WhenPropertyChanged(p => p.IsSelected).Subscribe(p =>
            {
                if (p.Value)
                    Insert(p.Sender);
                else
                    Selected.Remove(p.Sender);
            }).DisposeWith(disposable);
            await viewModel.Load.Execute().GetAwaiter();
        }
        protected readonly object synch = new object();
        protected virtual void Insert(TViewModel vm)
        {
            lock (synch)
            {
                if (!Selected.Any(s => s.Id.Equals(vm.Id)))
                    Selected.Add(vm);
            }
        }
        ~SelectEntitiesViewModel()
        {
            disposable.Dispose();
        }
    }
    public abstract class SelectEntitiesViewModel<TKey, TEntity, TViewModel> : SelectEntitiesViewModel<TKey, TEntity, TViewModel, IBusinessRepositoryFacade<TEntity, TKey>>
        where TKey : struct
        where TEntity : Entity<TKey>, new()
        where TViewModel : IEntityViewModel<TKey, TEntity>
    {
        protected SelectEntitiesViewModel(IBusinessRepositoryFacade<TEntity, TKey> facade,
            ILogger<SelectEntitiesViewModel<TKey, TEntity, TViewModel, IBusinessRepositoryFacade<TEntity, TKey>>> logger) :
            base(facade, logger)
        {
        }
    }
    public interface IManageEntityViewModel<TKey, TEntity> : IHandleObservableErrors, IReactiveObject, INotifyPropertyChanged, INotifyPropertyChanging, Splat.IEnableLogger
        where TKey : struct
        where TEntity : Entity<TKey>, new()
    {
        Interaction<string, bool> Alert { get; }
        ReactiveCommand<DataGridRequest<TKey, TEntity>, RepositoryResultSet<TKey, TEntity>?> Fetch { get; }
    }
    public interface IManageEntityViewModel<TKey, TEntity, out TAddVM> : IManageEntityViewModel<TKey, TEntity>
        where TKey: struct
        where TEntity: Entity<TKey>, new()
        where TAddVM: IAddEntityViewModel<TKey, TEntity>
    {
        Func<Task>? Reload { get; set; }
        TAddVM AddViewModel { get; }
    }

    public static class ExpressionExtensions
    {
        public static Expression<Func<T, bool>>? CombineWithAnd<T>(
            this Expression<Func<T, bool>>? first,
            Expression<Func<T, bool>>? second)
        {
            if (first == null) return second;
            if (second == null) return first;

            var parameter = Expression.Parameter(typeof(T), "entity");

            var combinedBody = Expression.AndAlso(
                Expression.Invoke(first, parameter),
                Expression.Invoke(second, parameter)
            );

            return Expression.Lambda<Func<T, bool>>(combinedBody, parameter);
        }

        public static Expression<Func<T, bool>>? CombineWithOr<T>(
            this Expression<Func<T, bool>>? first,
            Expression<Func<T, bool>>? second)
        {
            if (first == null) return second;
            if (second == null) return first;

            var parameter = Expression.Parameter(typeof(T), "entity");

            var combinedBody = Expression.OrElse(
                Expression.Invoke(first, parameter),
                Expression.Invoke(second, parameter)
            );

            return Expression.Lambda<Func<T, bool>>(combinedBody, parameter);
        }
    }

    public class ManageEntityViewModel<TKey, TEntity, TFacade> : ReactiveObject, IManageEntityViewModel<TKey, TEntity>
        where TKey : struct
        where TEntity : Entity<TKey>, new()
        where TFacade : IBusinessRepositoryFacade<TEntity, TKey>
    {
        public Interaction<string, bool> Alert { get; } = new Interaction<string, bool>();
        protected virtual Task<Expression<Func<TEntity, bool>>?> GetBaseFilterCondition()
        {
            return Task.FromResult<Expression<Func<TEntity, bool>>?>(null);
        }
        protected virtual Func<IQueryable<TEntity>, IQueryable<TEntity>>? PropertiesToLoad()
        {
            return null;
        }
        public ReactiveCommand<DataGridRequest<TKey, TEntity>, RepositoryResultSet<TKey, TEntity>?> Fetch { get; }
        protected ILogger Logger { get; }
        protected TFacade Facade { get; }
        public ManageEntityViewModel(TFacade facade, ILogger<ManageEntityViewModel<TKey, TEntity, TFacade>> logger)
        {
            Logger = logger;
            Facade = facade;
            Fetch = ReactiveCommand.CreateFromTask<DataGridRequest<TKey, TEntity>, RepositoryResultSet<TKey, TEntity>?>(DoFetch);
        }
        protected virtual async Task<RepositoryResultSet<TKey, TEntity>?> DoFetch(DataGridRequest<TKey, TEntity> request, CancellationToken token)
        {
            try
            {
                return await Facade.Get(page: request.Pager, properites: PropertiesToLoad(), filter: (await GetBaseFilterCondition()).CombineWithAnd(request.Filter), orderBy: request.OrderBy, token: token);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                await Alert.Handle(ex.Message).GetAwaiter();
            }
            return null;
        }
    }
    public class ManageEntityViewModel<TKey, TEntity, TFacade, TAddVM> : ManageEntityViewModel<TKey, TEntity, TFacade>, IManageEntityViewModel<TKey, TEntity, TAddVM>
        where TKey : struct
        where TEntity : Entity<TKey>, new()
        where TFacade : IBusinessRepositoryFacade<TEntity, TKey>
        where TAddVM : IAddEntityViewModel<TKey, TEntity>
    {
        public ManageEntityViewModel(TFacade facade, TAddVM addVM, ILogger<ManageEntityViewModel<TKey, TEntity, TFacade>> logger) : base(facade, logger)
        {
            AddViewModel = addVM;
            AddViewModel.Added += async (s, e) =>
            {
                if (Reload != null)
                    await Reload();
            };
        }

        public Func<Task>? Reload { get; set; }

        public TAddVM AddViewModel{ get; }
    }
    public class ManageEntityViewModelWithAdd<TKey, TEntity, TAddVM> : ManageEntityViewModel<TKey, TEntity, IBusinessRepositoryFacade<TEntity, TKey>, TAddVM>
        where TKey : struct
        where TEntity : Entity<TKey>, new()
        where TAddVM : IAddEntityViewModel<TKey, TEntity>
    {
        public ManageEntityViewModelWithAdd(IBusinessRepositoryFacade<TEntity, TKey> facade, TAddVM addVM, ILogger<ManageEntityViewModel<TKey, TEntity, IBusinessRepositoryFacade<TEntity, TKey>>> logger) : base(facade, addVM, logger)
        {
        }
    }
    public class ManageEntityViewModel<TKey, TEntity> : ManageEntityViewModel<TKey, TEntity, IBusinessRepositoryFacade<TEntity, TKey>>
        where TKey : struct
        where TEntity : Entity<TKey>, new()
    {
        public ManageEntityViewModel(IBusinessRepositoryFacade<TEntity, TKey> facade, ILogger<ManageEntityViewModel<TKey, TEntity, IBusinessRepositoryFacade<TEntity, TKey>>> logger) : base(facade, logger)
        {
        }
    }
    public interface IAddEntityViewModel<TKey, TEntity> : IEntity<TKey>, IHandleObservableErrors, IReactiveObject, INotifyPropertyChanged, INotifyPropertyChanging, Splat.IEnableLogger
        where TKey : struct
        where TEntity : Entity<TKey>, new()
    {
        Interaction<string, bool> Alert { get; }
        ReactiveCommand<Unit, TEntity?> Add { get; }
        event EventHandler<TEntity>? Added;
        bool IsOpen { get; set; }
        ReactiveCommand<Unit, Unit>  Init { get; }
        ReactiveCommand<Unit, Unit> Cancel { get; }
        void SetText(string text);
        bool CanAdd { get; }
    }
    public abstract class AddEntityViewModel<TKey, TEntity, TFacade> : ReactiveObject, IAddEntityViewModel<TKey, TEntity>
        where TKey : struct
        where TEntity : Entity<TKey>, new()
        where TFacade : IBusinessRepositoryFacade<TEntity, TKey>
    {
        public virtual bool CanAdd { get => true; }
        private bool isOpen;
        public bool IsOpen
        {
            get => isOpen;
            set => this.RaiseAndSetIfChanged(ref isOpen, value);
        }
        public Interaction<string, bool> Alert { get; } = new Interaction<string, bool>();

        public ReactiveCommand<Unit, TEntity?> Add { get; }

        private TKey id;
        public TKey Id
        {
            get => id;
            set => this.RaiseAndSetIfChanged(ref id, value);
        }

        public event EventHandler<TEntity>? Added;
        protected TFacade Facade { get; }
        protected ILogger Logger { get; }
        public ReactiveCommand<Unit, Unit> Init { get; }
        public ReactiveCommand<Unit, Unit> Cancel { get; }
        public AddEntityViewModel(TFacade facade, ILogger<AddEntityViewModel<TKey, TEntity, TFacade>> logger)
        {
            Facade = facade;
            Logger = logger;
            Add = ReactiveCommand.CreateFromTask(DoAdd);
            Init = ReactiveCommand.CreateFromTask(DoInit);
            Cancel = ReactiveCommand.CreateFromTask(DoCancel);
        }
        protected virtual async Task DoInit(CancellationToken token)
        {
            await Clear();
        }
        protected virtual async Task DoCancel(CancellationToken token)
        {
            await Clear();
            IsOpen = false;
        }
        protected virtual async Task<TEntity?> DoAdd(CancellationToken token)
        {
            try
            {
                if (CanAdd)
                {
                    var e = await ConstructEntity();
                    await Facade.Add(e, token: token);
                    Added?.Invoke(this, e);
                    await Clear();
                    IsOpen = false;
                    return e;
                }
                else
                {
                    await Alert.Handle("Validation Error").GetAwaiter();
                    return null;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                await Alert.Handle(ex.Message).GetAwaiter();
            }
            return null;
        }
        protected abstract Task<TEntity> ConstructEntity();
        protected abstract Task Clear();
        public virtual void SetText(string text) { }
    }
    public abstract class AddEntityViewModel<TKey, TEntity> : AddEntityViewModel<TKey, TEntity, IBusinessRepositoryFacade<TEntity, TKey>>
        where TKey : struct
        where TEntity : Entity<TKey>, new()
    {
        protected AddEntityViewModel(IBusinessRepositoryFacade<TEntity, TKey> facade, ILogger<AddEntityViewModel<TKey, TEntity, IBusinessRepositoryFacade<TEntity, TKey>>> logger) : base(facade, logger)
        {
        }
    }
    public abstract class AddUserPartionedEntity<TKey, TEntity> : AddEntityViewModel<TKey, TEntity, IBusinessRepositoryFacade<TEntity,TKey>>, IUserPartionedEntity
        where TKey : struct
        where TEntity : Entity<TKey>, IUserPartionedEntity, new()
    {
        protected AddUserPartionedEntity(IBusinessRepositoryFacade<TEntity, TKey> facade, ILogger<AddEntityViewModel<TKey, TEntity, IBusinessRepositoryFacade<TEntity, TKey>>> logger) : base(facade, logger)
        {
            
        }
        private Guid userId;
        public Guid UserId
        {
            get => userId;
            set => this.RaiseAndSetIfChanged(ref userId, value);
        }
        protected override async Task DoInit(CancellationToken token)
        {
            UserId = await Facade.GetCurrentUserId(token: token) ?? throw new UnauthorizedAccessException();
            await base.DoInit(token);
        }
    }
    public interface IEntityLoaderViewModel<TKey, TEntity, TViewModel>
        where TKey : struct
        where TEntity : Entity<TKey>, new()
        where TViewModel : class, IEntityViewModel<TKey, TEntity>
    {
        Interaction<string, bool> Alert { get; }
        TKey? Id { get; }
        ReactiveCommand<TKey, Unit> Load { get; }
        TViewModel? ViewModel { get; }
    }
    public abstract class EntityLoaderViewModel<TKey, TEntity, TViewModel, TFacade> : ReactiveObject, IEntityLoaderViewModel<TKey, TEntity, TViewModel>
        where TKey : struct
        where TEntity : Entity<TKey>, new()
        where TViewModel : class, IEntityViewModel<TKey, TEntity>
        where TFacade : IBusinessRepositoryFacade<TEntity, TKey>
    {
        public Interaction<string, bool> Alert { get; } = new Interaction<string, bool>();

        private TKey? id;

        public TKey? Id
        {
            get => id;
            protected set => this.RaiseAndSetIfChanged(ref id, value);
        }

        public ReactiveCommand<TKey, Unit> Load{ get; }
        private TViewModel? viewModel;
        public TViewModel? ViewModel
        {
            get => viewModel;
            protected set => this.RaiseAndSetIfChanged(ref viewModel, value);
        }
        protected TFacade Facade { get; }
        protected ILogger Logger { get; }
        public EntityLoaderViewModel(TFacade facade, ILogger<EntityLoaderViewModel<TKey, TEntity, TViewModel, TFacade>> logger)
        {
            Facade = facade;
            Logger = logger;
            Load = ReactiveCommand.CreateFromTask<TKey>(DoLoad);
        }
        protected abstract TViewModel Construct(TEntity entity);
        protected virtual async Task DoLoad(TKey key, CancellationToken token)
        {
            try
            {
                var entity = await Facade.GetByID(key, token: token);
                if (entity == null)
                {
                    Id = null;
                    ViewModel = null;
                }
                else
                {
                    Id = entity.Id;
                    ViewModel = Construct(entity);
                    await ViewModel.Load.Execute().GetAwaiter();
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                await Alert.Handle(ex.Message).GetAwaiter();
            }
        }
    }
    public abstract class EntitiesViewModel<TKey, TEntity, TViewModel> : EntitiesViewModel<TKey, TEntity, TViewModel, IBusinessRepositoryFacade<TEntity, TKey>>
        where TKey : struct
        where TEntity : Entity<TKey>, new()
        where TViewModel : IEntityViewModel<TKey, TEntity>
    {
        protected EntitiesViewModel(IBusinessRepositoryFacade<TEntity, TKey> facade, ILogger<EntitiesViewModel<TKey, TEntity, TViewModel, IBusinessRepositoryFacade<TEntity, TKey>>> logger) : base(facade, logger)
        {
        }
    }
    public class PaginationViewModel : ReactiveObject
    {
        private int? pageSize = 5;
        public int? PageSize
        {
            get => pageSize;
            set
            {
                bool changed = pageSize != value;
                this.RaiseAndSetIfChanged(ref pageSize, value);
                if (changed)
                {
                    this.RaisePropertyChanged(nameof(PageCount));
                    this.RaisePropertyChanged(nameof(Pager));
                }
            }
        }
        private int itemCount = 0;
        public int ItemCount
        {
            get => itemCount;
            set
            {
                bool changed = itemCount != value;
                this.RaiseAndSetIfChanged(ref itemCount, value);
                if (changed)
                {
                    CurrentPage = 1;
                    this.RaisePropertyChanged(nameof(PageCount));
                    this.RaisePropertyChanged(nameof(Pager));
                }
            }
        }   
        public int PageCount
        {
            get
            {
                if (PageSize.HasValue && PageSize.Value > 0)
                {
                    return (int)Math.Ceiling((double)ItemCount / PageSize.Value);
                }
                return 0;
            }
        }
        private int currentPage = 1;
        public int CurrentPage
        {
            get => currentPage;
            set
            {
                bool changed = currentPage != value;
                if (value > 0 && value <= PageCount)
                {
                    this.RaiseAndSetIfChanged(ref currentPage, value);
                }
                else if (value <= 0)
                {
                    changed = currentPage != 1;
                    this.RaiseAndSetIfChanged(ref currentPage, 1);
                }
                if(changed)
                    this.RaisePropertyChanged(nameof(Pager));   
            }
        }
        public Pager? Pager
        {
            get
            {
                if (PageSize == null)
                    return null;
                return new Pager()
                {
                    Size = PageSize.Value,
                    Page = CurrentPage
                };
            }
        }
    }
    public abstract class EntitiesViewModel<TKey, TEntity, TViewModel, TFacade> : ReactiveObject
        where TKey : struct
        where TEntity : Entity<TKey>, new()
        where TFacade : IBusinessRepositoryFacade<TEntity, TKey>
        where TViewModel : IEntityViewModel<TKey, TEntity>
    {
        public Interaction<string, bool> Alert { get; } = new Interaction<string, bool>();
        protected ILogger Logger { get; }
        protected TFacade Facade { get; }
        public ObservableCollection<TViewModel> Entities { get; } = new ObservableCollection<TViewModel>();
        public ReactiveCommand<Unit, Unit> Load { get; }
        public PaginationViewModel Pagination { get; } = new PaginationViewModel();
        private readonly CompositeDisposable disposable = new CompositeDisposable();
        public EntitiesViewModel(TFacade facade, ILogger<EntitiesViewModel<TKey, TEntity, TViewModel, TFacade>> logger)
        {
            Facade = facade;
            Logger = logger;
            Load = ReactiveCommand.CreateFromTask(DoLoad);
            Pagination.WhenPropertyChanged(p => p.Pager).Subscribe(async p =>
            {
                if(p.Value != null)
                    await Load.Execute().GetAwaiter();  
            }).DisposeWith(disposable);
        }
        ~EntitiesViewModel() 
        {
            disposable.Dispose();
        }
        protected virtual Pager? GetPager()
        {
            return Pagination.Pager;
        }
        protected virtual Func<IQueryable<TEntity>, IQueryable<TEntity>>? PropertiesToLoad()
        {
            return null;
        }
        protected virtual Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? OrderBy()
        {
            return null;
        }
        protected virtual Task<Expression<Func<TEntity, bool>>?> FilterCondition()
        {
            return Task.FromResult<Expression<Func<TEntity, bool>>?>(null);
        }
        public IEnumerable<TEntity>? InitialEntities { get; set; }
        private bool isLoading = false;
        private readonly object synch = new object();
        protected bool CanLoad()
        {
            lock (synch)
            {
                if (!isLoading)
                {
                    isLoading = true;
                    return true;
                }
                return false;
            }
        }
        protected virtual async Task DoLoad(CancellationToken token)
        {
            try
            {
                if (CanLoad())
                {
                    try
                    {
                        if(Entities.Count > 0)
                            Entities.Clear();
                        var rs = InitialEntities;
                        if (rs == null)
                        {
                            var rse = await Facade.Get(filter: await FilterCondition(), page: GetPager(), orderBy: OrderBy(), properites: PropertiesToLoad(), token: token);
                            Pagination.ItemCount = rse?.Count ?? 0;
                            Pagination.PageSize = rse?.PageSize;
                            rs = rse?.Entities;
                        }
                        else
                        {
                            Pagination.PageSize = null;
                            Pagination.ItemCount = rs.Count();
                        }
                        InitialEntities = null;
                        foreach (var e in rs ?? [])
                        {
                            var vm = await Construct(e, token);
                            await vm.Load.Execute().GetAwaiter();
                            vm.Deleted += async (s, id) => await Load.Execute().GetAwaiter();
                            vm.Updated += async (s, ne) => await Load.Execute().GetAwaiter();
                            Entities.Add(vm);
                        }
                    }
                    finally
                    {
                        isLoading = false;
                    }
                }
            }
            catch(Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                await Alert.Handle(ex.Message).GetAwaiter();
            }
        }
        protected abstract Task<TViewModel> Construct(TEntity entity, CancellationToken token);
    }
    public abstract class EntitiesDefaultViewModel<TKey, TEntity, TViewModel, TAddViewModel> : EntitiesViewModel<TKey, TEntity, TViewModel, IBusinessRepositoryFacade<TEntity, TKey>, TAddViewModel>
        where TKey : struct
        where TEntity : Entity<TKey>, new()
        where TViewModel : IEntityViewModel<TKey, TEntity>
        where TAddViewModel : IAddEntityViewModel<TKey, TEntity>
    {
        protected EntitiesDefaultViewModel(TAddViewModel addViewModel, IBusinessRepositoryFacade<TEntity, TKey> facade, ILogger<EntitiesViewModel<TKey, TEntity, TViewModel, IBusinessRepositoryFacade<TEntity, TKey>>> logger) : base(addViewModel, facade, logger)
        {
        }
    }
    public abstract class EntitiesViewModel<TKey, TEntity, TViewModel, TFacade, TAddViewModel> : EntitiesViewModel<TKey, TEntity, TViewModel, TFacade>
        where TKey : struct
        where TEntity : Entity<TKey>, new()
        where TFacade : IBusinessRepositoryFacade<TEntity, TKey>
        where TViewModel: IEntityViewModel<TKey, TEntity>
        where TAddViewModel : IAddEntityViewModel<TKey, TEntity>
    {
        public ReactiveCommand<Unit, Unit> StartAdd { get; }
        protected EntitiesViewModel(TAddViewModel addViewModel, TFacade facade, ILogger<EntitiesViewModel<TKey, TEntity, TViewModel, TFacade>> logger) : base(facade, logger)
        {
            AddViewModel = addViewModel;
            AddViewModel.Added += async (s, e) =>
            {
                await Load.Execute().GetAwaiter();
            };
            StartAdd = ReactiveCommand.Create(() => { AddViewModel.IsOpen = true; });
        }

        public TAddViewModel AddViewModel { get; }
        
    }
    public abstract class EntitySelectSearchViewModel<TKey, TEntity, TFacade, TAddViewModel> : ReactiveObject
        where TKey: struct
        where TEntity: Entity<TKey>, new()
        where TFacade: IBusinessRepositoryFacade<TEntity, TKey>
        where TAddViewModel: IAddEntityViewModel<TKey, TEntity>
    {
        public Interaction<string, bool> Alert { get; } = new Interaction<string, bool>();
        protected ILogger Logger { get; }
        protected TFacade Facade { get; }
        public TAddViewModel AddViewModel { get; }
        public ReactiveCommand<string?, IEnumerable<TEntity>> Search { get; }
        public ReactiveCommand<Unit, Unit> StartAdd { get; }
        private string? searchString;
        public string? SearchString
        {
            get => searchString;
            set
            {
                this.RaiseAndSetIfChanged(ref searchString, value);
                this.RaisePropertyChanged(nameof(CanAdd));
            }
        }
        public EntitySelectSearchViewModel(TFacade facade, TAddViewModel addViewModel, ILogger<EntitySelectSearchViewModel<TKey, TEntity, TFacade, TAddViewModel>> logger)
        {
            Logger = logger;
            Facade = facade;
            AddViewModel = addViewModel;
            Search = ReactiveCommand.CreateFromTask<string?, IEnumerable<TEntity>>(StartSearch);
            StartAdd = ReactiveCommand.Create(() =>
            {
                AddViewModel.SetText(SearchString ?? "");
                AddViewModel.IsOpen = true;
                
            });
            AddViewModel.Added += AddViewModel_Added;
        }

        protected virtual void AddViewModel_Added(object? sender, TEntity e)
        {
            Selected = e;
            AddViewModel.IsOpen = false;
        }
        protected virtual async Task<IEnumerable<TEntity>> StartSearch(string? text, CancellationToken token = default)
        {
            try
            {
                SearchString = text;
                Selected = null;
                return await DoSearch(text, token);
            }
            catch(Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                await Alert.Handle(ex.Message).GetAwaiter();
            }
            return [];
        }
        protected abstract Task<IEnumerable<TEntity>> DoSearch(string? text, CancellationToken token = default);
        private TEntity? selected;
        public TEntity? Selected
        {
            get => selected;
            set
            {
                this.RaiseAndSetIfChanged(ref selected, value);
                this.RaisePropertyChanged(nameof(CanAdd));
            }
        }
        public virtual bool CanAdd
        {
            get => Selected == null && !string.IsNullOrWhiteSpace(SearchString);
        }
    }
    public abstract class EntitySelectSearchViewModel<TKey, TEntity, TAddViewModel> : EntitySelectSearchViewModel<TKey, TEntity, IBusinessRepositoryFacade<TEntity, TKey>, TAddViewModel>
        where TKey : struct
        where TEntity : Entity<TKey>, new()
        where TAddViewModel : IAddEntityViewModel<TKey, TEntity>
    {
        protected EntitySelectSearchViewModel(IBusinessRepositoryFacade<TEntity, TKey> facade, TAddViewModel addViewModel, ILogger<EntitySelectSearchViewModel<TKey, TEntity, IBusinessRepositoryFacade<TEntity, TKey>, TAddViewModel>> logger) : base(facade, addViewModel, logger)
        {
        }
    }
}
