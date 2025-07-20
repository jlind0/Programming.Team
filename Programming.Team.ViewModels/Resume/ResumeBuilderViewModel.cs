using DynamicData;
using DynamicData.Binding;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage.Json;
using Microsoft.Extensions.Logging;
using Programming.Team.AI.Core;
using Programming.Team.Business.Core;
using Programming.Team.Core;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
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
        public ReactiveCommand<Unit, Unit> ExtractTitle { get; }
        protected IBusinessRepositoryFacade<DocumentTemplate, Guid> DocumentTemplateFacade { get; }
        protected IUserBusinessFacade UserFacade { get; }
        protected NavigationManager NavMan { get; }
        protected IChatService ResumeEnricher { get; }
        public ResumeBuilderViewModel(IChatService resumeEnricher, NavigationManager navMan, ResumeConfigurationViewModel config, IUserBusinessFacade userFacade, IBusinessRepositoryFacade<DocumentTemplate, Guid> documentTemplateFacade, ILogger<ResumeBuilderViewModel> logger, IResumeBuilder builder)
        {
            Configuration = config;
            Logger = logger;
            UserFacade = userFacade;
            DocumentTemplateFacade = documentTemplateFacade;
            Builder = builder;
            Build = ReactiveCommand.CreateFromTask(DoBuild);
            Load = ReactiveCommand.CreateFromTask(DoLoad);
            NavMan = navMan;
            ExtractTitle = ReactiveCommand.CreateFromTask(DoExtractTitle);
            ResumeEnricher = resumeEnricher;
        }
        private bool isProcessing = false;
        public bool IsProcessing
        {
            get => isProcessing;
            set => this.RaiseAndSetIfChanged(ref isProcessing, value);
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
                if (Configuration.SelectedTemplate == null)
                    return;
                Progress<string> progressable = new Progress<string>(str =>
                {
                    Progress = str;
                });
                var resume = await Builder.BuildResume(userId.Value, progressable, token);
                var posting = await Builder.BuildPosting(userId.Value, Configuration.SelectedTemplate.Id, Name, PostingText, resume, progressable, Configuration.GetConfiguration(), token: token);
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
                var dts = await DocumentTemplateFacade.Get(orderBy: o => o.OrderBy(e => e.Name), token: token);
                await Configuration.Load(user?.DefaultResumeConfiguration);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                await Alert.Handle(ex.Message).GetAwaiter();
            }
        }
        protected async Task DoExtractTitle(CancellationToken token)
        {
            try
            {
                IsProcessing = true;
                if (string.IsNullOrWhiteSpace(PostingText))
                    throw new InvalidOperationException("Posting text cannot be empty");
                Name = await ResumeEnricher.ExtractPostingTitle(PostingText ?? throw new InvalidDataException("Posting text cannot be empty"), token: token) ?? string.Empty;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                await Alert.Handle(ex.Message).GetAwaiter();
            }
            finally
            {
                IsProcessing = false;
            }
        }
    }
}
