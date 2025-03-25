using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text;
using System.IO.Compression;

namespace Programming.Team.Web.Authorization
{
    public class RolePopulationMiddleware
    {
        private readonly RequestDelegate _next;
        public RolePopulationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.User.Identity != null && context.User.Identity.IsAuthenticated)
            {
                var identity = context.User.Identity as ClaimsIdentity;
                if (identity != null && identity.HasClaim(c => c.Type == "extension_Roles"))
                {
                    var rolesClaim = context.User.FindFirst(c => c.Type == "extension_Roles") ?? throw new InvalidDataException();
                    var roles = rolesClaim.Value.Split(',').Select(role => role.Trim());
                    foreach (var role in roles)
                    {
                        identity.AddClaim(new Claim(ClaimTypes.Role, role));
                    }

                }
            }
            await _next(context);
        }
    }
public class LinkRewritingMiddleware
    {
        private readonly RequestDelegate _next;

        public LinkRewritingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Only apply for routes starting with /blog
            if (context.Request.Path.StartsWithSegments("/blog"))
            {
                // Backup the original response stream.
                var originalBodyStream = context.Response.Body;

                using (var responseBody = new MemoryStream())
                {
                    // Replace the response stream with our memory stream.
                    context.Response.Body = responseBody;

                    // Process the request down the pipeline.
                    await _next(context);

                    // Reset the stream position to the beginning.
                    responseBody.Seek(0, SeekOrigin.Begin);

                    // Read the content, decompressing if necessary.
                    string bodyText;
                    var contentEncoding = context.Response.Headers["Content-Encoding"].ToString();

                    if (contentEncoding.Contains("gzip"))
                    {
                        using (var decompressionStream = new GZipStream(responseBody, CompressionMode.Decompress))
                        using (var reader = new StreamReader(decompressionStream))
                        {
                            bodyText = await reader.ReadToEndAsync();
                        }
                    }
                    else if (contentEncoding.Contains("deflate"))
                    {
                        using (var decompressionStream = new DeflateStream(responseBody, CompressionMode.Decompress))
                        using (var reader = new StreamReader(decompressionStream))
                        {
                            bodyText = await reader.ReadToEndAsync();
                        }
                    }
                    else
                    {
                        using (var reader = new StreamReader(responseBody))
                        {
                            bodyText = await reader.ReadToEndAsync();
                        }
                    }

                    // Perform the link rewriting.
                    string modifiedBody = bodyText.Replace(
                        "https://blog.programming.team/",
                        "https://programming.team/blog/");

                    // Remove compression headers because the content is now modified (and uncompressed).
                    context.Response.Headers.Remove("Content-Encoding");
                    context.Response.Headers.Remove("Content-Length");

                    // Reset the response body stream to the original stream.
                    context.Response.Body = originalBodyStream;

                    // Write out the modified content.
                    await context.Response.WriteAsync(modifiedBody, Encoding.UTF8);
                }
            }
            else
            {
                // For non-/blog requests, simply continue processing.
                await _next(context);
            }
        }
    }


    public class RoleAuthorizationAttribute : AuthorizeAttribute, IAuthorizationFilter
    {
        readonly string[] _roles;

        public RoleAuthorizationAttribute(params string[] roles)
        {
            _roles = roles;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if (context != null && context.HttpContext != null)
            {
                var isAuthenticated = context.HttpContext.User.Identity?.IsAuthenticated == true;
                if (!isAuthenticated)
                {
                    context.Result = new UnauthorizedResult();
                    return;
                }

                var hasAllRequredClaims = _roles.All(r => context.HttpContext.User.IsInRole(r));
                if (!hasAllRequredClaims)
                {
                    context.Result = new ForbidResult();
                    return;
                }
            }
        }
    }
}
