﻿using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using Programming.Team.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Programming.Team.AI.MCP
{
    [McpServerToolType]
    public sealed class SkillExtractionTool
    {
        [McpServerTool(Name = "extractSkills"), Description("Extract skills")]
        public static async Task<string?> ExtractSkills(
            IMcpServer thisServer,
            [Description("Text to extract skills from")] string prompt,
            [Description("Maximum number of tokens to generate")] int maxTokens = 2048,
            CancellationToken cancellationToken = default)
        {
            try
            {
                ChatMessage[] messages =
                [
                    new(ChatRole.User,
                        """
                        
                        You are a skills-extraction service.  
                        Your _only_ output must be a JSON array of objects like:
                            [
                            {"skill":"<skill name>"},
                            {"skill":"<other skill>"}
                            ]
                        Do _not_ emit any other text, explanation, or formatting for the following message:
                     
                        """ + prompt)
                ];

                // 3) Call the model
                var resp = await thisServer
                    .AsSamplingChatClient()
                    .GetResponseAsync(
                        messages,
                        new ChatOptions
                        {
                            MaxOutputTokens = maxTokens,
                            Temperature = 1f,
                            TopP = 1f,
                            FrequencyPenalty = 0f,
                            PresencePenalty = 0f,
                            ResponseFormat = ChatResponseFormat.Text
                        },
                        cancellationToken
                    );

                // 4) Return the assistant’s reply
                var assistant = resp.Messages.FirstOrDefault(m => m.Role == ChatRole.Assistant);
                return assistant?.Text;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
    [McpServerToolType]
    public sealed class TailorBioTool
    {
        [McpServerTool(Name = "tailorBio"), Description("Tailor Bio")]
        public static async Task<string?> TailorBio(
            IMcpServer thisServer,
            [Description("Job Description")] string jd,
            [Description("Raw Bio")] string bio,
            [Description("Maximum number of tokens to generate")] int maxTokens = 2048,
            CancellationToken cancellationToken = default)
        {
            ChatMessage[] messages =
            [
                new(ChatRole.User, $"Output a LaTex snippet that will be added to an existing latex document - do not generate opening or closing article, document, sections, textbf or pargraph tags. Do not use LaTeX special charachter escaping. The user message is a biography: tailor/summarize it highlighting how it pertains the following job description, write three paragraphs and 6 bullet points (written with itemize, no dashes) - stick to what you know, don't make things up for the following job description:  {jd};;;; and bio:{bio} "),
            ];

            ChatOptions options = new()
            {
                MaxOutputTokens = maxTokens,
                Temperature = 1f,
                TopP = 1,
                FrequencyPenalty = 0,
                PresencePenalty = 0,

            };

            var resp = await thisServer.AsSamplingChatClient().GetResponseAsync(messages, options, cancellationToken);
            return resp.Messages.FirstOrDefault()?.Text;
        }
        }
    [McpServerToolType]
    public sealed class PercentMatchTool
    {
        [McpServerTool(Name = "percentMatch"), Description("Match JD Percent")]
        public static async Task<double> PercentMatch(
            IMcpServer thisServer,
            [Description("Job Description")] string jd,
            [Description("Position Description")] string position,
            [Description("Maximum number of tokens to generate")] int maxTokens = 2048,
            CancellationToken cancellationToken = default)
        {
            ChatMessage[] messages =
            [
                new(ChatRole.User,$"Indicate a percent match, only responding with a single value in \\\"%\\\", for the user message to the following job description: {jd};;; with the following position details {position}"),
            ];

            ChatOptions options = new()
            {
                MaxOutputTokens = maxTokens,
                Temperature = 1f,
                TopP = 1,
                FrequencyPenalty = 0,
                PresencePenalty = 0,

            };

            var resp = await thisServer.AsSamplingChatClient().GetResponseAsync(messages, options, cancellationToken);
            var s = resp.Messages.FirstOrDefault()?.Text;
            if (s == null)
                return 0;
            if (double.TryParse(s.Replace("%", ""), out double pct))
                return pct / 100;
            return 0;
        }
    }
        [McpServerToolType]
        public sealed class TailorPositionTool
        {
            [McpServerTool(Name = "tailorPosition"), Description("Tailor Position")]
        public static async Task<string?> TailorPosition(
            IMcpServer thisServer,
            [Description("Job Description")] string jd,
            [Description("Position Description")] string position,
            [Description("Bullet Count")] double bullets,
            [Description("Charchater Length")] int length,
            [Description("Maximum number of tokens to generate")] int maxTokens = 2048,
            CancellationToken cancellationToken = default)
        {
            ChatMessage[] messages =
            [
                new(ChatRole.User,$"Output a LaTex snippet, without special charachter escaping and the bullets properly itemized, that will be added to an existing latex document - do not generate opening or closing article, document sections or headers. Tailor user message - which is a description of a job experience, resulting in a total text length of no more than {length} characters, to the following job requirement sticking to the facts included in the user message, do not be creative IF A TECHNOLOGY IS NOT MENTIONED IN THE USER MESSAGE DO NOT INCLUDE IT IN THE SUMMARY!!!! include a short paragraph and {Math.Round(bullets)} bullet points which are LaTeX formated (written with itemize, no dashes - use itemize for bullets), do not bold anything or include any type of header - just the paragraph and bullets for the following job description: {jd};;;; targeting the follow position details {position}"),
            ];

            ChatOptions options = new()
            {
                MaxOutputTokens = maxTokens,
                Temperature = 1f,
                TopP = 1,
                FrequencyPenalty = 0,
                PresencePenalty = 0,

            };

            var resp = await thisServer.AsSamplingChatClient().GetResponseAsync(messages, options, cancellationToken);
            return resp.Messages.FirstOrDefault()?.Text;
        }
    }
    [McpServerToolType]
    public sealed class GenerateCoverLetterTool
    {
        [McpServerTool(Name = "generateCoverLetter"), Description("Generate Cover Letter")]
        public static async Task<string?> GenerateCoverLetter(
        IMcpServer thisServer,
        [Description("Job Description")] string jd,
        [Description("Resume")] string resume,
        [Description("Bullet Count")] double numberOfBullets,
        [Description("Charchater Length")] int targetLength,
        [Description("Maximum number of tokens to generate")] int maxTokens = 2048,
        CancellationToken cancellationToken = default)
        {
            ChatMessage[] messages =
            [
                new(ChatRole.User,$"Output a LaTex snippet that will be added to an existing latex document - do not generate opening or closing article, document sections or headers. Do not escape special characters. The user message is a latex resume: tailor/summarize it, to generate a cover letter, highlighting how it pertains the following job description, write up to {targetLength} characters and {numberOfBullets} bullet points (written with itemize, no dashes) - stick to what you know, don't make things up for the following job description: {jd};;; and the following resume (in LaTeX): {resume}"),
            ];

            ChatOptions options = new()
            {
                MaxOutputTokens = maxTokens,
                Temperature = 1f,
                TopP = 1,
                FrequencyPenalty = 0,
                PresencePenalty = 0,

            };

            var resp = await thisServer.AsSamplingChatClient().GetResponseAsync(messages, options, cancellationToken);
            return resp.Messages.FirstOrDefault()?.Text;
        }
    }
}
