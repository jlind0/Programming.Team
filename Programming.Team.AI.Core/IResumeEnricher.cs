/**
IResumeEnricher, IChatGPT, and INLP (natural language process)
*/
using Programming.Team.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Programming.Team.AI.Core
{
    /// <summary>
    /// Extracting skills or enhance resume data based on job postings.
    /// </summary>
    public interface IResumeEnricher
    {
        /// <summary>
        /// Extracts skills from the provided text.
        /// </summary>
        /// <param name="text">The text from which to extract skills.</param>
        /// <param name="token">Used to cancel the operation.</param>
        /// <returns>A task of the asynchronous operation. The task result contains an array of extracted skills, or <c>null</c> if no skills are found.</returns>
        Task<string[]?> ExtractSkills(string text, CancellationToken token = default);
        /// <summary>
        /// Enriches the provided resume based on the given job posting.
        /// </summary>
        /// <param name="resume">The resume to be enriched.</param>
        /// <param name="posting">The job posting used as a reference for enrichment.</param>
        /// <param name="progress">An optional progress reporter to provide updates during the enrichment process.</param>
        /// <param name="token">Used to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task EnrichResume(Resume resume, Posting posting, IProgress<string>? progress = null, CancellationToken token = default);
        Task<CoverLetter?> GenerateCoverLetter(Posting posting, IProgress<string>? progress = null, CancellationToken token = default);
    }
    public interface IChatService
    {
        Task<string?> ExtractSkills(string prompt, int maxTokens = 2048, CancellationToken token = default);
        Task<string?> TailorBio(string jd, string bio, int paragraphs = 3, int bullets = 6, int maxTokens = 2048, CancellationToken token = default);
        Task<double> PercentMatch(string jd, string position, int maxTokens = 2048, CancellationToken token = default);
        Task<string?> TailorPosition(string jd, string position, double bullets, int length, TextType inputFormat = TextType.Text, int maxTokens = 2048, CancellationToken token = default);
        Task<string?> GenerateCoverLetter(string jd, string resume, int targetLength, int numberOfBullets, int maxTokens = 2048, CancellationToken token = default);
        Task<string?> ExtractCompanyName(string jd, int maxTokens = 2048, CancellationToken token = default);
        Task<string?> ResearchCompany(string companyName, int maxTokens = 2048, CancellationToken token = default);
        Task<string?> ExtractPostingTitle(string jd, int maxTokens = 2048, CancellationToken token = default);
        Task<string?> GenerateInterviewQuestions(string jd, string resume, int maxTokens = 2048, CancellationToken token = default);
        Task<string?> GenerateEmployeerQuestions(string jd, string? companyResearch = null, int maxTokens = 2048, CancellationToken token = default);
        Task<string?> SummarizeResume(string resume, int pages = 3, int maxTokens = 2048, CancellationToken token = default);
        Task<string?> ConvertToLaTeX(string str, TextType inputFormat = TextType.Text, int maxTokens = 2048, CancellationToken token = default);
    }
    /// <summary>
    /// Provides natural language processing (NLP) functionality for identifying paragraphs in a given text.
    /// </summary>
    public interface INLP
    {
        /// <summary>
        /// Identifies and splits the input text into paragraphs.
        /// </summary>
        /// <param name="text">The input text to be processed.</param>
        /// <param name="token">Used to cancel the operation.</param>
        /// <returns>A task of the asynchronous operation. The task result contains an array of strings, where each string represents a paragraph.</returns>
        Task<string[]> IdentifyParagraphs(string text, CancellationToken token = default);
    }
}
