
using System;
using System.Collections.Generic;

namespace Programming.Team.Core;

public interface IRecommendation : IEntity<Guid>, IUserPartionedEntity, INamedEntity
{

    Guid PositionId { get; set; }


    string Body { get; set; }

    string? SortOrder { get; set; }
    string? Title { get; set; }
}
public partial class Recommendation : Entity<Guid>, IRecommendation
{
    public Guid UserId { get; set; }

    public Guid PositionId { get; set; }

    public string Name { get; set; } = null!;

    public string Body { get; set; } = null!;

    public string? SortOrder { get; set; }
    public string? Title { get; set; }

    public virtual Position Position { get; set; } = null!;


    public virtual User User { get; set; } = null!;
}
