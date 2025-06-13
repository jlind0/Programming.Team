using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;
using Programming.Team.Business.Core;
using Programming.Team.Core;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Programming.Team.ViewModels.Admin
{
    public class ManageRolesViewModel : ManageEntityViewModelWithAdd<Guid, Role, AddRoleViewModel>
    {
        public ManageRolesViewModel(IBusinessRepositoryFacade<Role, Guid> facade, AddRoleViewModel addVM, ILogger<ManageEntityViewModel<Guid, Role, IBusinessRepositoryFacade<Role, Guid>>> logger) : base(facade, addVM, logger)
        {
        }
    }
    public class RoleLoaderViewModel : EntityLoaderViewModel<Guid, Role, RoleViewModel, IRoleBusinessFacade>
    {
        protected SelectUsersViewModel SelectedUsersVM { get; }
        public RoleLoaderViewModel(SelectUsersViewModel selectUsersVM, IRoleBusinessFacade facade, ILogger<EntityLoaderViewModel<Guid, Role, RoleViewModel, IRoleBusinessFacade>> logger) : base(facade, logger)
        {
            SelectedUsersVM = selectUsersVM;
        }

        protected override RoleViewModel Construct(Role entity)
        {
            return new RoleViewModel(SelectedUsersVM, Logger, Facade, entity);
        }
    }
    public class RoleViewModel : EntityViewModel<Guid, Role, IRoleBusinessFacade>, IRole
    {
        public SelectUsersViewModel SelectedUsers { get; }
        private string name = null!;

        public RoleViewModel(SelectUsersViewModel selectUsersViewModel, ILogger logger, IRoleBusinessFacade facade, Guid id) : base(logger, facade, id)
        {
            SelectedUsers = selectUsersViewModel;
        }
        public RoleViewModel(SelectUsersViewModel selectUsers, ILogger logger, IRoleBusinessFacade facade, Role entity)
            :base(logger, facade, entity) 
        {
            SelectedUsers = selectUsers;
        }
        public string Name 
        {
            get => name;
            set => this.RaiseAndSetIfChanged(ref name, value);
        }

        internal override Task<Role> Populate()
        {
            var role = new Role()
            {
                Name = Name,
                Id = Id
            };
            return Task.FromResult(role);
        }
        protected override async Task<Role> DoUpdate(CancellationToken token)
        {
            await Facade.SetSelectedUsersToRole(Id, SelectedUsers.Selected.Select(s => s.Id).ToArray(), token: token);
            var role = await base.DoUpdate(token);
            return role;
        }
        internal override async Task Read(Role entity)
        {
            try
            {
                Name = entity.Name;
                Id = entity.Id;
                await SelectedUsers.SetSelected(await Facade.GetUserIds(Id));
            }
            catch(Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                await Alert.Handle(ex.Message).GetAwaiter();
            }
        }
    }
    public class AddRoleViewModel : AddEntityViewModel<Guid, Role>, IRole
    {
        private string name = null!;
        public AddRoleViewModel(IBusinessRepositoryFacade<Role, Guid> facade, ILogger<AddEntityViewModel<Guid, Role, IBusinessRepositoryFacade<Role, Guid>>> logger) : base(facade, logger)
        {
        }
        [Required]
        public string Name 
        {
            get => name;
            set => this.RaiseAndSetIfChanged(ref name, value);
        }

        protected override Task Clear()
        {
            Name = "";
            return Task.CompletedTask;
        }

        protected override Task<Role> ConstructEntity()
        {
            return Task.FromResult(new Role() { Name = Name });
        }
        public override void SetText(string text)
        {
            Name = text;
        }
    }
}
