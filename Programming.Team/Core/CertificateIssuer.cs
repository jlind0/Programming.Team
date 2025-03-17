using System;
using System.Collections.Generic;

namespace Programming.Team.Core;
public interface ICertificateIssuer : IEntity<Guid>
{
    string Name { get; set; }

    string? Description { get; set; }

    string? Url { get; set; }
}
public partial class CertificateIssuer : Entity<Guid>, ICertificateIssuer
{

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string? Url { get; set; }

    public virtual ICollection<Certificate> Certificates { get; set; } = new List<Certificate>();
}
