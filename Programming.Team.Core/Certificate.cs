using System;
using System.Collections.Generic;

namespace Programming.Team.Core;
public interface ICertificate : IEntity<Guid>, IUserPartionedEntity, INamedEntity, IHasTextType
{
    Guid IssuerId { get; set; }


    DateOnly ValidFromDate { get; set; }

    DateOnly? ValidToDate { get; set; }

    string? Url { get; set; }

    string? Description { get; set; }
}
public partial class Certificate : Entity<Guid>, ICertificate
{

    public Guid IssuerId { get; set; }

    public Guid UserId { get; set; }

    public string Name { get; set; } = null!;

    public DateOnly ValidFromDate { get; set; }

    public DateOnly? ValidToDate { get; set; }

    public string ValidFromDateString
    {
        get => ValidFromDate.ToString("yyyy-MM-dd");
    }

    public string? ValidToDateString
    {
        get => ValidToDate?.ToString("yyyy-MM-dd");
    }

    public string? Url { get; set; }

    public string? Description { get; set; }

    public virtual CertificateIssuer Issuer { get; set; } = null!;

    public virtual User User { get; set; } = null!;
    public TextType TextTypeId { get; set; }
}
