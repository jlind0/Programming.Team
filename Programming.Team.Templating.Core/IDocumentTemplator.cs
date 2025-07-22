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
        Task<string> ApplyTemplate<TObject>(string template, TObject obj, CancellationToken token = default)
            where TObject : class, new();
        Task<byte[]> RenderLatex(string latex, CancellationToken token = default);
        Task<string?> RenderHtmlFromMarkdown(string markdown, CancellationToken token = default);
        Task<string?> RenderMarkdownFromHtml(string html, CancellationToken token = default);
    }
    
}
