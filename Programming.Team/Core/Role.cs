using Programming.Team.Core;
using System;
using System.Collections.Generic;

namespace Programming.Team.Core;
public interface IRole : IEntity<Guid>
{
    string Name { get; set; }
}
public partial class Role : Entity<Guid>, IRole
{

    public string Name { get; set; } = null!;

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
