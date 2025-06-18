using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Programming.Team.Core
{
    public interface IFAQ : IEntity<Guid>
    {
        string Question { get; set; }
        string Answer { get; set; }
        string? SortOrder { get; set; }
    }
    public class FAQ : Entity<Guid>, IFAQ
    {
        public string Question { get; set; } = null!;
        public string Answer { get; set; } = null!;
        public string? SortOrder { get; set; }
    }
}
