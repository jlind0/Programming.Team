using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Programming.Team.AI.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Programming.Team.AI
{
    public class ChatGPT : IChatGPT
    {
        protected string ApiKey { get; }
        protected string Enpoint { get; }
        protected ILogger Logger { get; }
        public ChatGPT(IConfiguration config, ILogger<ChatGPT> logger)
        {
            ApiKey = config["ChatGPT:ApiKey"] ?? throw new InvalidDataException();
            Enpoint = config["ChatGPT:Endpoint"] ?? throw new InvalidDataException();
            Logger = logger;
        }
        public async Task<string?> GetRepsonse(string systemPrompt, string userPrompt, int maxTokens = 2048, CancellationToken token = default)
        {
            try
            {

                using (var client = new HttpClient())
                using (var request = new HttpRequestMessage())
                {
                    // Build the request.
                    request.Method = HttpMethod.Post;
                    request.RequestUri = new Uri(Enpoint);
                    var requestBody = $"{{\"model\": \"gpt-4o-mini\", \"messages\": [{{\"role\": \"system\", \"content\":{JsonSerializer.Serialize(systemPrompt)}}},{{\"role\": \"user\", \"content\": {JsonSerializer.Serialize(userPrompt)}}}], \"temperature\": 1, \"max_tokens\": {maxTokens}, \"top_p\": 1, \"frequency_penalty\":0 , \"presence_penalty\": 0}}";
                    Logger.LogInformation(requestBody);
                    request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", ApiKey);
                    // Send POST request
                    HttpResponseMessage response = await client.SendAsync(request, token);
                    // Read response as a string.
                    response.EnsureSuccessStatusCode();
                    // Read response
                    string result = await response.Content.ReadAsStringAsync();
                    Logger.LogInformation(result);
                    JObject jsonResponse = JObject.Parse(result);
                    var reply = jsonResponse["choices"][0]["message"]["content"].ToString();
                    Logger.LogInformation(reply);
                    return reply;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                return null;
            }
        }
    }
}
