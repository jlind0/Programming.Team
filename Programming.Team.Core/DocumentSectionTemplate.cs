using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Programming.Team.Core
{
    public interface IDocumentSectionTemplate : IEntity<Guid>
    {
        Guid DocumentTemplateId { get; set; }
        Guid SectionTemplateId { get; set; }
        bool IsDefault { get; set; }
    }
    public interface IDocumentTemplatePurchase : IEntity<Guid>, IUserPartionedEntity
    {
        Guid DocumentTemplateId { get; set; }
        decimal PricePaid { get; set; }
        bool IsPaid { get; set; }
        string StripeSessionUrl { get; set; }

    }
    public class DocumentSectionTemplate : Entity<Guid>, IDocumentSectionTemplate
    {
        public Guid DocumentTemplateId { get; set; }
        public Guid SectionTemplateId { get; set; }
        public bool IsDefault { get; set; } = false;
        public virtual DocumentTemplate DocumentTemplate { get; set; } = null!;
        public virtual SectionTemplate SectionTemplate { get; set; } = null!;

    }
    public class DocumentTemplatePurchase : Entity<Guid>, IDocumentTemplatePurchase
    {
        public Guid DocumentTemplateId { get; set; }
        public decimal PricePaid { get; set; } = 0;
        public bool IsPaid { get; set; } = false;
        public string StripeSessionUrl { get; set; } = null!;
        public Guid UserId { get; set; }
        public virtual DocumentTemplate DocumentTemplate { get; set; } = null!;
        public virtual User User { get; set; } = null!;
    }
}
