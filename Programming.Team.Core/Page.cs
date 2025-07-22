using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Programming.Team.Core
{
    public interface IPage: IEntity<Guid>, INamedEntity
    {
        string Route { get; set; }
        string? Markdown { get; set; }
        string? VideoEmbed { get; set; }
    }
    public partial class Page : Entity<Guid>, IPage
    {
        public string Route { get; set; } = null!;
        public string? Markdown { get; set; }
        public string? VideoEmbed { get; set; }
        public string Name { get; set; } = null!;
    }
}
