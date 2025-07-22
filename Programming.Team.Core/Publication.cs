using Programming.Team.Core;
using System;
using System.Collections.Generic;

namespace Programming.Team.Core;

public interface IPublication : IEntity<Guid>, IUserPartionedEntity, IHasTextType
{

    string Title { get; set; }

    string? Description { get; set; }

    string Url { get; set; }

    DateOnly? PublishDate { get; set; }
}
public partial class Publication : Entity<Guid>, IPublication
{

    public Guid UserId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public string Url { get; set; } = null!;

    public DateOnly? PublishDate { get; set; }

    public virtual User User { get; set; } = null!;
    public TextType TextTypeId { get; set; }
}
