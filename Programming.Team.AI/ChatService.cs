﻿using Microsoft.Extensions.AI;
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

        public async Task<string?> TailorBio(string jd, string bio, int maxTokens = 2048, CancellationToken token = default)
        {
            var args = new Dictionary<string, object?>
            {
                ["jd"] = jd,
                ["bio"] = bio,
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
