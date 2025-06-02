using System;
using System.Collections.Generic;

namespace Programming.Team.Core;
public enum ApprovalStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2
}
public interface IDocumentTemplate : IEntity<Guid>, INamedEntity
{
    int DocumentTypeId { get; set; }

    string Template { get; set; }
    Guid? OwnerId { get; set; }
    decimal? Price { get; set; }
    ApprovalStatus ApprovalStatus { get; set; }
}
public partial class DocumentTemplate : Entity<Guid>, IDocumentTemplate
{
    public int DocumentTypeId { get; set; }
    public Guid? OwnerId { get; set; }
    public string Name { get; set; } = null!;

    public string Template { get; set; } = null!;
    public decimal? Price { get; set; } = null!;
    public ApprovalStatus ApprovalStatus { get; set; } = ApprovalStatus.Pending;

    public virtual DocumentType DocumentType { get; set; } = null!;

    public virtual ICollection<Posting> Postings { get; set; } = new List<Posting>();
    public virtual User? Owner { get; set; }
    public virtual ICollection<DocumentSectionTemplate> DocumentSectionTemplates { get; set; } = new List<DocumentSectionTemplate>();
    public virtual ICollection<DocumentTemplatePurchase> TemplatePurchases { get; set; } = new List<DocumentTemplatePurchase>();

}
