﻿using Microsoft.Extensions.Logging;
using Programming.Team.Business.Core;
using Programming.Team.Core;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Programming.Team.ViewModels.Resume
{
    public class UserProfileLoaderViewModel : ReactiveObject
    {
        public Interaction<string, bool> Alert { get; } = new Interaction<string, bool>();
        public ReactiveCommand<Unit, Unit> Load { get; }
        protected IUserBusinessFacade Facade { get; }
        protected ILogger Logger { get; }
        private UserProfileViewModel? viewModel;
        public UserProfileViewModel? ViewModel
        {
            get => viewModel;
            set => this.RaiseAndSetIfChanged(ref viewModel, value);
        }
        public UserProfileLoaderViewModel(IUserBusinessFacade facade, ILogger<UserProfileLoaderViewModel> logger) 
        { 
            Facade = facade;
            Logger = logger;
            Load = ReactiveCommand.CreateFromTask(DoLoad);
        }
        protected virtual async Task DoLoad(CancellationToken token)
        {
            try
            {
                var userId = await Facade.GetCurrentUserId(token: token);
                if (userId != null)
                {
                    var user = await Facade.GetByID(userId.Value, token: token);
                    if (user != null)
                    {
                        ViewModel = new UserProfileViewModel(Logger, Facade, user);
                        await ViewModel.Load.Execute().GetAwaiter();
                    }
                }
                else
                    ViewModel = null;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                await Alert.Handle(ex.Message).GetAwaiter();
            }
        }
    }
    public class UserProfileViewModel : EntityViewModel<Guid, User>, IUser
    {
        public UserProfileViewModel(ILogger logger, IBusinessRepositoryFacade<User, Guid> facade, Guid id) : base(logger, facade, id)
        {
        }

        public UserProfileViewModel(ILogger logger, IBusinessRepositoryFacade<User, Guid> facade, User entity) : base(logger, facade, entity)
        {
        }

        private string objectId = string.Empty;
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


        protected override Task<User> Populate()
        {
            return Task.FromResult(new User()
            {
                Id = Id,
                ObjectId = ObjectId,
                FirstName = FirstName,
                LastName = LastName,
                EmailAddress = EmailAddress,
                GitHubUrl = GitHubUrl,
                LinkedInUrl = LinkedInUrl,
                PortfolioUrl = PortfolioUrl,
                Bio = Bio,
                Title = Title,
                PhoneNumber = PhoneNumber,
                City = City,
                State = State,
                Country = Country
            });
        }

        protected override Task Read(User entity)
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
            return Task.CompletedTask;
        }
    }
}