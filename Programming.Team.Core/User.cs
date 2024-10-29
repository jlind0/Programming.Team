﻿using System;
using System.Collections.Generic;

namespace Programming.Team.Core;

public partial class User : Entity<Guid>
{
    public string ObjectId { get; set; } = null!;

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? EmailAddress { get; set; }

    public string? GitHubUrl { get; set; }

    public string? LinkedInUrl { get; set; }

    public string? PortfolioUrl { get; set; }

    public string? Bio { get; set; }

    public string? Title { get; set; }

    public string? PhoneNumber { get; set; }

    public string? City { get; set; }

    public string? State { get; set; }

    public string? Country { get; set; }

    public virtual ICollection<Certificate> CertificateCreatedByUsers { get; set; } = new List<Certificate>();

    public virtual ICollection<CertificateIssuer> CertificateIssuerCreatedByUsers { get; set; } = new List<CertificateIssuer>();

    public virtual ICollection<CertificateIssuer> CertificateIssuerUpdatedByUsers { get; set; } = new List<CertificateIssuer>();

    public virtual ICollection<Certificate> CertificateUpdatedByUsers { get; set; } = new List<Certificate>();

    public virtual ICollection<Certificate> CertificateUsers { get; set; } = new List<Certificate>();

    public virtual ICollection<Company> CompanyCreatedByUsers { get; set; } = new List<Company>();

    public virtual ICollection<Company> CompanyUpdatedByUsers { get; set; } = new List<Company>();

    public virtual ICollection<DocumentTemplate> DocumentTemplateCreatedByUsers { get; set; } = new List<DocumentTemplate>();

    public virtual ICollection<DocumentTemplate> DocumentTemplateUpdatedByUsers { get; set; } = new List<DocumentTemplate>();

    public virtual ICollection<DocumentType> DocumentTypeCreatedByUsers { get; set; } = new List<DocumentType>();

    public virtual ICollection<DocumentType> DocumentTypeUpdatedByUsers { get; set; } = new List<DocumentType>();

    public virtual ICollection<Education> EducationCreatedByUsers { get; set; } = new List<Education>();

    public virtual ICollection<Education> EducationUpdatedByUsers { get; set; } = new List<Education>();

    public virtual ICollection<Education> EducationUsers { get; set; } = new List<Education>();

    public virtual ICollection<Institution> InstitutionCreatedByUsers { get; set; } = new List<Institution>();

    public virtual ICollection<Institution> InstitutionUpdatedByUsers { get; set; } = new List<Institution>();

    public virtual ICollection<User> InverseCreatedByUser { get; set; } = new List<User>();

    public virtual ICollection<User> InverseUpdatedByUser { get; set; } = new List<User>();

    public virtual ICollection<Position> PositionCreatedByUsers { get; set; } = new List<Position>();

    public virtual ICollection<PositionSkill> PositionSkillCreatedByUsers { get; set; } = new List<PositionSkill>();

    public virtual ICollection<PositionSkill> PositionSkillUpdatedByUsers { get; set; } = new List<PositionSkill>();

    public virtual ICollection<Position> PositionUpdatedByUsers { get; set; } = new List<Position>();

    public virtual ICollection<Position> PositionUsers { get; set; } = new List<Position>();

    public virtual ICollection<Posting> PostingCreatedByUsers { get; set; } = new List<Posting>();

    public virtual ICollection<Posting> PostingUpdatedByUsers { get; set; } = new List<Posting>();

    public virtual ICollection<Posting> PostingUsers { get; set; } = new List<Posting>();

    public virtual ICollection<Role> RoleCreatedByUsers { get; set; } = new List<Role>();

    public virtual ICollection<Role> RoleUpdatedByUsers { get; set; } = new List<Role>();

    public virtual ICollection<Skill> SkillCreatedByUsers { get; set; } = new List<Skill>();

    public virtual ICollection<Skill> SkillUpdatedByUsers { get; set; } = new List<Skill>();

    public virtual ICollection<Role> Roles { get; set; } = new List<Role>();
}