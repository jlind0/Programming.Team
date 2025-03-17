using Programming.Team.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Programming.Team.Templating.Core
{
    public interface IDocumentTemplator
    {
        Task<string> ApplyTemplate(string template, Resume resume, CancellationToken token = default);
        Task<byte[]> RenderLatex(string latex, CancellationToken token = default);
    }
}
