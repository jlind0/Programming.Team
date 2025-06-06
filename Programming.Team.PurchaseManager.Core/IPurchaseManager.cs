using Programming.Team.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Programming.Team.PurchaseManager.Core
{
    public interface IPurchaseManager<TPurchaseable, TPurchase>
        where TPurchaseable: Entity<Guid>, IStripePurchaseable, new()
        where TPurchase: Entity<Guid>, IStripePurchase, new()
    {
        Task ConfigurePackage(TPurchaseable purchaseable, CancellationToken token = default);
        Task FinishPurchase(TPurchase purchase, CancellationToken token = default);
        Task<TPurchase> StartPurchase(TPurchaseable purchaseable, CancellationToken token = default);
    }
}
