using Markdig;
using Microsoft.Extensions.Logging;
using Programming.Team.Core;
using Programming.Team.Templating.Core;
using Scriban;
using Scriban.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using static System.Net.WebRequestMethods;

namespace Programming.Team.Templating
{
    public class DocumentTemplator : IDocumentTemplator
    {
        protected ILogger Logger { get; }
        protected MarkdownPipeline Pipeline { get; }
        protected ReverseMarkdown.Converter Converter { get; }
        public DocumentTemplator(ILogger<DocumentTemplator> logger, MarkdownPipeline pipeline, ReverseMarkdown.Converter converter)
        {
            Logger = logger;
            Pipeline = pipeline;
            Converter = converter;
        }
        public async Task<string> ApplyTemplate<TObject>(string template, TObject obj, CancellationToken token = default)
            where TObject : class, new()
        {
            try
            {
                var templator = Template.Parse(template);
                var context = new ScriptObject();
                context.Import(obj);
                var scriptContext = new TemplateContext { MemberRenamer = member => member.Name };
                scriptContext.PushGlobal(context);

                return await templator.RenderAsync(scriptContext);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                throw;
            }
        }
        protected class LatexRequest
        {
            public string latex { get; set; } = null!;
        }


        public async Task<byte[]> RenderLatex(string latex, CancellationToken token = default)
        {
            string url = "https://api.programming.team/Latex/compile";

            try
            {
                // Define the HTTP request payload
                LatexRequest request = new LatexRequest()
                {
                    latex = latex
                };

                // Serialize the request object to JSON
                var content = JsonContent.Create(request);

                // Send the HTTP POST request
                using var client = new HttpClient();
                var response = await client.PostAsync(url, content, token);

                // Ensure the response indicates success
                response.EnsureSuccessStatusCode();

                // Return the response content as a byte array
                return await response.Content.ReadAsByteArrayAsync();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                throw; // Re-throw the exception to let the caller handle it
            }
        }

        public Task<string?> RenderHtmlFromMarkdown(string markdown, CancellationToken token = default)
        {
            try
            {
                
                return Task.FromResult<string?>(Markdown.ToHtml(markdown, Pipeline));
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                throw;
            }
        }

        public Task<string?> RenderMarkdownFromHtml(string html, CancellationToken token = default)
        {
            try
            {

                return Task.FromResult<string?>(Converter.Convert(html));
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                throw;
            }
        }
    }
}
