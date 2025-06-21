using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
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
        protected IMcpClient McpClient { get; }
        public ChatService(ILogger<ChatService> logger, IMcpClient mcpClient)
        {
            Logger = logger;
            McpClient = mcpClient;
        }
        public async Task<string?> ExtractSkills(string prompt, int maxTokens = 2048, CancellationToken token = default)
        {
            try
            {
                var args = new Dictionary<string, object?>
                {
                    ["prompt"] = prompt,
                    ["maxTokens"] = maxTokens
                };
                var resp = await McpClient.CallToolAsync("extractSkills", args, cancellationToken: token);
                var message = resp?.Content?.FirstOrDefault();
                if (message == null)
                    return null;

                return message.Text; ;
            }
            catch(Exception ex) 
            {

                Logger.LogError(ex, ex.Message);
                throw;
            }
        }
        public async Task<double> PercentMatch(string jd, string position, int maxTokens = 2048, CancellationToken token = default)
        {
            try
            {
                var args = new Dictionary<string, object?>
                {
                    ["jd"] = jd,
                    ["position"] = position,
                    ["maxTokens"] = maxTokens
                };
                var resp = await McpClient.CallToolAsync("percentMatch", args, cancellationToken: token);
                var message = resp?.Content?.FirstOrDefault();
                if (message?.Text == null)
                    return 0;

                return double.Parse(message.Text);
            }
            catch (Exception ex)
            {

                Logger.LogError(ex, ex.Message);
                throw;
            }
        }

        public async Task<string?> TailorBio(string jd, string bio, int maxTokens = 2048, CancellationToken token = default)
        {
            try
            {
                var args = new Dictionary<string, object?>
                {
                    ["jd"] = jd,
                    ["bio"] = bio,
                    ["maxTokens"] = maxTokens
                };
                var resp = await McpClient.CallToolAsync("tailorBio", args, cancellationToken: token);
                var message = resp?.Content?.FirstOrDefault();
                if (message == null)
                    return null;

                return message.Text; ;
            }
            catch (Exception ex)
            {

                Logger.LogError(ex, ex.Message);
                throw;
            }
        }

        public async Task<string?> TailorPosition(string jd, string position, double bullets, int length, int maxTokens = 2048, CancellationToken token = default)
        {
            try
            {
                var args = new Dictionary<string, object?>
                {
                    ["jd"] = jd,
                    ["position"] = position,
                    ["bullets"] = bullets,
                    ["length"] = length,
                    ["maxTokens"] = maxTokens
                };
                var resp = await McpClient.CallToolAsync("tailorPosition", args, cancellationToken: token);
                var message = resp?.Content?.FirstOrDefault();
                if (message == null)
                    return null;

                return message.Text; ;
            }
            catch (Exception ex)
            {

                Logger.LogError(ex, ex.Message);
                throw;
            }
        }

        public async Task<string?> GenerateCoverLetter(string jd, string resume, int targetLength, int numberOfBullets, int maxTokens = 2048, CancellationToken token = default)
        {
            try
            {
                var args = new Dictionary<string, object?>
                {
                    ["jd"] = jd,
                    ["resume"] = resume,
                    ["targetLength"] = targetLength,
                    ["numberOfBullets"] = numberOfBullets,
                    ["maxTokens"] = maxTokens
                };
                var resp = await McpClient.CallToolAsync("generateCoverLetter", args, cancellationToken: token);
                var message = resp?.Content?.FirstOrDefault();
                if (message == null)
                    return null;

                return message.Text; ;
            }
            catch (Exception ex)
            {

                Logger.LogError(ex, ex.Message);
                throw;
            }
        }
    }
}
