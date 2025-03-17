using DynamicData;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage.Json;
using Microsoft.Extensions.Logging;
using Programming.Team.Business.Core;
using Programming.Team.Core;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Programming.Team.ViewModels.Resume
{
    public class ResumeBuilderViewModel : ReactiveObject
    {
        public ResumeConfigurationViewModel Configuration { get; }
        public Interaction<string, bool> Alert { get; } = new Interaction<string, bool>();
        protected ILogger Logger { get; }
        protected IResumeBuilder Builder { get; }
        public ReactiveCommand<Unit, Unit> Build { get; }
        public ReactiveCommand<Unit, Unit> Load { get; }
        protected IBusinessRepositoryFacade<DocumentTemplate, Guid> DocumentTemplateFacade { get; }
        protected IUserBusinessFacade UserFacade { get; }
        public ObservableCollection<DocumentTemplate> DocumentTemplates { get; } = new ObservableCollection<DocumentTemplate>();
        protected NavigationManager NavMan { get; }
        public ResumeBuilderViewModel(NavigationManager navMan, ResumeConfigurationViewModel config, IUserBusinessFacade userFacade, IBusinessRepositoryFacade<DocumentTemplate, Guid> documentTemplateFacade, ILogger<ResumeBuilderViewModel> logger, IResumeBuilder builder)
        {
            Configuration = config;
            Logger = logger;
            UserFacade = userFacade;
            DocumentTemplateFacade = documentTemplateFacade;
            Builder = builder;
            Build = ReactiveCommand.CreateFromTask(DoBuild);
            Load = ReactiveCommand.CreateFromTask(DoLoad);
            NavMan = navMan;
        }
        private DocumentTemplate? selectedTemplate;
        public DocumentTemplate? SelectedTemplate
        {
            get => selectedTemplate;
            set => this.RaiseAndSetIfChanged(ref selectedTemplate, value);
        }
        private string postingText = string.Empty;
        public string PostingText
        {
            get => postingText;
            set => this.RaiseAndSetIfChanged(ref postingText, value);
        }
        private string name = string.Empty;
        public string Name
        {
            get => name;
            set => this.RaiseAndSetIfChanged(ref name, value);
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
        protected async Task DoBuild(CancellationToken token)
        {
            try
            {
                var userId = await DocumentTemplateFacade.GetCurrentUserId();
                if (userId == null)
                    return;
                Progress<string> progressable = new Progress<string>(str =>
                {
                    Progress = str;
                });
                var resume = await Builder.BuildResume(userId.Value, progressable, token);
                var posting = await Builder.BuildPosting(userId.Value, SelectedTemplate!.Id, Name, PostingText, resume, progressable, Configuration.GetConfiguration(), token: token);
                Progress = null;
                await Alert.Handle("Resume Built").GetAwaiter();
                NavMan.NavigateTo($"/resume/postings/{posting.Id}");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                await Alert.Handle(ex.Message).GetAwaiter();
            }
        }
        protected async Task DoLoad(CancellationToken token)
        {
            try
            {
                var userId = await UserFacade.GetCurrentUserId(fetchTrueUserId: true, token: token);
                var user = await UserFacade.GetByID(userId.Value, token: token);

                DocumentTemplates.Clear();
                var dts = await DocumentTemplateFacade.Get(orderBy: o => o.OrderBy(e => e.Name), token: token);
                DocumentTemplates.AddRange(dts.Entities);
                SelectedTemplate = DocumentTemplates.First();
                await Configuration.Load(user?.DefaultResumeConfiguration);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                await Alert.Handle(ex.Message).GetAwaiter();
            }
        }
    }
}
