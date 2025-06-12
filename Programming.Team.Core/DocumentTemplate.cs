using System;
using System.Collections.Generic;

namespace Programming.Team.Core;
public enum ApprovalStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2
}
public interface IStripePurchaseable
{
    decimal? Price { get; set; }
    string? StripeProductId { get; set; }
    string? StripePriceId { get; set; }
    string? StripeUrl { get; set; }
    string StripeName { get; }
}
public interface IDocumentTemplate : IEntity<Guid>, INamedEntity, IStripePurchaseable
{
    DocumentTypes DocumentTypeId { get; set; }

    string Template { get; set; }
    Guid? OwnerId { get; set; }
    
    ApprovalStatus ApprovalStatus { get; set; }
}
public partial class DocumentTemplate : Entity<Guid>, IDocumentTemplate
{
    public DocumentTypes DocumentTypeId { get; set; }
    public Guid? OwnerId { get; set; }
    public string Name { get; set; } = null!;

    public string Template { get; set; } = null!;
    public decimal? Price { get; set; } = null!;
    public ApprovalStatus ApprovalStatus { get; set; } = ApprovalStatus.Pending;
    public string StripeName => $"Document Template {Name}";
    public virtual DocumentType DocumentType { get; set; } = null!;

    public virtual ICollection<Posting> Postings { get; set; } = new List<Posting>();
    public virtual User? Owner { get; set; }
    public virtual ICollection<DocumentSectionTemplate> DocumentSectionTemplates { get; set; } = new List<DocumentSectionTemplate>();
    public virtual ICollection<DocumentTemplatePurchase> TemplatePurchases { get; set; } = new List<DocumentTemplatePurchase>();
    public string? StripeProductId { get; set; }
    public string? StripePriceId { get; set; }
    public string? StripeUrl { get; set; }
}
