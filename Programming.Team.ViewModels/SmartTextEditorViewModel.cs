using DynamicData.Binding;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Programming.Team.Core;
using Programming.Team.Templating.Core;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Programming.Team.ViewModels
{
    public class SmartTextEditorViewModel<TViewModel> : ReactiveObject
        where TViewModel : ReactiveObject, ITextual
    {
        public Interaction<string, bool> Alert { get; } = new Interaction<string, bool>();
        protected TViewModel ViewModel { get; }
        protected ILogger Logger { get; }
        protected IDocumentTemplator Templator { get; }
        public ReactiveCommand<Unit, Unit> RenderMarkdownToHtmlCommand { get; }
        public ReactiveCommand<Unit, Unit> ToggleMarkdownEditor { get; }
        public ReactiveCommand<Unit, Unit> ToggleHtmlEditor { get; }
        private bool isHtmlEditorOpen;
        public bool IsHtmlEditorOpen
        {
            get => isHtmlEditorOpen;
            set
            {
                this.RaiseAndSetIfChanged(ref isHtmlEditorOpen, value);
            }
        }
        public string ApiKey { get; }
        private readonly CompositeDisposable disposables = new CompositeDisposable();
        private readonly object _lock = new object();
        public SmartTextEditorViewModel(TViewModel viewModel, ILogger logger,
            IDocumentTemplator templator, IConfiguration config)
        {
            ApiKey = config["TinyMCE:APIKey"] ?? throw new ArgumentNullException("TinyMCE:APIKey is not configured.");
            ViewModel = viewModel;
            Logger = logger;
            Templator = templator;
            RenderMarkdownToHtmlCommand = ReactiveCommand.CreateFromTask(DoRenderMarkdownToHtml);
            ToggleMarkdownEditor = ReactiveCommand.Create(() =>
            {
                IsMarkdownEditorOpen = !IsMarkdownEditorOpen;
            });

            ToggleHtmlEditor = ReactiveCommand.Create(() =>
            {
                IsHtmlEditorOpen = !IsHtmlEditorOpen;
            });
            
            ViewModel.WhenPropertyChanged(p => p.Text).Subscribe(p =>
            {
                if(IsHtmlEditorOpen)
                {
                    Html = p.Value;
                }
                else
                {
                    Text = p.Value;
                }
            }).DisposeWith(disposables);
            ViewModel.WhenPropertyChanged(p => p.TextTypeId).DistinctUntilChanged().Subscribe(p =>
            {
                IsHtmlEditorOpen = p.Value == TextType.Html;
            }).DisposeWith(disposables);
            this.WhenPropertyChanged(p => p.Text).Subscribe(p =>
            {
                if (!IsHtmlEditorOpen)
                {
                    ViewModel.Text = p.Value;
                }
            }).DisposeWith(disposables);
            this.WhenPropertyChanged(p => p.Html).Subscribe(p =>
            {
                if (IsHtmlEditorOpen)
                {
                    ViewModel.Text = p.Value;
                }
            }).DisposeWith(disposables);
            
            this.WhenPropertyChanged(p => p.IsHtmlEditorOpen).Subscribe(async p =>
            {
                if (!p.Value)
                {
                    if (!string.IsNullOrWhiteSpace(Html))
                    {
                        Text = await Templator.RenderMarkdownFromHtml(Html);
                    }
                    Html = null; // Clear HTML when closing editor
                    ViewModel.TextTypeId = TextType.Text; // Update ViewModel TextType to Text
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(Text))
                        Html = await Templator.RenderHtmlFromMarkdown(Text);
                    Text = null;
                    ViewModel.TextTypeId = TextType.Html; // Update ViewModel TextType to Html
                }
                
            }).DisposeWith(disposables);
        }
        ~SmartTextEditorViewModel()
        {
            disposables.Dispose();
        }
        private string? markdown;
        public string? Markdown
        {
            get => markdown;
            set
            {
                this.RaiseAndSetIfChanged(ref markdown, value);
                this.RaisePropertyChanged(nameof(CanRendeMarkdownToHtml));
            }
        }
        private bool isMarkdownEditorOpen;
        public bool IsMarkdownEditorOpen
        {
            get => isMarkdownEditorOpen;
            set
            {
                this.RaiseAndSetIfChanged(ref isMarkdownEditorOpen, value);
            }
        }
        public bool CanRendeMarkdownToHtml
        {
            get => !string.IsNullOrWhiteSpace(Markdown);
        }
        private string? html;
        public string? Html
        {
            get => html;
            set
            {
                this.RaiseAndSetIfChanged(ref html, value);
            }
        }
        private string? text;
        public string? Text
        {
            get => text;
            set
            {
                this.RaiseAndSetIfChanged(ref text, value);
            }
        }
        public async Task DoRenderMarkdownToHtml(CancellationToken token = default)
        {
            if (string.IsNullOrWhiteSpace(Markdown))
            {
                await Alert.Handle("Markdown content is empty. Please enter some content.").GetAwaiter();
                return;
            }
            try
            {
                Html = await Templator.RenderHtmlFromMarkdown(Markdown, token);
                this.IsMarkdownEditorOpen = false;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error rendering Markdown to HTML");
                await Alert.Handle($"Error rendering Markdown to HTML: {ex.Message}").GetAwaiter();
            }
        }
    }
}
