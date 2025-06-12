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
        public DocumentTemplator(ILogger<DocumentTemplator> logger)
        {
            Logger = logger;
        }
        public async Task<string> ApplyTemplate(string template, Resume resume, CancellationToken token = default)
        {
            try
            {
                var templator = Template.Parse(template);
                var context = new ScriptObject();
                context.Import(resume);
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

        public async Task<string> ApplyTemplate(string template, CoverLetter coverLetter, CancellationToken token = default)
        {
            try
            {
                var templator = Template.Parse(template);
                var context = new ScriptObject();
                context.Import(coverLetter);
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
    }
}
