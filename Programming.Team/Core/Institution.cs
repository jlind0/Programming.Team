using System;
using System.Collections.Generic;

namespace Programming.Team.Core;

public interface IInstitution : IEntity<Guid>
{
    string Name { get; set; }

    string? Description { get; set; }

    string? City { get; set; }

    string? State { get; set; }

    string? Country { get; set; }

    string? Url { get; set; }
}
public partial class Institution : Entity<Guid>, IInstitution
{
    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string? City { get; set; }

    public string? State { get; set; }

    public string? Country { get; set; }

    public string? Url { get; set; }

    public virtual ICollection<Education> Educations { get; set; } = new List<Education>();
}
