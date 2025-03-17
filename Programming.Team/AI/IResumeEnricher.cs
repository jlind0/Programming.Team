using Programming.Team.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Programming.Team.AI.Core
{
    public interface IResumeEnricher
    {
        Task<string[]?> ExtractSkills(string text, CancellationToken token = default);
        Task EnrichResume(Resume resume, Posting posting, IProgress<string>? progress = null, CancellationToken token = default);
    }
    public interface IChatGPT
    {
        Task<string?> GetRepsonse(string systemPrompt, string userPrompt, int maxTokens = 2048, CancellationToken token = default);
    }
    public interface INLP
    {
        Task<string[]> IdentifyParagraphs(string text, CancellationToken token = default);
    }
}
