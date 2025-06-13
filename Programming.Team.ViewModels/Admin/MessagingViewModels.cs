using Microsoft.Extensions.Logging;
using Programming.Team.Business.Core;
using Programming.Team.Core;
using Programming.Team.Messaging.Core;
using Programming.Team.Templating.Core;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Programming.Team.ViewModels.Admin
{
    public class MessagingViewModel : ReactiveObject
    {
        public Interaction<string, bool> Alert { get; } = new Interaction<string, bool>();
        public SelectUsersViewModel SelectUsersVM { get; }
        protected IUserBusinessFacade UserFacade { get; }
        protected IEmailMessaging EmailMessaging { get; }
        protected IDocumentTemplator DocumentTemplator { get; }
        public ReactiveCommand<Unit, Unit> Load { get; }
        public ReactiveCommand<Unit, Unit> TestTemplate { get; }
        public ReactiveCommand<Unit, Unit> Send { get; }
        protected ILogger Logger { get; }
        public MessagingViewModel(IUserBusinessFacade userFacade, IEmailMessaging emailMessaging, 
            IDocumentTemplator documentTemplator, SelectUsersViewModel selectUsersViewModel, ILogger<MessagingViewModel> logger)
        {
            UserFacade = userFacade;
            EmailMessaging = emailMessaging;
            DocumentTemplator = documentTemplator;
            SelectUsersVM = selectUsersViewModel;
            Logger = logger;
            Load = ReactiveCommand.CreateFromTask(DoLoad);
            TestTemplate = ReactiveCommand.CreateFromTask(DoTestTemplate);
            Send = ReactiveCommand.CreateFromTask(DoSend);
        }
        private bool isReadyToSend = false;
        public bool IsReadyToSend
        {
            get => isReadyToSend;
            set
            {
                this.RaiseAndSetIfChanged(ref isReadyToSend, value);
            }
        }
        protected Task DoLoad(CancellationToken token)
        {
            return Task.CompletedTask;
        }
        protected async Task DoTestTemplate(CancellationToken token)
        {
            try
            {
                IsReadyToSend = false;
                if (!string.IsNullOrWhiteSpace(MessageTemplate) || !string.IsNullOrWhiteSpace(SubjectTemplate))
                {
                    await Alert.Handle("Message and Subject templates cannot be empty.");
                    var userVm = SelectUsersVM.Selected.FirstOrDefault();
                    User? testUser = null;
                    if (userVm != null)
                        testUser = await userVm.Populate();
                    if(testUser == null)
                    {
                        var users = await UserFacade.Get(page: new Pager() { Page = 1, Size = 1 },
                            orderBy: e => e.OrderBy(x => x.Id), token: token);
                        testUser = users.Entities.FirstOrDefault();
                    }
                    if(testUser == null)
                    {
                        await Alert.Handle("No users selected and no test user found. Please select a user or ensure at least one user exists.");
                        return;
                    }
                    ExampleMessage = await DocumentTemplator.ApplyTemplate(MessageTemplate!, testUser, token: token);
                    ExampleSubject = await DocumentTemplator.ApplyTemplate(SubjectTemplate!, testUser, token: token);
                    IsReadyToSend = true;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error testing template");
                await Alert.Handle($"Error testing template: {ex.Message}");
            }
        }
        protected async Task DoSend(CancellationToken token)
        {
            try
            {
                if (IsReadyToSend)
                {
                    Progress<string> progressable = new Progress<string>(str =>
                    {
                        Progress = str;
                    });
                    await EmailMessaging.SendEmails(MessageTemplate!, SubjectTemplate!, 
                        progressable, IsHtml ? MessageType.Html : MessageType.PlainText, SelectUsersVM.Selected.Select(u => u.Id).ToArray(), token);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error sending messages");
                Progress = null;
                await Alert.Handle($"Error sending messages: {ex.Message}");
            }
            finally
            {
                Progress = null;
            }
        }
        private string? progress;
        public string? Progress
        {
            get => progress;
            set
            {
                this.RaiseAndSetIfChanged(ref progress, value);
                this.RaisePropertyChanged(nameof(IsOverlayOpen));
            }
        }
        public bool IsOverlayOpen
        {
            get => Progress != null;
            set { }
        }
        private string? messageTemplate;
        public string? MessageTemplate
        {
            get => messageTemplate;
            set
            {
                IsReadyToSend = false;
                this.RaiseAndSetIfChanged(ref messageTemplate, value);
            }
        }
        private string? exampleMessage;
        public string? ExampleMessage
        {
            get => exampleMessage;
            set => this.RaiseAndSetIfChanged(ref exampleMessage, value);
        }
        private string? subjectTemplate;
        public string? SubjectTemplate
        {
            get => subjectTemplate;
            set
            {
                IsReadyToSend = false;
                this.RaiseAndSetIfChanged(ref subjectTemplate, value);
            }
        }
        private string? exampleSubject;
        public string? ExampleSubject
        {
            get => exampleSubject;
            set => this.RaiseAndSetIfChanged(ref exampleSubject, value);
        }
        private bool isHtml = false;
        public bool IsHtml
        {
            get => isHtml;
            set
            {
                IsReadyToSend = false;
                this.RaiseAndSetIfChanged(ref isHtml, value);
            }
        }
    }
}
