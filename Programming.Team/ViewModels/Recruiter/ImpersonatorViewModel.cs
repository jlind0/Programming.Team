using DynamicData.Binding;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Programming.Team.Business.Core;
using Programming.Team.Core;
using Programming.Team.Data.Core;
using Programming.Team.ViewModels.Admin;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Programming.Team.ViewModels.Recruiter
{
    public class ImpersonatorViewModel : ManageEntityViewModel<Guid, User, IUserBusinessFacade>
    {
        public ReactiveCommand<Guid, Unit> Impersonate { get; }
        public ReactiveCommand<Unit, Unit> EndImpersonation { get; }
        public ReactiveCommand<Unit, Guid?> Load { get; }
        protected IContextFactory ContextFactory { get; }
        protected NavigationManager NavMan { get; }
        private User? impersonatedUser;
        public User? ImpersonatedUser
        {
            get => impersonatedUser;
            set => this.RaiseAndSetIfChanged(ref impersonatedUser, value);
        }
        public ImpersonatorViewModel(NavigationManager manage, IContextFactory factory, IUserBusinessFacade facade, ILogger<ManageEntityViewModel<Guid, User, IUserBusinessFacade>> logger) : base(facade, logger)
        {
            ContextFactory = factory;
            NavMan = manage;
            Impersonate = ReactiveCommand.CreateFromTask<Guid>(DoImpersonate);
            EndImpersonation = ReactiveCommand.CreateFromTask(DoEndImpersonation);
            Load = ReactiveCommand.CreateFromTask<Guid?>(DoLoadImpersonation);
        }
        protected override async Task<Expression<Func<User, bool>>?> GetBaseFilterCondition()
        {
            var realUserId = await Facade.GetCurrentUserId(fetchTrueUserId: true);
            if (realUserId == null)
                throw new InvalidOperationException();
            return q => q.Recruiters.Any(r => r.Id == realUserId);
        }
        protected override async Task<RepositoryResultSet<Guid, User>?> DoFetch(DataGridRequest<Guid, User> request, CancellationToken token)
        {
            var impersonated = await DoLoadImpersonation(token);
            var fetch = await base.DoFetch(request, token);
            if (fetch == null)
                return null;
            fetch.Entities = fetch.Entities.Where(e => e.Id != impersonated).ToArray();
            return fetch;

        }
        protected async Task<Guid?> DoLoadImpersonation(CancellationToken token)
        {
            var impersonated = await ContextFactory.GetImpersonatedUser();
            if (impersonated != null)
                ImpersonatedUser = await Facade.GetByID(impersonated.Value, token: token);
            return impersonated;
        }
        protected async Task DoImpersonate(Guid id, CancellationToken token)
        {
            await ContextFactory.SetImpersonatedUser(id);
            NavMan.NavigateTo("/resume/profile");
            NavMan.Refresh(true);
        }
        protected async Task DoEndImpersonation(CancellationToken token)
        {
            await ContextFactory.SetImpersonatedUser(null);
            ImpersonatedUser = null;
            NavMan.Refresh(true);
        }
    }
    public class RecruitersViewModel : ManageEntityViewModel<Guid, User, IUserBusinessFacade>
    {
        protected NavigationManager NavMan { get; }
        public ReactiveCommand<Guid, Unit> RemoveRecruiter { get; }
        public RecruitersViewModel(NavigationManager navMan, IUserBusinessFacade facade, ILogger<ManageEntityViewModel<Guid, User, IUserBusinessFacade>> logger) : base(facade, logger)
        {
            RemoveRecruiter = ReactiveCommand.CreateFromTask<Guid>(DoRemoveRecruiter);
            NavMan = navMan;
        }
        protected override async Task<Expression<Func<User, bool>>?> GetBaseFilterCondition()
        {
            var userId = await Facade.GetCurrentUserId(fetchTrueUserId: true);
            if (userId == null)
                throw new InvalidDataException();
            return q => q.Recruits.Any(q => q.Id == userId);
        }
        protected async Task DoRemoveRecruiter(Guid id, CancellationToken token)
        {
            try
            {
                var userId = await Facade.GetCurrentUserId(fetchTrueUserId: true);
                if (userId == null)
                    throw new InvalidDataException();
                await Facade.RemoveRecruiter(userId.Value, id, token: token);
                NavMan.Refresh(true);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                await Alert.Handle(ex.Message).GetAwaiter();
            }
        }
    }
    public class RecruitsViewModel : ManageEntityViewModel<Guid, User, IUserBusinessFacade>
    {
        protected NavigationManager NavMan { get; }
        public RecruitsViewModel(NavigationManager navMan, IUserBusinessFacade facade, ILogger<ManageEntityViewModel<Guid, User, IUserBusinessFacade>> logger) : base(facade, logger)
        {
            RemoveRecruit = ReactiveCommand.CreateFromTask<Guid>(DoRemoveRecruit);
            NavMan = navMan;
        }

        public ReactiveCommand<Guid, Unit> RemoveRecruit { get; }
        protected async Task DoRemoveRecruit(Guid id, CancellationToken token)
        {
            try
            {
                var userId = await Facade.GetCurrentUserId(fetchTrueUserId: true, token: token);
                if (userId == null)
                    throw new InvalidDataException();
                await Facade.RemoveRecruiter(id, userId.Value, token: token);
                NavMan.Refresh(true);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                await Alert.Handle(ex.Message).GetAwaiter();
            }
        }
        protected override async Task<Expression<Func<User, bool>>?> GetBaseFilterCondition()
        {
            var userId = await Facade.GetCurrentUserId(fetchTrueUserId: true);
            if (userId == null)
                throw new InvalidOperationException();
            return q => q.Recruiters.Any(a => a.Id == userId);
        }
    }
    public class AcceptRecruiterViewModel : ReactiveObject
    {
        public Interaction<string, bool> Alert { get; } = new Interaction<string, bool>();
        private string? recruiterId;
        public string? RecruiterId
        {
            get => recruiterId;
            set => this.RaiseAndSetIfChanged(ref recruiterId, value);
        }
        private User? recruiter;
        public User? Recruiter
        {
            get => recruiter;
            set
            {
                this.RaiseAndSetIfChanged(ref recruiter, value);
                this.RaisePropertyChanged(nameof(CanAddRecruiter));
            }
        }
        public ReactiveCommand<Unit, Unit> FetchRecruiter { get; }
        public ReactiveCommand<Unit, Unit> AddRecruiter { get; }
        public bool CanAddRecruiter
        {
            get => Recruiter != null;
        }
        protected IUserBusinessFacade Facade { get; }
        protected ILogger Logger { get; }
        protected NavigationManager NavMan { get; }
        public AcceptRecruiterViewModel(NavigationManager navMan, IUserBusinessFacade facade, ILogger<AcceptRecruiterViewModel> logger)
        {
            Facade = facade;
            Logger = logger;
            FetchRecruiter = ReactiveCommand.CreateFromTask(DoFetchRecruiter);
            AddRecruiter = ReactiveCommand.CreateFromTask(DoAddRecruiter);
            NavMan = navMan;
        }
        protected async Task DoFetchRecruiter(CancellationToken token)
        {
            try
            {
                if (Guid.TryParse(RecruiterId?.Trim(), out var id))
                {
                    Recruiter = await Facade.GetByID(id, properites: q => q.Include(e => e.Roles), token: token);
                    if (!(Recruiter?.Roles.Any(e => e.Name == "Recruiter") ?? false))
                        Recruiter = null;
                }
                else
                    Recruiter = null;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                await Alert.Handle(ex.Message).GetAwaiter();
            }
        }
        protected async Task DoAddRecruiter(CancellationToken token)
        {
            try
            {
                if (CanAddRecruiter)
                {
                    var targetUserId = await Facade.GetCurrentUserId(fetchTrueUserId: true, token: token);
                    if (targetUserId == null)
                        throw new InvalidOperationException();
                    await Facade.AddRecruiter(targetUserId.Value, Recruiter!.Id, token: token);
                }
                NavMan.NavigateTo("/recruiters");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                await Alert.Handle(ex.Message).GetAwaiter();
            }
        }
    }
}
