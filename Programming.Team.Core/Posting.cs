namespace Programming.Team.Core;

public interface IPosting : IEntity<Guid>, IUserPartionedEntity, INamedEntity
{
    Guid DocumentTemplateId { get; set; }


    string Details { get; set; }

    string? RenderedLaTex { get; set; }

    string? CoverLetterLaTeX { get; set; }
    string? CoverLetterConfiguration { get; set; }
    string? Configuration { get; set; }
    string? ResumeJson { get; set; }
    string? ResumeMarkdown { get; set; }
    string? CompanyName { get; set; }
    string? CompanyResearch { get; set; }
    string? InterviewQuestions { get; set; }
    string? QuestionsToAsk { get; set; }
    string? ResumeSummaryLatex { get; set; }
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
    public string? CoverLetterLaTeX { get; set; }
    public string? CoverLetterConfiguration { get; set; }
    public string? ResumeJson { get; set; }
    public string? ResumeMarkdown { get; set; }
    public string? CompanyName { get; set; }
    public string? CompanyResearch { get; set; }
    public string? InterviewQuestions { get; set; }
    public string? QuestionsToAsk { get; set; }
    public string? ResumeSummaryLatex { get; set; }
}
