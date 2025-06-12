using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Programming.Team.Core
{
    public interface IPurchase : IEntity<Guid>, IStripePurchase
    {
        Guid PackageId { get; set; }

        int ResumeGenerations { get; set; }
    }
    public class Purchase : Entity<Guid>, IPurchase
    {
        public Guid UserId { get; set; }

        public Guid PackageId { get; set; }

        public bool IsPaid { get; set; }

        public decimal PricePaid { get; set; }

        public int ResumeGenerations { get; set; }

        public string StripeSessionUrl { get; set; } = null!;

        public virtual Package Package { get; set; } = null!;

        public virtual User User { get; set; } = null!;
        public bool IsRefunded { get; set; }
        public DateTime? RefundDate { get; set; }
        public string? StripePaymentIntentId { get; set; }
    }
}
