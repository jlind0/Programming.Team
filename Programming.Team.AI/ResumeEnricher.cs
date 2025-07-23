using Microsoft.Extensions.Logging;
using Programming.Team.AI.Core;
using Programming.Team.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Programming.Team.AI
{
    ///<inheritdoc cref="IResumeEnricher"/>
    public class ResumeEnricher : IResumeEnricher
    {
        protected ILogger Logger { get; }
        protected IChatService ChatService { get; }
        public ResumeEnricher(ILogger<ResumeEnricher> logger, IChatService chatService)
        {
            Logger = logger;
            ChatService = chatService;
        }
        public async Task EnrichResume(Resume resume, Posting posting, IProgress<string>? progress = null, CancellationToken token = default)
        {
            try
            {
                var config = !string.IsNullOrWhiteSpace(posting.Configuration) ?
                    JsonSerializer.Deserialize<ResumeConfiguration>(posting.Configuration) ?? throw new InvalidDataException() : new ResumeConfiguration();

                if (config.HideSkillsNotInJD)
                {
                    progress?.Report("Filtering Skills");
                    try
                    {
                        var skills = await ExtractSkills(posting.Details, token);
                        if (skills != null)
                        {
                            HashSet<SkillRollup> globalSkills = new HashSet<SkillRollup>();
                            foreach (var skill in skills)
                            {
                                foreach (var skillRollup in resume.Skills.Where(s => string.Equals(s.Skill.Name.Replace("\\", ""), skill, StringComparison.OrdinalIgnoreCase)))
                                {

                                    globalSkills.Add(skillRollup);
                                }

                            }
                            resume.Skills = globalSkills.OrderByDescending(e => e.YearsOfExperience).ToList();
                        }
                    }
                    catch { }
                }

                if (!string.IsNullOrWhiteSpace(resume.User.Bio))
                {
                    progress?.Report("Tailoring Bio");
                    resume.User.Bio = await ChatService.TailorBio(posting.Details, resume.User.Bio, config.BioParagraphs ?? 3, config.BioBullets ?? 6, token: token);
                    resume.User.Bio = resume.User.Bio?.Replace("#", "\\#").Replace("$", "\\$").Replace("&", "\\&").Replace("%", "\\%");
                }
                foreach (var skill in resume.Skills.Select(p => p.Skill).Union(resume.Positions.SelectMany(p => p.PositionSkills.Select(p => p.Skill))))
                {
                    skill.Name = skill.Name.Replace("#", "\\#").Replace("$", "\\$").Replace("&", "\\&").Replace("%", "\\%");
                }
                progress?.Report("Tailoring Recommendations");
                foreach (var rec in resume.Recommendations)
                {
                    rec.Name = rec.Name.Replace("#", "\\#").Replace("$", "\\$").Replace("&", "\\&").Replace("%", "\\%");
                    if (string.IsNullOrWhiteSpace(rec.Body))
                        continue;
                    rec.Body = await ChatService.ConvertToLaTeX(rec.Body, rec.TextTypeId, token: token) ?? string.Empty;
                    //rec.Body = rec.Body.Replace("#", "\\#").Replace("$", "\\$").Replace("&", "\\&").Replace("%", "\\%");

                }
                progress?.Report("Tailoring Education");
                foreach (var edu in resume.Educations)
                {
                    if(string.IsNullOrEmpty(edu.Description))
                        continue;
                    edu.Description = await ChatService.ConvertToLaTeX(edu.Description, edu.TextTypeId, token: token);
                    //edu.Description = edu.Description?.Replace("#", "\\#").Replace("$", "\\$").Replace("&", "\\&").Replace("%", "\\%");
                }
                progress?.Report("Tailoring Publications");
                foreach (var pub in resume.Publications)
                {
                    pub.Title = pub.Title.Replace("#", "\\#").Replace("$", "\\$").Replace("&", "\\&").Replace("%", "\\%");
                    if(string.IsNullOrWhiteSpace(pub.Description))
                        continue;
                    pub.Description = await ChatService.ConvertToLaTeX(pub.Description, pub.TextTypeId, token: token);
                    //pub.Description = pub.Description?.Replace("#", "\\#").Replace("$", "\\$").Replace("&", "\\&").Replace("%", "\\%");
                }
                progress?.Report("Tailoring Certs");
                foreach(var cert in resume.Certificates)
                {
                    cert.Name = cert.Name.Replace("#", "\\#").Replace("$", "\\$").Replace("&", "\\&").Replace("%", "\\%");
                    if (string.IsNullOrWhiteSpace(cert.Description))
                        continue;
                    cert.Description = await ChatService.ConvertToLaTeX(cert.Description, cert.TextTypeId, token: token);
                    //cert.Description = cert.Description?.Replace("#", "\\#").Replace("$", "\\$").Replace("&", "\\&").Replace("%", "\\%");
                }
                int count = resume.Positions.Count;
                int numberProcessed = 0;
                List<Position> toRemove = new List<Position>();
                await Parallel.ForEachAsync(resume.Positions, token, async (position, t) =>
                {
                    if (!string.IsNullOrWhiteSpace(position.Description))
                    {
                        var mtch = await ChatService.PercentMatch(posting.Details, position.Description, token: token);
                        
                        if (mtch >= (config.MatchThreshold ?? 0.4))
                        {
                            double length = mtch * 10 * (config.TargetLengthPer10Percent ?? 75);
                            double bullets = (Math.Ceiling((mtch / 0.2) * (config.BulletsPer20Percent ?? 0.75)));
                            if (bullets < 2)
                                bullets = 2;
                            position.Description = await ChatService.TailorPosition(posting.Details, position.Description, bullets, Convert.ToInt32(length), position.TextTypeId, token: token);
                            position.Description = position.Description?.Replace("#", "\\#").Replace("$", "\\$").Replace("&", "\\&").Replace("%", "\\%");

                        }
                        else if (!config.HidePositionsNotInJD)
                            position.Description = "";
                        else
                            toRemove.Add(position);
                        if (config.SkillsPer20Percent != null)
                        {
                            position.PositionSkills = position.PositionSkills.Take(Math.Max(Convert.ToInt32((mtch / 0.2) * config.SkillsPer20Percent.Value), 10)).ToList();
                        }
                    }
                    int currentCount = Interlocked.Increment(ref numberProcessed);
                    progress?.Report($"{currentCount}/{count} Positions Processed");
                });
                foreach (var position in toRemove)
                {
                    resume.Positions.Remove(position);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                throw;
            }
        }

        public async Task<string[]?> ExtractSkills(string text, CancellationToken token = default)
        {
            try
            {
                string? postingSkills = await ChatService.ExtractSkills(text, token: token);
                if (postingSkills != null)
                {
                    postingSkills = postingSkills.Replace("```json", "").Replace("```", "").Trim().ReplaceLineEndings();
                    var skills = JsonSerializer.Deserialize<List<Dictionary<string, string>>>(postingSkills);
                    return skills?.Select(r => r["skill"]).ToArray();
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                throw;
            }
            return null;
        }

        public async Task<CoverLetter?> GenerateCoverLetter(Posting posting, IProgress<string>? progress = null, CancellationToken token = default)
        {
            try 
            { 
                if(string.IsNullOrWhiteSpace(posting.RenderedLaTex))
                    throw new InvalidDataException("Posting must have a rendered LaTeX to generate a cover letter.");
                progress?.Report("Starting Cover Letter Generation");
                CoverLetterConfiguration config = posting.CoverLetterConfiguration != null ? 
                    JsonSerializer.Deserialize<CoverLetterConfiguration>(posting.CoverLetterConfiguration) ?? new CoverLetterConfiguration() : new CoverLetterConfiguration();
                posting.CoverLetterConfiguration = JsonSerializer.Serialize(config);
                progress?.Report("Generating Cover Letter");
                var coverLetter = await ChatService.GenerateCoverLetter(posting.Details, posting.RenderedLaTex, config.TargetLength ?? 2000, config.NumberOfBullets ?? 10, token: token);
                coverLetter = coverLetter?.Replace("#", "\\#").Replace("$", "\\$").Replace("&", "\\&").Replace("%", "\\%").Replace("```latex", "").Replace("```", "");
                progress?.Report("Cover Letter Generated Successfully");
                return new CoverLetter() { Body = coverLetter ?? throw new InvalidOperationException() };
            }
            catch(Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                throw;
            }
        }
    }
}
