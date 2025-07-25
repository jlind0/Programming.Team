﻿using System;
using System.Collections.Generic;

namespace Programming.Team.Core;

public interface IStripePayable
{
    string? StripeAccountId { get; set; }
    string? StripeStatus { get; set; }
    DateTime? StripeUpdateDate { get; set; }
    
}
public interface IUser : IEntity<Guid>, IStripePayable
{
    string ObjectId { get; set; }
    string? FirstName { get; set; }
    string? LastName { get; set; }

    string? EmailAddress { get; set; }

    string? GitHubUrl { get; set; }

    string? LinkedInUrl { get; set; }

    string? PortfolioUrl { get; set; }

    string? Bio { get; set; }

    string? Title { get; set; }

    string? PhoneNumber { get; set; }

    string? City { get; set; }

    string? State { get; set; }

    string? Country { get; set; }
    int ResumeGenerationsLeft { get; set; }
    string? DefaultResumeConfiguration { get; set; }
    string? DefaultCoverLetterConfiguration { get; set; }

}

public partial class User : Entity<Guid>, IUser
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
    public virtual ICollection<Page> PageCreatedByUsers { get; set; } = new List<Page>();

    public virtual ICollection<Page> PageUpdatedByUsers { get; set; } = new List<Page>();

    public virtual ICollection<Role> Roles { get; set; } = new List<Role>();
    public virtual ICollection<Publication> PublicationCreatedByUsers { get; set; } = new List<Publication>();

    public virtual ICollection<Publication> PublicationUpdatedByUsers { get; set; } = new List<Publication>();

    public virtual ICollection<Publication> PublicationUsers { get; set; } = new List<Publication>();

    public virtual ICollection<Recommendation> RecommendationCreatedByUsers { get; set; } = new List<Recommendation>();

    public virtual ICollection<Recommendation> RecommendationUpdatedByUsers { get; set; } = new List<Recommendation>();

    public virtual ICollection<Recommendation> RecommendationUsers { get; set; } = new List<Recommendation>();
    public virtual ICollection<User> Recruiters { get; set; } = new List<User>();
    public virtual ICollection<User> Recruits { get; set; } = new List<User>();
    public int ResumeGenerationsLeft { get; set; } = 15;
    public virtual ICollection<Package> PackageCreatedByUsers { get; set; } = new List<Package>();

    public virtual ICollection<Package> PackageUpdatedByUsers { get; set; } = new List<Package>();
    public virtual ICollection<Purchase> PurchaseCreatedByUsers { get; set; } = new List<Purchase>();

    public virtual ICollection<Purchase> PurchaseUpdatedByUsers { get; set; } = new List<Purchase>();

    public virtual ICollection<Purchase> PurchaseUsers { get; set; } = new List<Purchase>();
    public virtual ICollection<SectionTemplate> SectionTemplateCreatedByUsers { get; set; } = new List<SectionTemplate>();
    public virtual ICollection<SectionTemplate> SectionTemplateUpdatedByUsers { get; set; } = new List<SectionTemplate>();
    public string? DefaultResumeConfiguration { get; set; }
    public virtual ICollection<DocumentSectionTemplate> DocumentSectionTemplateCreatedByUsers { get; set; } = new List<DocumentSectionTemplate>();
    public virtual ICollection<DocumentSectionTemplate> DocumentSectionTemplateUpdatedByUsers { get; set; } = new List<DocumentSectionTemplate>();
    public virtual ICollection<EmailMessageTemplate> EmailMessageTemplateCreatedByUsers { get; set; } = new List<EmailMessageTemplate>();
    public virtual ICollection<EmailMessageTemplate> EmailMessageTemplateUpdatedByUsers { get; set; } = new List<EmailMessageTemplate>();
    public virtual ICollection<DocumentTemplatePurchase> DocumentTemplatePurchaseCreatedByUsers { get; set; } = new List<DocumentTemplatePurchase>();
    public virtual ICollection<DocumentTemplatePurchase> DocumentTemplatePurchaseUpdatedByUsers { get; set; } = new List<DocumentTemplatePurchase>();
    public virtual ICollection<DocumentTemplatePurchase> DocumentTemplatePurchases { get; set; } = new List<DocumentTemplatePurchase>();
    public virtual ICollection<FAQ> FAQUpdatedByUsers { get; set; } = new List<FAQ>();
    public virtual ICollection<FAQ> FAQCreatedByUsers { get; set; } = new List<FAQ>();
    public virtual ICollection<SectionTemplate> SectionTemplates { get; set; } = new List<SectionTemplate>();
    public string? StripeAccountId { get; set; }
    public string? StripeStatus { get; set; }
    public DateTime? StripeUpdateDate { get; set; }
    public string? DefaultCoverLetterConfiguration { get; set; }
    public virtual ICollection<Project> Projects { get; set; } = [];
    public virtual ICollection<Project> ProjectCreatedByUsers { get; set; } = [];
    public virtual ICollection<Project> ProjectUpdatedByUsers { get; set; } = [];
    public virtual ICollection<ProjectSkill> ProjectSkillCreatedByUsers { get; set; } = [];

    public virtual ICollection<ProjectSkill> ProjectSkillUpdatedByUsers { get; set; } = [];
}
