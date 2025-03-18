using System;
using System.Collections.Generic;

namespace Programming.Team.Core;

public interface IDocumentType : IEntity<int>, INamedEntity
{
    string Name { get; set; }
}
public partial class DocumentType : Entity<int>, IDocumentType
{
    public string Name { get; set; } = null!;

    public virtual ICollection<DocumentTemplate> DocumentTemplates { get; set; } = new List<DocumentTemplate>();
}
