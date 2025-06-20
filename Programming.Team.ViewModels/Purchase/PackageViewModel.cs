using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Programming.Team.Business.Core;
using Programming.Team.Core;
using Programming.Team.PurchaseManager.Core;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive;
using System.Reactive.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using P = Programming.Team.Core.Purchase;

namespace Programming.Team.ViewModels.Purchase
{
    public class PurchaseHistoryViewModel : ManageEntityViewModel<Guid, P>
    {
        public PurchaseHistoryViewModel(IBusinessRepositoryFacade<P, Guid> facade, ILogger<ManageEntityViewModel<Guid, P, IBusinessRepositoryFacade<P, Guid>>> logger) : base(facade, logger)
        {
        }
        protected override async Task<Expression<Func<P, bool>>?> GetBaseFilterCondition()
        {
            var userId = await Facade.GetCurrentUserId(fetchTrueUserId: true);
            return q => q.UserId == userId;
        }
    }
    public class GlobalPurchaseHistoryViewModel : ManageEntityViewModel<Guid, P>
    {
        public GlobalPurchaseHistoryViewModel(IBusinessRepositoryFacade<P, Guid> facade, ILogger<ManageEntityViewModel<Guid, P, IBusinessRepositoryFacade<P, Guid>>> logger) : base(facade, logger)
        {
        }
        protected override Func<IQueryable<P>, IQueryable<P>>? PropertiesToLoad()
        {
            return q => q.Include(p => p.User);
        }
    }
    public class PackagesViewModel : EntitiesDefaultViewModel<Guid, Package, PackageViewModel, AddPackageViewModel>
    {
        public PackagesViewModel(IPurchaseManager<Package, P> purchaseManager, NavigationManager navMan, AddPackageViewModel addViewModel, IBusinessRepositoryFacade<Package, Guid> facade, ILogger<EntitiesViewModel<Guid, Package, PackageViewModel, IBusinessRepositoryFacade<Package, Guid>>> logger) : base(addViewModel, facade, logger)
        {
            PurchaseManager = purchaseManager;
            NavMan = navMan;
        }
        protected override Func<IQueryable<Package>, IOrderedQueryable<Package>>? OrderBy()
        {
            return x => x.OrderByDescending(e => e.Price);
        }
        protected IPurchaseManager<Package, P> PurchaseManager { get; }
        protected NavigationManager NavMan { get; }
        protected override Task<PackageViewModel> Construct(Package entity, CancellationToken token)
        {
            return Task.FromResult(new PackageViewModel(NavMan, PurchaseManager, Logger, Facade, entity));
        }
    }
    public class AddPackageViewModel : AddEntityViewModel<Guid, Package>, IPackage
    {
        private decimal? price;
        private int resumeGenerations;
        private string? stripeProductId;
        private string? stripePriceId;
        private string? stripeUrl;

        public AddPackageViewModel(IBusinessRepositoryFacade<Package, Guid> facade, ILogger<AddEntityViewModel<Guid, Package, IBusinessRepositoryFacade<Package, Guid>>> logger) : base(facade, logger)
        {
        }

        // Price Property with Change Notification
        public decimal? Price
        {
            get => price;
            set => this.RaiseAndSetIfChanged(ref price, value);
        }

        // ResumeGenerations Property with Change Notification
        public int ResumeGenerations
        {
            get => resumeGenerations;
            set => this.RaiseAndSetIfChanged(ref resumeGenerations, value);
        }

        // StripeProductId Property with Change Notification
        public string? StripeProductId
        {
            get => stripeProductId;
            set => this.RaiseAndSetIfChanged(ref stripeProductId, value);
        }

        // StripePriceId Property with Change Notification
        public string? StripePriceId
        {
            get => stripePriceId;
            set => this.RaiseAndSetIfChanged(ref stripePriceId, value);
        }

        // StripeUrl Property with Change Notification
        public string? StripeUrl
        {
            get => stripeUrl;
            set => this.RaiseAndSetIfChanged(ref stripeUrl, value);
        }

        public string StripeName => throw new NotImplementedException();
        private string name = null!;

        public string Name {
            get => name;
            set => this.RaiseAndSetIfChanged(ref name, value);
        }

        protected override Task Clear()
        {
            Price = null;
            Name = string.Empty;
            ResumeGenerations = 0;
            return Task.CompletedTask;
        }

        protected override Task<Package> ConstructEntity()
        {
            return Task.FromResult(new Package()
            {
                Price = price,
                ResumeGenerations = resumeGenerations,
                Name = name,
            });
        }
    }
    public class PackageLoaderViewModel : EntityLoaderViewModel<Guid, Package, PackageViewModel, IBusinessRepositoryFacade<Package, Guid>>
    {
        protected IPurchaseManager<Package, P> PurchaseManager { get; }
        protected NavigationManager NavMan { get; }
        public PackageLoaderViewModel(NavigationManager navMan, IPurchaseManager<Package, P> purchaseManager, IBusinessRepositoryFacade<Package, Guid> facade, ILogger<EntityLoaderViewModel<Guid, Package, PackageViewModel, IBusinessRepositoryFacade<Package, Guid>>> logger) : base(facade, logger)
        {
            PurchaseManager = purchaseManager;
            NavMan = navMan;
        }

        protected override PackageViewModel Construct(Package entity)
        {
            return new PackageViewModel(NavMan, PurchaseManager, Logger, Facade, entity);
        }
    }
    public class PackageViewModel : EntityViewModel<Guid, Package>, IPackage
    {
        public ReactiveCommand<Unit, Unit> Purchase { get; }
        protected IPurchaseManager<Package, P> PurchaseManager { get; }
        protected NavigationManager NavMan { get; } 
        public PackageViewModel(NavigationManager navMan, IPurchaseManager<Package, P> purchaseManager, ILogger logger, IBusinessRepositoryFacade<Package, Guid> facade, Guid id) : base(logger, facade, id)
        {
            PurchaseManager = purchaseManager;
            Purchase = ReactiveCommand.CreateFromTask(DoPurchase);
            NavMan = navMan;
        }

        public PackageViewModel(NavigationManager navMan, IPurchaseManager<Package, P> purchaseManager, ILogger logger, IBusinessRepositoryFacade<Package, Guid> facade, Package entity) : base(logger, facade, entity)
        {
            PurchaseManager = purchaseManager;
            Purchase = ReactiveCommand.CreateFromTask(DoPurchase);
            NavMan = navMan;
        }
        protected async Task DoPurchase(CancellationToken token)
        {
            try
            {
                var package = await Facade.GetByID(Id, token: token);
                if (package == null)
                    throw new InvalidDataException("Package not found.");
                var purchase = await PurchaseManager.StartPurchase(package, token);
                NavMan.NavigateTo(purchase.StripeSessionUrl);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                await Alert.Handle(ex.Message).GetAwaiter();
            }
        }
        private decimal? price;
        private int resumeGenerations;
        private string? stripeProductId;
        private string? stripePriceId;
        private string? stripeUrl;

        // Price Property with Change Notification
        public decimal? Price
        {
            get => price;
            set => this.RaiseAndSetIfChanged(ref price, value);
        }

        // ResumeGenerations Property with Change Notification
        public int ResumeGenerations
        {
            get => resumeGenerations;
            set => this.RaiseAndSetIfChanged(ref resumeGenerations, value);
        }

        // StripeProductId Property with Change Notification
        public string? StripeProductId
        {
            get => stripeProductId;
            set => this.RaiseAndSetIfChanged(ref stripeProductId, value);
        }

        // StripePriceId Property with Change Notification
        public string? StripePriceId
        {
            get => stripePriceId;
            set => this.RaiseAndSetIfChanged(ref stripePriceId, value);
        }

        // StripeUrl Property with Change Notification
        public string? StripeUrl
        {
            get => stripeUrl;
            set => this.RaiseAndSetIfChanged(ref stripeUrl, value);
        }

        public string StripeName => throw new NotImplementedException();
        private string name = null!;

        public string Name {
            get => name;
            set => this.RaiseAndSetIfChanged(ref name, value);
        }

        internal override Task<Package> Populate()
        {
            return Task.FromResult(new Package()
            {
                Id = Id,
                Price = Price,
                StripeProductId = StripeProductId,
                StripePriceId = StripePriceId,
                StripeUrl = StripeUrl,
                ResumeGenerations = ResumeGenerations,
                Name = Name
            });
        }

        internal override Task Read(Package entity)
        {
            Id = entity.Id;
            Price = entity.Price;
            StripeProductId = entity.StripeProductId;
            StripePriceId = entity.StripePriceId;
            StripeUrl = entity.StripeUrl;
            ResumeGenerations = entity.ResumeGenerations;
            Name = entity.Name;
            return Task.CompletedTask;
        }
    }
}
