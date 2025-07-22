using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Programming.Team.Core
{
    public interface IProject : IEntity<Guid>, INamedEntity, IUserPartionedEntity, IHasTextType
    {
        Guid PositionId { get; set; }
        string? Description { get; set; }
        string? ProjectUrl { get; set; }
        string? SourceUrl { get; set; }
        string? License { get; set; }
        string? SortOrder { get; set; }
    }
    public class Project : Entity<Guid>, IProject
    {
        public Guid PositionId { get; set; }
        public virtual Position Position { get; set; } = null!;
        public string? Description { get; set; }
        public string? ProjectUrl { get; set; }
        public string? SourceUrl { get; set; }
        public string? License { get; set; }
        public string Name { get; set; } = null!;
        public Guid UserId { get; set; }
        public virtual User User { get; set; } = null!;
        public virtual ICollection<ProjectSkill> ProjectSkills { get; set; } = [];
        public string? SortOrder { get; set; }
        public TextType TextTypeId { get; set; }
    }
    public interface IProjectSkill : IEntity<Guid>, IHasTextType
    {
        Guid ProjectId { get; set; }
        Guid SkillId { get; set; }
        string? Description { get; set; }
    }
    public class ProjectSkill : Entity<Guid>, IProjectSkill
    {
        public Guid ProjectId { get; set; }
        public virtual Project Project { get; set; } = null!;
        public Guid SkillId { get; set; }
        public virtual Skill Skill { get; set; } = null!;
        public string? Description { get; set; }
        public TextType TextTypeId { get; set; }
    }
}
