using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Xml.Linq;

namespace Programming.Team.Web.Controllers
{
    [Route("/sitemap.xml")]
    [ApiController]
    [AllowAnonymous]
    public class SiteMapXmlController : ControllerBase
    {
        [HttpGet("{path?}")]
        public async Task<IActionResult> Get(string? path = null)
        {
            path ??= "sitemap.xml";
            using(var httpClient = new HttpClient())
            {
                try
                {
                    var response = await httpClient.GetAsync($"https://blog.programming.team/{path}");
                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        XDocument doc = XDocument.Parse(content);
                        var xsltInstruction = doc.Nodes()
                                 .OfType<XProcessingInstruction>()
                                 .FirstOrDefault(pi => pi.Target == "xml-stylesheet");

                        if (xsltInstruction != null)
                        {
                            xsltInstruction.Remove();
                        }
                        XNamespace sm = "http://www.sitemaps.org/schemas/sitemap/0.9";
                        // Find all 'loc' elements directly within any 'sitemap' elements
                        foreach (var locElement in doc.Descendants(sm + "loc"))
                        {
                            var loc = locElement.Value; // Access the value directly from the loc element
                            if (!string.IsNullOrWhiteSpace(loc) && loc.Contains("blog.programming.team"))
                            {
                                if(loc.EndsWith(".xml"))
                                    locElement.Value = loc.Replace("blog.programming.team/", "programming.team/sitemap.xml/");
                                else
                                    locElement.Value = loc.Replace("blog.programming.team/", "programming.team/blog/");
                            }
                        }
                        return Content(doc.ToString(), "application/xml");
                    }
                    else
                    {
                        return StatusCode((int)response.StatusCode, "Failed to retrieve sitemap.");
                    }
                }
                catch (Exception ex)
                {
                    return StatusCode(500, $"Error retrieving sitemap: {ex.Message}");
                }
            }
        }
    }
}
