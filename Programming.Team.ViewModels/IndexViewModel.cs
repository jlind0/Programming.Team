using DynamicData;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Programming.Team.AI.Core;
using Programming.Team.Business.Core;
using Programming.Team.Core;
using Programming.Team.PurchaseManager.Core;
using Programming.Team.ViewModels.Purchase;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Programming.Team.ViewModels
{
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
        public ObservableCollection<FAQ> FAQs { get; } = new ObservableCollection<FAQ>();
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
                    //faq.Question = string.Join(' ', (await NLP.IdentifyParagraphs(faq.Answer)).Select(x => $"<p>{x}</p>"));
                    FAQs.Add(faq);
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
