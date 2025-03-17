using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Programming.Team.AI.Core;

namespace Programming.Team.AI
{
    public class NLP : INLP
    {
        protected ILogger Logger { get; }
        public NLP(ILogger<NLP> logger)
        {
            Logger = logger;
        }
        public Task<string[]> IdentifyParagraphs(string text, CancellationToken token = default)
        {
            try
            {
                text = text.Replace("\r\n", "\n").Replace("\r", "\n");
                // Reconstruct paragraphs using double newlines as indicators
                return Task.FromResult(Regex.Split(text, @"\n\s*\n"));
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                throw;
            }
        }
    }
}
