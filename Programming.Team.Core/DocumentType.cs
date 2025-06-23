using System;
using System.Collections.Generic;

namespace Programming.Team.Core;

public enum DocumentTypes
{
    Resume = 1,
    CoverLetter = 2,
    MarkdownResume = 5
}
public interface IDocumentType : IEntity<DocumentTypes>, INamedEntity
{
}
public partial class DocumentType : Entity<DocumentTypes>, IDocumentType
{
    public string Name { get; set; } = null!;

    public virtual ICollection<DocumentTemplate> DocumentTemplates { get; set; } = new List<DocumentTemplate>();
}
