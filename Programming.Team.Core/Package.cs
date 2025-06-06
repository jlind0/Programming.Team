using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Programming.Team.Core
{
    public interface IPackage : IEntity<Guid>, IStripePurchaseable
    {
        int ResumeGenerations { get; set; }
    }
    public class Package : Entity<Guid>, IPackage
    {
        public decimal? Price { get; set; }
        public int ResumeGenerations { get; set; } 
        public string? StripeProductId { get; set; }
        public string? StripePriceId { get; set; }
        public string? StripeUrl { get; set; }
        public virtual ICollection<Purchase> Purchases { get; set; } = new List<Purchase>();
        public string StripeName => $"{ResumeGenerations} Resume Generations Package";
    }
}
