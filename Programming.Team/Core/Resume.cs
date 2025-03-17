using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Programming.Team.Core
{
    public class Resume
    {
        public User User { get; set; } = null!;
        public List<Position> Positions { get; set; } = new List<Position>();
        public List<Education> Educations { get; set; } = new List<Education>();
        public List<Reccomendation> Reccomendations { get; set; } = new List<Reccomendation>();
        public List<Certificate> Certificates { get; set; } = new List<Certificate>();
        public List<SkillRollup> Skills { get; set; } = new List<SkillRollup>();
        public List<Publication> Publications { get; set; } = new List<Publication>();
        public ResumePart[] Parts { get; set; } = [ResumePart.Bio, ResumePart.Reccomendations, ResumePart.Skills, ResumePart.Positions, ResumePart.Education, ResumePart.Certifications, ResumePart.Publications];
    }
    public class SkillRollup
    {
        public double YearsOfExperience { get; set; }
        public List<Position> Positions { get; set; } = new List<Position>();
        public Skill Skill { get; set; } = null!;
    }
}
