using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Programming.Team.Business.Core;
using Programming.Team.Core;
using Programming.Team.Data.Core;
using Programming.Team.PurchaseManager.Core;
using Programming.Team.ViewModels.Resume;
using ReactiveUI;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Reactive;
using System.Reactive.Linq;

namespace Programming.Team.ViewModels.Admin
{
    public class UsersViewModel : ManageEntityViewModel<Guid, User, IUserBusinessFacade>
    {
        public UsersViewModel(IUserBusinessFacade facade, ILogger<ManageEntityViewModel<Guid, User, IUserBusinessFacade>> logger) : base(facade, logger)
        {
        }
    }
    public class SelectUsersViewModel : SelectEntitiesViewModel<Guid, User, UserViewModel, IUserBusinessFacade>
    {
        public SelectUsersViewModel(IUserBusinessFacade facade, ILogger<SelectEntitiesViewModel<Guid, User, UserViewModel, IUserBusinessFacade>> logger) : base(facade, logger)
        {
        }

        protected override Task<UserViewModel> ConstructViewModel(User entity)
        {
            return Task.FromResult(new UserViewModel(Logger, Facade, entity, null, null));
        }
    }
    public class TrueUserLoaderViewModel : EntityLoaderViewModel<Guid, User, UserViewModel, IUserBusinessFacade>
    {
        protected ResumeConfigurationViewModel Config { get; }
        protected IServiceProvider Services { get; }
        public TrueUserLoaderViewModel(IServiceProvider services, IUserBusinessFacade facade, ResumeConfigurationViewModel config, ILogger<EntityLoaderViewModel<Guid, User, UserViewModel, IUserBusinessFacade>> logger) : base(facade, logger)
        {
            Config = config;
            Services = services;
        }
        protected override async Task DoLoad(Guid key, CancellationToken token)
        {
            key = (await Facade.GetCurrentUserId(fetchTrueUserId: true, token: token)) ?? throw new InvalidDataException();
            await base.DoLoad(key, token);
        }
        protected override UserViewModel Construct(User entity)
        {
            return new UserViewModel(Logger, Facade, entity, Config, Services.GetRequiredService<UserStripeAccountViewModel>());
        }
    }
    public class UserStripeAccountViewModel : ReactiveObject, IStripePayable
    {
        public Interaction<string, bool> Alert { get; } = new Interaction<string, bool>();
        protected IAccountManager AccountManager { get; }
        protected IUserBusinessFacade UserFacade { get; }
        protected ILogger Logger { get; }
        public ReactiveCommand<Unit, Unit> CreateAccount { get; }
        protected NavigationManager NavMan { get; }
        public UserStripeAccountViewModel(NavigationManager navMan, IAccountManager accountManager, IUserBusinessFacade facade, ILogger<UserStripeAccountViewModel> logger)
        {
            AccountManager = accountManager;
            UserFacade = facade;
            NavMan = navMan;
            Logger = logger;
            CreateAccount = ReactiveCommand.CreateFromTask(DoCreateAccount);
        }
        protected User? User { get; set; }
        protected async Task DoCreateAccount(CancellationToken token)
        {
            try
            {
                if (User == null || StripeStatus == "approved")
                    return;
                var url = await AccountManager.CreateAccountId(User, StripeAccountId, token);
                if (url == null) return;
                NavMan.NavigateTo(url);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                await Alert.Handle(ex.Message).GetAwaiter();
            }
        }
        public Task Populate(User user, CancellationToken token = default)
        {
            User = user;
            SupplyStripeConnect = SupplyStripeConnect && !string.IsNullOrEmpty(user.StripeAccountId);
            StripeAccountId = user.StripeAccountId;
            StripeStatus = user.StripeStatus;
            StripeUpdateDate = user.StripeUpdateDate;
            return Task.CompletedTask;
        }
        private bool supplyStripeConnect;
        public bool SupplyStripeConnect
        {
            get => supplyStripeConnect;
            set => this.RaiseAndSetIfChanged(ref supplyStripeConnect, value);
        }
        private string? stripeAccountId;
        public string? StripeAccountId
        {
            get => stripeAccountId;
            set => this.RaiseAndSetIfChanged(ref stripeAccountId, value);
        }
        private string? stripeStatus;
        public string? StripeStatus
        {
            get => stripeStatus;
            set => this.RaiseAndSetIfChanged(ref stripeStatus, value);
        }
        private DateTime? stripeUpdateDate;
        public DateTime? StripeUpdateDate
        {
            get => stripeUpdateDate;
            set => this.RaiseAndSetIfChanged(ref stripeUpdateDate, value);
        }
    }
    public class UserViewModel : EntityViewModel<Guid, User, IUserBusinessFacade>, IUser
    {
        public ResumeConfigurationViewModel? Configuration { get; }
        public UserStripeAccountViewModel? StripeVM { get; }
        public UserViewModel(ILogger logger, IUserBusinessFacade facade, Guid id, ResumeConfigurationViewModel? config, UserStripeAccountViewModel? stripeVM) : base(logger, facade, id)
        {
            Configuration = config;
            StripeVM = stripeVM;
        }

        public UserViewModel(ILogger logger, IUserBusinessFacade facade, User entity, ResumeConfigurationViewModel? config, UserStripeAccountViewModel? stripeVM) : base(logger, facade, entity)
        {
            Configuration = config;
            StripeVM = stripeVM;    
        }

        private string objectId = null!;
        public string ObjectId 
        {
            get => objectId;
            set => this.RaiseAndSetIfChanged(ref objectId, value);
        }
        private string? firstName;
        public string? FirstName
        {
            get => firstName;
            set => this.RaiseAndSetIfChanged(ref firstName, value);
        }

        private string? lastName;
        public string? LastName
        {
            get => lastName;
            set => this.RaiseAndSetIfChanged(ref lastName, value);
        }

        private string? emailAddress;
        public string? EmailAddress
        {
            get => emailAddress;
            set => this.RaiseAndSetIfChanged(ref emailAddress, value);
        }

        private string? gitHubUrl;
        public string? GitHubUrl
        {
            get => gitHubUrl;
            set => this.RaiseAndSetIfChanged(ref gitHubUrl, value);
        }

        private string? linkedInUrl;
        public string? LinkedInUrl
        {
            get => linkedInUrl;
            set => this.RaiseAndSetIfChanged(ref linkedInUrl, value);
        }

        private string? portfolioUrl;
        public string? PortfolioUrl
        {
            get => portfolioUrl;
            set => this.RaiseAndSetIfChanged(ref portfolioUrl, value);
        }

        private string? bio;
        public string? Bio
        {
            get => bio;
            set => this.RaiseAndSetIfChanged(ref bio, value);
        }

        private string? title;
        public string? Title
        {
            get => title;
            set => this.RaiseAndSetIfChanged(ref title, value);
        }

        private string? phoneNumber;
        public string? PhoneNumber
        {
            get => phoneNumber;
            set => this.RaiseAndSetIfChanged(ref phoneNumber, value);
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
        private int resumeGenerationsLeft;
        public int ResumeGenerationsLeft
        {
            get => resumeGenerationsLeft;
            set => this.RaiseAndSetIfChanged(ref resumeGenerationsLeft, value);
        }
        private string? defaultResumeConfiguration;
        public string? DefaultResumeConfiguration
        {
            get => defaultResumeConfiguration;
            set => this.RaiseAndSetIfChanged(ref defaultResumeConfiguration, value);
        }
        public string? StripeAccountId
        {
            get => StripeVM?.StripeAccountId;
            set
            {
                if(StripeVM != null)
                    StripeVM.StripeAccountId = value;
            }
        }
        public string? StripeStatus
        {
            get => StripeVM?.StripeStatus;
            set
            {
                if (StripeVM != null)
                    StripeVM.StripeStatus = value;
            }
        }
        public DateTime? StripeUpdateDate
        {
            get => StripeVM?.StripeUpdateDate;
            set
            {
                if (StripeVM != null)
                    StripeVM.StripeUpdateDate = value;
            }
        }

        protected override Task<User> Populate()
        {
            User user = new User();
            user.Id = Id;
            user.ObjectId = ObjectId;
            user.FirstName = FirstName;
            user.LastName = LastName;
            user.EmailAddress = EmailAddress;
            user.GitHubUrl = GitHubUrl;
            user.LinkedInUrl = LinkedInUrl;
            user.PortfolioUrl = PortfolioUrl;
            user.Bio = Bio;
            user.Title = Title;
            user.PhoneNumber = PhoneNumber;
            user.City = City;
            user.State = State;
            user.Country = Country;
            user.ResumeGenerationsLeft = ResumeGenerationsLeft;
            user.DefaultResumeConfiguration = Configuration?.GetSerializedConfiguration() ?? DefaultResumeConfiguration;
            user.StripeAccountId = StripeAccountId;
            user.StripeStatus = StripeStatus;
            user.StripeUpdateDate = StripeUpdateDate;
            return Task.FromResult(user);
        }
        
        protected override async Task Read(User entity)
        {
            Id = entity.Id;
            ObjectId = entity.ObjectId;
            FirstName = entity.FirstName;
            LastName = entity.LastName;
            EmailAddress = entity.EmailAddress;
            GitHubUrl = entity.GitHubUrl;
            LinkedInUrl = entity.LinkedInUrl;
            PortfolioUrl = entity.PortfolioUrl;
            Bio = entity.Bio;
            Title = entity.Title;
            PhoneNumber = entity.PhoneNumber;
            City = entity.City;
            State = entity.State;
            Country = entity.Country;
            ResumeGenerationsLeft = entity.ResumeGenerationsLeft;
            DefaultResumeConfiguration = entity.DefaultResumeConfiguration;
            if(Configuration != null)
                await Configuration.Load(DefaultResumeConfiguration);
            if(StripeVM != null)
                await StripeVM.Populate(entity);
        }
    }
}
