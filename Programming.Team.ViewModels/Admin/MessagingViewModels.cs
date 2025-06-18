using Microsoft.Extensions.DependencyInjection;
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
    public class AddEmailMessageTemplateViewModel : AddEntityViewModel<Guid, EmailMessageTemplate>, IEmailMessageTemplate
    {
        public AddEmailMessageTemplateViewModel(IBusinessRepositoryFacade<EmailMessageTemplate, Guid> facade, ILogger<AddEntityViewModel<Guid, EmailMessageTemplate, IBusinessRepositoryFacade<EmailMessageTemplate, Guid>>> logger) : base(facade, logger)
        {
        }
        private string messageTemplate = string.Empty;
        private string subjectTemplate = string.Empty;
        private bool isHtml = true;
        private string name = string.Empty;

        // Properties with Notification
        public string MessageTemplate
        {
            get => messageTemplate;
            set => this.RaiseAndSetIfChanged(ref messageTemplate, value);
        }

        public string SubjectTemplate
        {
            get => subjectTemplate;
            set => this.RaiseAndSetIfChanged(ref subjectTemplate, value);
        }

        public bool IsHtml
        {
            get => isHtml;
            set => this.RaiseAndSetIfChanged(ref isHtml, value);
        }

        public string Name
        {
            get => name;
            set => this.RaiseAndSetIfChanged(ref name, value);
        }

        protected override Task Clear()
        {
            MessageTemplate = "";
            SubjectTemplate = "";
            IsHtml = true;
            Name = "";
            return Task.CompletedTask;
        }

        protected override Task<EmailMessageTemplate> ConstructEntity()
        {
            return Task.FromResult(new EmailMessageTemplate()
            {
                IsHtml = IsHtml,
                Name = Name,
                SubjectTemplate = SubjectTemplate,
                MessageTemplate = messageTemplate
            });
        }
    }
    public class EmailMessageTemplatesViewModel : EntitiesDefaultViewModel<Guid, EmailMessageTemplate, EmailMessageTemplateViewModel, AddEmailMessageTemplateViewModel>
    {
        protected IServiceProvider ServiceProvider { get; }
        public EmailMessageTemplatesViewModel(IServiceProvider serviceProvider, AddEmailMessageTemplateViewModel addViewModel, IBusinessRepositoryFacade<EmailMessageTemplate, Guid> facade, ILogger<EntitiesViewModel<Guid, EmailMessageTemplate, EmailMessageTemplateViewModel, IBusinessRepositoryFacade<EmailMessageTemplate, Guid>>> logger) : base(addViewModel, facade, logger)
        {
            ServiceProvider = serviceProvider;
        }

        protected override Task<EmailMessageTemplateViewModel> Construct(EmailMessageTemplate entity, CancellationToken token)
        {
            return Task.FromResult(new EmailMessageTemplateViewModel(Facade,
                ServiceProvider.GetRequiredService<IUserBusinessFacade>(),
                ServiceProvider.GetRequiredService<IEmailMessaging>(),
                ServiceProvider.GetRequiredService<IDocumentTemplator>(),
                ServiceProvider.GetRequiredService<SelectUsersViewModel>(),
                Logger, entity));
        }
    }
    public class EmailMessageTemplateViewModel : EntityViewModel<Guid, EmailMessageTemplate>, IEmailMessageTemplate
    {
        public SelectUsersViewModel SelectUsersVM { get; }
        protected IUserBusinessFacade UserFacade { get; }
        protected IEmailMessaging EmailMessaging { get; }
        protected IDocumentTemplator DocumentTemplator { get; }
        public ReactiveCommand<Unit, Unit> TestTemplate { get; }
        public ReactiveCommand<Unit, Unit> Send { get; }
        public EmailMessageTemplateViewModel(IBusinessRepositoryFacade<EmailMessageTemplate, Guid> facade, IUserBusinessFacade userFacade, IEmailMessaging emailMessaging, 
            IDocumentTemplator documentTemplator, SelectUsersViewModel selectUsersViewModel, ILogger logger, EmailMessageTemplate entity): 
            base(logger,facade, entity)
        {
            UserFacade = userFacade;
            EmailMessaging = emailMessaging;
            DocumentTemplator = documentTemplator;
            SelectUsersVM = selectUsersViewModel;
            TestTemplate = ReactiveCommand.CreateFromTask(DoTestTemplate);
            Send = ReactiveCommand.CreateFromTask(DoSend);
        }
        public EmailMessageTemplateViewModel(IBusinessRepositoryFacade<EmailMessageTemplate, Guid> facade, IUserBusinessFacade userFacade, IEmailMessaging emailMessaging,
            IDocumentTemplator documentTemplator, SelectUsersViewModel selectUsersViewModel, ILogger logger, Guid id) :
            base(logger, facade, id)
        {
            UserFacade = userFacade;
            EmailMessaging = emailMessaging;
            DocumentTemplator = documentTemplator;
            SelectUsersVM = selectUsersViewModel;
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
        protected override async Task<EmailMessageTemplate?> DoLoad(CancellationToken token)
        {
            var e = await base.DoLoad(token);
            await SelectUsersVM.SetSelected([], token);
            return e;
        }
        protected async Task DoTestTemplate(CancellationToken token)
        {
            try
            {
                IsReadyToSend = false;
                if (!string.IsNullOrWhiteSpace(MessageTemplate) && !string.IsNullOrWhiteSpace(SubjectTemplate))
                {
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

        internal override Task<EmailMessageTemplate> Populate()
        {
            return Task.FromResult(new EmailMessageTemplate()
            {
                Id = Id,
                MessageTemplate = MessageTemplate,
                SubjectTemplate = SubjectTemplate,
                IsHtml = IsHtml,
                Name = Name,
            });
        }

        internal override Task Read(EmailMessageTemplate entity)
        {
            Id = entity.Id;
            Name = entity.Name;
            IsHtml = entity.IsHtml;
            SubjectTemplate = entity.SubjectTemplate;
            MessageTemplate = entity.MessageTemplate;
            return Task.CompletedTask;
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
        private string messageTemplate = "";
        public string MessageTemplate
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
        private string subjectTemplate = "";
        public string SubjectTemplate
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
        private string name = "";
        public string Name 
        {
            get => name;
            set => this.RaiseAndSetIfChanged(ref name, value);
        }
    }
}
