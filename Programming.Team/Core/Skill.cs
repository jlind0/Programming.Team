using System;
using System.Collections.Generic;

namespace Programming.Team.Core;
public interface ISkill : IEntity<Guid>
{
    string Name { get; set; }
}
public partial class Skill : Entity<Guid>, ISkill
{

    public string Name { get; set; } = null!;

    public virtual ICollection<PositionSkill> PositionSkills { get; set; } = new List<PositionSkill>();
}
