using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using Newtonsoft.Json.Linq;
using Programming.Team.AI.Core;
using Programming.Team.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Programming.Team.AI
{
    public class ChatService : IChatService
    {
        protected ILogger Logger { get; }
        protected IMcpClient McpClient { get; set; }
        protected IServiceProvider ServiceProvider { get; }
        public ChatService(ILogger<ChatService> logger, IMcpClient mcpClient, IServiceProvider serviceProvider)
        {
            Logger = logger;
            McpClient = mcpClient;
            ServiceProvider = serviceProvider;
        }
        public async Task<string?> ExtractSkills(string prompt, int maxTokens = 2048, CancellationToken token = default)
        {
            var args = new Dictionary<string, object?>
            {
                ["prompt"] = prompt,
                ["maxTokens"] = maxTokens
            };
            return await CallTool<string?>(msg => msg?.Text, "extractSkills", args, token);
        }
        public async Task<double> PercentMatch(string jd, string position, int maxTokens = 2048, CancellationToken token = default)
        {
            var args = new Dictionary<string, object?>
            {
                ["jd"] = jd,
                ["position"] = position,
                ["maxTokens"] = maxTokens
            };
            return await CallTool<double>(msg => msg?.Text == null ? 0d : double.Parse(msg.Text), "percentMatch", args, token);
        }

        public async Task<string?> TailorBio(string jd, string bio, int paragraphs = 3, int bullets = 6, int maxTokens = 2048, CancellationToken token = default)
        {
            var args = new Dictionary<string, object?>
            {
                ["jd"] = jd,
                ["bio"] = bio,
                ["paragraphs"] = paragraphs,
                ["bullets"] = bullets,
                ["maxTokens"] = maxTokens
            };
            return await CallTool<string?>(msg => msg?.Text, "tailorBio", args, token);
        }

        public async Task<string?> TailorPosition(string jd, string position, double bullets, int length, int maxTokens = 2048, CancellationToken token = default)
        {
            var args = new Dictionary<string, object?>
            {
                ["jd"] = jd,
                ["position"] = position,
                ["bullets"] = bullets,
                ["length"] = length,
                ["maxTokens"] = maxTokens
            };
            return await CallTool<string?>(msg => msg?.Text, "tailorPosition", args, token);
        }
        
        public async Task<string?> GenerateCoverLetter(string jd, string resume, int targetLength, int numberOfBullets, int maxTokens = 2048, CancellationToken token = default)
        {
            var args = new Dictionary<string, object?>
            {
                ["jd"] = jd,
                ["resume"] = resume,
                ["targetLength"] = targetLength,
                ["numberOfBullets"] = numberOfBullets,
                ["maxTokens"] = maxTokens
            };
            return await CallTool<string?>(msg => msg?.Text, "generateCoverLetter", args, token);
        }
        public async Task<string?> ExtractCompanyName(string jd, int maxTokens = 2048, CancellationToken token = default)
        {
            var args = new Dictionary<string, object?>
            {
                ["jd"] = jd,
                ["maxTokens"] = maxTokens
            };
            return await CallTool<string?>(msg => msg?.Text, "extractCompanyName", args, token);
        }
        public async Task<string?> ExtractPostingTitle(string jd, int maxTokens = 2048, CancellationToken token = default)
        {
            var args = new Dictionary<string, object?>
            {
                ["jd"] = jd,
                ["maxTokens"] = maxTokens
            };
            return await CallTool<string?>(msg => msg?.Text, "extractPostingTitle", args, token);
        }
        public async Task<string?> SummarizeResume(string resume, int pages = 3, int maxTokens = 2048, CancellationToken token = default)
        {
            var args = new Dictionary<string, object?>
            {
                ["resume"] = resume,
                ["pages"] = pages,
                ["maxTokens"] = maxTokens
            };
            var str = await CallTool<string?>(msg => msg?.Text, "summarizeResume", args, token);
            str = str?.Replace("```latex", "").Replace("```", "").Trim().ReplaceLineEndings();
            return str;
        }
        public async Task<string?> ResearchCompany(string companyName, int maxTokens = 2048, CancellationToken token = default)
        {
            var args = new Dictionary<string, object?>
            {
                ["company"] = companyName,
                ["maxTokens"] = maxTokens
            };
            var str = await CallTool<string?>(msg => msg?.Text, "researchCompany", args, token);
            str = str?.Replace("```markdown", "").Replace("```", "").Trim().ReplaceLineEndings();
            return str;
        }
        public async Task<string?> GenerateInterviewQuestions(string jd, string resume, int maxTokens = 2048, CancellationToken token = default)
        {
            var args = new Dictionary<string, object?>
            {
                ["jd"] = jd,
                ["resume"] = resume,
                ["maxTokens"] = maxTokens
            };
            var str = await CallTool<string?>(msg => msg?.Text, "generateInterviewQuestions", args, token);
            str = str?.Replace("```markdown", "").Replace("```", "").Trim().ReplaceLineEndings();
            return str;
        }
        public async Task<string?> GenerateEmployeerQuestions(string jd, string? companyResearch = null, int maxTokens = 2048, CancellationToken token = default)
        {
            var args = new Dictionary<string, object?>
            {
                ["jd"] = jd,
                ["companyResearch"] = companyResearch,
                ["maxTokens"] = maxTokens
            };
            var str = await CallTool<string?>(msg => msg?.Text, "generateEmployerQuestions", args, token);
            str = str?.Replace("```markdown", "").Replace("```", "").Trim().ReplaceLineEndings();
            return str;
        }
        protected async Task<T> CallTool<T>(Func<Content?, T> readValue, string toolName, IReadOnlyDictionary<string, object?> args, CancellationToken token)
        {
            bool isFirstAttempt = true;
            Func<Task<T>> attempt = () => throw new NotImplementedException();
            attempt = async () =>
            {
                try
                {

                    var resp = await McpClient.CallToolAsync(toolName, args, cancellationToken: token);
                    var message = resp?.Content?.FirstOrDefault();
                    return readValue(message);
                }
                catch (HttpRequestException ex)
                {
                    if (isFirstAttempt)
                    {
                        if (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                        {
                            McpClient = ServiceProvider.GetRequiredService<IMcpClient>();
                            isFirstAttempt = false;
                            return await attempt();
                        }
                    }
                    Logger.LogError(ex, ex.Message);
                    throw;
                }
                catch (Exception ex)
                {

                    Logger.LogError(ex, ex.Message);
                    throw;
                }
            };
            return await attempt();
        }

        
    }
}
