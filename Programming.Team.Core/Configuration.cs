﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Programming.Team.Core
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ResumePart
    {
        Bio,
        Recommendations,
        Positions,
        Skills,
        Education,
        Certifications,
        Publications
    }
    public interface ICoverLetterConfiguration
    {
        int? TargetLength { get; set; }
        Guid? DocumentTemplateId { get; set; }
        int? NumberOfBullets { get; set; }
    }
    public class CoverLetterConfiguration : ICoverLetterConfiguration
    {
        public int? TargetLength { get; set; } = 2000;
        public Guid? DocumentTemplateId { get; set; }
        public int? NumberOfBullets { get; set; } = 10;
    }
    public interface IResumeConfiguration
    {
        double? MatchThreshold { get; set; }
        int? TargetLengthPer10Percent { get; set; }
        bool HideSkillsNotInJD { get; set; }
        double? BulletsPer20Percent { get; set; }
        bool HidePositionsNotInJD { get; set; }
        ResumePart[] Parts { get; set; }
        Dictionary<ResumePart, Guid?> SectionTemplates { get; set; }
        double? SkillsPer20Percent { get; set; }
        Guid? DefaultDocumentTemplateId { get; set; }
        int? BioParagraphs { get; set; }
        int? BioBullets { get; set; }
        int? SummaryPageLength { get; set; }
    }
    public class ResumeConfiguration : IResumeConfiguration
    {
        public double? MatchThreshold { get; set; }
        public int? TargetLengthPer10Percent { get; set; }
        public bool HideSkillsNotInJD { get; set; } = true;
        public double? BulletsPer20Percent { get; set; }
        public bool HidePositionsNotInJD { get; set; } = false;
        public ResumePart[] Parts { get; set; } = [ResumePart.Bio, ResumePart.Recommendations, ResumePart.Skills, ResumePart.Positions, ResumePart.Education, ResumePart.Certifications, ResumePart.Publications];
        public Dictionary<ResumePart, Guid?> SectionTemplates { get; set; } = [];
        public Guid? DefaultDocumentTemplateId { get; set; }
        public double? SkillsPer20Percent { get; set; }
        public int? BioParagraphs { get; set; } = 3;
        public int? BioBullets { get; set; } = 6;
        public int? SummaryPageLength { get; set; } = 3;
    }
}
