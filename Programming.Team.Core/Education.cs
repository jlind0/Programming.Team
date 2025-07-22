using System;
using System.Collections.Generic;

namespace Programming.Team.Core;

public interface IEducation : IEntity<Guid>, IUserPartionedEntity, IHasTextType
{
    Guid InstitutionId { get; set; }

    string? Major { get; set; }

    DateOnly StartDate { get; set; }

    DateOnly? EndDate { get; set; }

    string? Description { get; set; }

    bool Graduated { get; set; }
}
public partial class Education : Entity<Guid>, IEducation
{
   

    public Guid InstitutionId { get; set; }

    public Guid UserId { get; set; }

    public string? Major { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly? EndDate { get; set; }
    public string StartDateString
    {
        get => StartDate.ToString("yyyy-MM-dd");
    }

    public string? EndDateString
    {
        get => EndDate?.ToString("yyyy-MM-dd");
    }

    public string? Description { get; set; }

    public bool Graduated { get; set; }

    public virtual Institution Institution { get; set; } = null!;

    public virtual User User { get; set; } = null!;
    public TextType TextTypeId { get; set; }
}
