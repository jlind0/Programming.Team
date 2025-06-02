using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Programming.Team.Core
{
    public interface ISectionTemplate : IEntity<Guid>, INamedEntity
    {
        ResumePart SectionId { get; set; }
        string Template { get; set; }
    }
    public class SectionTemplate : Entity<Guid>, ISectionTemplate
    {
        public ResumePart SectionId { get; set; }
        public string Name { get; set; } = null!;
        public string Template { get; set; } = null!;
        public virtual ICollection<DocumentSectionTemplate> DocumentSectionTemplates { get; set; } = new List<DocumentSectionTemplate>();
    }
}
