using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Programming.Team.Core
{
    public interface IEmailMessageTemplate : IEntity<Guid>, INamedEntity
    {
        string MessageTemplate { get; set; }
        string SubjectTemplate { get; set; }
        bool IsHtml { get; set; }
    }
    public class EmailMessageTemplate : Entity<Guid>, IEmailMessageTemplate
    {
        public string MessageTemplate { get; set; } = null!;
        public string SubjectTemplate { get; set; } = null!;
        public bool IsHtml { get; set; } = true;
        public string Name { get; set; } = null!;
    } 
}
