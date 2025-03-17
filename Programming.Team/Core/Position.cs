﻿using System;
using System.Collections.Generic;

namespace Programming.Team.Core;
public interface IPosition : IEntity<Guid>, IUserPartionedEntity
{

    Guid CompanyId { get; set; }

    DateOnly StartDate { get; set; }

    DateOnly? EndDate { get; set; }

    string? Title { get; set; }

    string? Description { get; set; }

    string? SortOrder { get; set; }
}
public partial class Position : Entity<Guid>, IPosition
{

    public Guid UserId { get; set; }

    public Guid CompanyId { get; set; }

    public DateOnly StartDate { get; set; } = DateOnly.FromDateTime(DateTime.Now);
    public string StartDateString
    {
        get => StartDate.ToString("yyyy-MM-dd");
    }

    public DateOnly? EndDate { get; set; }
    public string? EndDateString
    {
        get => EndDate?.ToString("yyyy-MM-dd");
    }
    public string? Title { get; set; }

    public string? Description { get; set; }

    public string? SortOrder { get; set; }

    public virtual Company Company { get; set; } = null!;

    public virtual ICollection<PositionSkill> PositionSkills { get; set; } = new List<PositionSkill>();

    public virtual User User { get; set; } = null!;
    public virtual ICollection<Reccomendation> Reccomendations { get; set; } = new List<Reccomendation>();
}
