namespace Programming.Team.Core;

public interface IPosting : IEntity<Guid>, IUserPartionedEntity
{
    Guid DocumentTemplateId { get; set; }

    string Name { get; set; }

    string Details { get; set; }

    string? RenderedLaTex { get; set; }


    string? Configuration { get; set; }
}
public partial class Posting : Entity<Guid>, IPosting
{

    public Guid DocumentTemplateId { get; set; }

    public Guid UserId { get; set; }

    public string Name { get; set; } = null!;

    public string Details { get; set; } = null!;

    public string? RenderedLaTex { get; set; }

    public string? Configuration { get; set; }

    public virtual DocumentTemplate DocumentTemplate { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
