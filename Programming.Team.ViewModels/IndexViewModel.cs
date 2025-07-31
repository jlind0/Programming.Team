using DynamicData;
using DynamicData.Binding;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Programming.Team.AI.Core;
using Programming.Team.Business.Core;
using Programming.Team.Core;
using Programming.Team.PurchaseManager.Core;
using Programming.Team.ViewModels.Admin;
using Programming.Team.ViewModels.Purchase;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Programming.Team.ViewModels
{
    public class LayoutViewModel : ReactiveObject
    {
        protected ILogger Logger { get; }
        protected IPageBusinessFacade Facade { get; }
        public ReactiveCommand<Unit, Unit> Load { get; }
        public Interaction<string, bool> Alert { get; } = new Interaction<string, bool>();
        public PageViewModel Page { get; }
        private readonly CompositeDisposable disposables = new CompositeDisposable();
        public LayoutViewModel(ILogger<LayoutViewModel> logger, IPageBusinessFacade facade)
        {
            Logger = logger;
            Facade = facade;
            Load = ReactiveCommand.CreateFromTask(DoLoad);
            Page = new PageViewModel(logger, facade, Guid.Empty);
            this.WhenPropertyChanged(p => p.RouteTemplate).Subscribe(async _ =>
            {
                await Load.Execute().GetAwaiter();
            }).DisposeWith(disposables);
            
        }
        ~LayoutViewModel()
        {
            disposables.Dispose();
        }
        private string? routeTemplate;
        public string? RouteTemplate
        {
            get => routeTemplate;
            set => this.RaiseAndSetIfChanged(ref routeTemplate, value);
        }
        protected virtual async Task DoLoad(CancellationToken token)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(RouteTemplate))
                    return;
                var page = await Facade.GetByRoute(RouteTemplate, token: token);
                await Page.LoadPage.Execute(page).GetAwaiter();
            }
            catch(Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                await Alert.Handle(ex.Message).GetAwaiter();
            }
        }
    }
    public class AddPageViewModel : AddEntityViewModel<Guid, Page, IPageBusinessFacade>, IPage
    {
        private string route = string.Empty;
        public string Route
        {
            get => route;
            set => this.RaiseAndSetIfChanged(ref route, value);
        }
        private string? markdown;
        public string? Markdown
        {
            get => this.markdown;
            set => this.RaiseAndSetIfChanged(ref this.markdown, value);
        }

        private string? videoEmbed;
        public string? VideoEmbed
        {
            get => this.videoEmbed;
            set => this.RaiseAndSetIfChanged(ref this.videoEmbed, value);
        }

        private string name = string.Empty;

        public AddPageViewModel(IPageBusinessFacade facade, ILogger<AddEntityViewModel<Guid, Page, IPageBusinessFacade>> logger) : base(facade, logger)
        {
        }

        public string Name
        {
            get => this.name;
            set => this.RaiseAndSetIfChanged(ref this.name, value);
        }

        protected override Task<Page> ConstructEntity()
        {
            return Task.FromResult(new Page()
            {
                Id = Guid.NewGuid(),
                Route = Route,
                Markdown = Markdown,
                VideoEmbed = VideoEmbed,
                Name = Name
            });
        }

        protected override Task Clear()
        {
            Route = string.Empty;
            Markdown = null;
            VideoEmbed = null;
            Name = string.Empty;
            return Task.CompletedTask;
        }
    }
    public class PagesViewModel : EntitiesViewModel<Guid, Page, PageViewModel, IPageBusinessFacade, AddPageViewModel>
    {
        public PagesViewModel(AddPageViewModel addViewModel, IPageBusinessFacade facade, ILogger<EntitiesViewModel<Guid, Page, PageViewModel, IPageBusinessFacade>> logger) : base(addViewModel, facade, logger)
        {
        }

        protected override Task<PageViewModel> Construct(Page entity, CancellationToken token)
        {
            return Task.FromResult(new PageViewModel(Logger, Facade, entity));
        }
    }
    public class PageViewModel : EntityViewModel<Guid, Page, IPageBusinessFacade>, IPage
    {
        private string route = string.Empty;
        public string Route
        {
            get => route;
            set => this.RaiseAndSetIfChanged(ref route, value);
        }
        private string? markdown;
        public string? Markdown
        {
            get => this.markdown;
            set => this.RaiseAndSetIfChanged(ref this.markdown, value);
        }

        private string? videoEmbed;
        public string? VideoEmbed
        {
            get => this.videoEmbed;
            set => this.RaiseAndSetIfChanged(ref this.videoEmbed, value);
        }

        private string name = string.Empty;
        public string Name
        {
            get => this.name;
            set => this.RaiseAndSetIfChanged(ref this.name, value);
        }
        public ReactiveCommand<Unit, Unit> ToggleOpen { get; }
        public ReactiveCommand<Page?, Unit> LoadPage { get; }
        public PageViewModel(ILogger logger, IPageBusinessFacade facade, Guid id) : base(logger, facade, id)
        {
            ToggleOpen = ReactiveCommand.Create<Unit>(u => { IsOpen = !IsOpen; });
            LoadPage = ReactiveCommand.CreateFromTask<Page?>(DoReadPage);
        }
        protected virtual async Task DoReadPage(Page? page, CancellationToken token)
        {
            if (page == null)
            {
                Id = Guid.Empty;
                Route = string.Empty;
                Markdown = null;
                VideoEmbed = null;
                Name = string.Empty;
            }
            else
            {
                await Read(page);
            }
        }
        public PageViewModel(ILogger logger, IPageBusinessFacade facade, Page entity) : base(logger, facade, entity)
        {
            ToggleOpen = ReactiveCommand.Create<Unit>(u => { IsOpen = !IsOpen; });
            LoadPage = ReactiveCommand.CreateFromTask<Page?>(DoReadPage);
        }

        private bool isOpen;
        public bool IsOpen
        {
            get => isOpen;
            set => this.RaiseAndSetIfChanged(ref isOpen, value);
        }

        internal override Task<Page> Populate()
        {
            return Task.FromResult(new Page()
            {
                Id = Id,
                Route = Route,
                Markdown = Markdown,
                VideoEmbed = VideoEmbed,
                Name = Name
            });
        }

        internal override Task Read(Page entity)
        {
            Id = entity.Id;
            Route = entity.Route;
            Markdown = entity.Markdown;
            VideoEmbed = entity.VideoEmbed;
            Name = entity.Name;
            return Task.CompletedTask;
        }
    }
    public class IndexViewModel : ReactiveObject
    {
        public Interaction<string, bool> Alert { get; } = new Interaction<string, bool>();
        protected ILogger Logger { get; }
        protected IBusinessRepositoryFacade<Posting, Guid> PostingFacade { get; }
        protected IBusinessRepositoryFacade<Package, Guid> PackageFacade { get; }
        protected IBusinessRepositoryFacade<FAQ, Guid> FAQFacade { get; }
        public ReactiveCommand<Unit, Unit> Load { get; }
        public ObservableCollection<Posting> Postings { get; } = new ObservableCollection<Posting>();
        public ObservableCollection<PackageViewModel> Packages { get; } = new ObservableCollection<PackageViewModel>();
        public ObservableCollection<FAQViewModel> FAQs { get; } = new ObservableCollection<FAQViewModel>();
        protected INLP NLP { get; }
        protected IMemoryCache Cache { get; }
        protected NavigationManager NavMan { get; }
        protected IPurchaseManager<Package, Core.Purchase> PurchaseManager { get; }
        private int postingCount = 0;
        public int PostingCount
        {
            get => postingCount;
            set => this.RaiseAndSetIfChanged(ref postingCount, value);
        }
        public IndexViewModel(ILogger<IndexViewModel> logger, IBusinessRepositoryFacade<Package, Guid> packageFacade, IBusinessRepositoryFacade<FAQ, Guid> faqFacade,
            IBusinessRepositoryFacade<Posting, Guid> postingFacade, INLP nlp, IMemoryCache cache, NavigationManager navMan, IPurchaseManager<Package, Core.Purchase> purchaseManager)
        {
            Logger = logger;
            PackageFacade = packageFacade;
            PostingFacade = postingFacade;
            Load = ReactiveCommand.CreateFromTask(DoLoad);
            NLP = nlp;
            Cache = cache;
            NavMan = navMan;
            PurchaseManager = purchaseManager;
            FAQFacade = faqFacade;
        }
        protected async Task DoLoad(CancellationToken token)
        {
            try
            {
                PostingCount = await PostingFacade.Count(filter: f => !string.IsNullOrWhiteSpace(f.RenderedLaTex), token: token);
                Postings.Clear();
                Packages.Clear();
                FAQs.Clear();
                var packages = await PackageFacade.Get(orderBy: e => e.OrderByDescending(p => p.Price));
                foreach(var p in packages.Entities)
                {
                    var pVM = new PackageViewModel(NavMan, PurchaseManager, Logger, PackageFacade, p);
                    await pVM.Load.Execute().GetAwaiter();
                    Packages.Add(pVM);
                }
                var results = await PostingFacade.Get(page: new Pager() { Page = 1, Size = 5}, 
                    orderBy: q => q.OrderByDescending(q => q.UpdateDate), 
                    filter: f => !string.IsNullOrWhiteSpace(f.RenderedLaTex),
                    token: token);
                foreach(var p in results.Entities)
                {
                    var key = $"postDetailsCache-{p.Id}";
                    if (!Cache.TryGetValue<string>(key, out var details))
                    {
                        p.Details = string.Join(' ', (await NLP.IdentifyParagraphs(p.Details)).Select(x => $"<p>{x}</p>"));
                        Cache.Set(key, p.Details);
                    }
                    else
                        p.Details = details ?? throw new InvalidDataException();
                    Postings.Add(p);
                }
                var faqs = await FAQFacade.Get(orderBy: e => e.OrderBy(x => x.SortOrder), token: token);
                foreach (var faq in faqs.Entities)
                {
                    //faq.Answer = string.Join(' ', (await NLP.IdentifyParagraphs(faq.Answer)).Select(x => $"<p>{x}</p>"));
                    //faq.Question = string.Join(' ', (await NLP.IdentifyParagraphs(faq.Question)).Select(x => $"<p>{x}</p>"));
                    var vm = new FAQViewModel(Logger, FAQFacade, faq);
                    
                    FAQs.Add(vm);
                    await vm.Load.GetAwaiter();
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                await Alert.Handle(ex.Message).GetAwaiter();
            }
        }
    }
}
