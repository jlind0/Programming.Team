using Programming.Team.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Programming.Team.PurchaseManager.Core
{
    public interface IPurchaseManager
    {
        Task ConfigurePackage(Package package, CancellationToken token = default);
        Task FinishPurchase(Guid purchaseId, CancellationToken token = default);
        Task<Purchase> StartPurchase(Guid packageId, CancellationToken token = default);
    }
}
