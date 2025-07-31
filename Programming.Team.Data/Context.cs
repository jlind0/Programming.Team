using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Programming.Team.Core;

namespace Programming.Team.Data;

public partial class ResumesContext : DbContext
{
    public ResumesContext()
    {
    }

    public ResumesContext(DbContextOptions<ResumesContext> options)
        : base(options)
    {
    }

    public virtual DbSet<CertificateIssuer> Certificates { get; set; }

    public virtual DbSet<CertificateIssuer> CertificateIssuers { get; set; }

    public virtual DbSet<Company> Companies { get; set; }

    public virtual DbSet<DocumentTemplate> DocumentTemplates { get; set; }

    public virtual DbSet<DocumentType> DocumentTypes { get; set; }

    public virtual DbSet<Education> Educations { get; set; }

    public virtual DbSet<Institution> Institutions { get; set; }
    public virtual DbSet<Package> Packages { get; set; }
    public virtual DbSet<Purchase> Purchases { get; set; }
    public virtual DbSet<Position> Positions { get; set; }

    public virtual DbSet<PositionSkill> PositionSkills { get; set; }

    public virtual DbSet<Posting> Postings { get; set; }

    public virtual DbSet<Skill> Skills { get; set; }

    public virtual DbSet<User> Users { get; set; }
    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Publication> Publications { get; set; }

    public virtual DbSet<Recommendation> Recommendations { get; set; }
    public virtual DbSet<SectionTemplate> SectionTemplates { get; set; }
    public virtual DbSet<DocumentSectionTemplate> DocumentSectionTemplates { get; set; }
    public virtual DbSet<DocumentTemplatePurchase> DocumentTemplatePurchases { get; set; }
    public virtual DbSet<EmailMessageTemplate> EmailMessageTemplates { get; set; }
    public virtual DbSet<FAQ> FAQs { get; set; }
    public virtual DbSet<Project> Projects { get; set; }
    public virtual DbSet<ProjectSkill> ProjectSkills { get; set; }
    public virtual DbSet<Page> Pages { get; set; }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Name=Resumes");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Certificate>(entity =>
        {
            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.TextTypeId).HasConversion<short>();
            entity.Property(e => e.CreateDate)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Name).HasMaxLength(500);
            entity.Property(e => e.UpdateDate)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Url).HasMaxLength(1000);

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.CertificateCreatedByUsers)
                .HasForeignKey(d => d.CreatedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Issuer).WithMany(p => p.Certificates)
                .HasForeignKey(d => d.IssuerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Certificates_CertificateIssuers");

            entity.HasOne(d => d.UpdatedByUser).WithMany(p => p.CertificateUpdatedByUsers)
                .HasForeignKey(d => d.UpdatedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.User).WithMany(p => p.CertificateUsers)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Certificates_Users");
             entity.HasQueryFilter(d => !d.IsDeleted);
            entity.Ignore(p => p.ValidFromDateString);
            entity.Ignore(p => p.ValidToDateString);
            entity.ToTable("Certificates");
        });

        modelBuilder.Entity<CertificateIssuer>(entity =>
        {
            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.TextTypeId).HasConversion<short>();
            entity.Property(e => e.CreateDate)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Name).HasMaxLength(500);
            entity.Property(e => e.UpdateDate)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Url).HasMaxLength(1000);

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.CertificateIssuerCreatedByUsers)
                .HasForeignKey(d => d.CreatedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.UpdatedByUser).WithMany(p => p.CertificateIssuerUpdatedByUsers)
                .HasForeignKey(d => d.UpdatedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull);
             entity.HasQueryFilter(d => !d.IsDeleted);
            entity.ToTable("CertificateIssuers");
        });

        modelBuilder.Entity<Company>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Table_1");
            entity.Property(e => e.TextTypeId).HasConversion<short>();
            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.City).HasMaxLength(500);
            entity.Property(e => e.Country).HasMaxLength(500);
            entity.Property(e => e.CreateDate)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Description).IsUnicode(false);
            entity.Property(e => e.Name)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.State).HasMaxLength(500);
            entity.Property(e => e.UpdateDate)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Url).HasMaxLength(1000);

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.CompanyCreatedByUsers)
                .HasForeignKey(d => d.CreatedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.UpdatedByUser).WithMany(p => p.CompanyUpdatedByUsers)
                .HasForeignKey(d => d.UpdatedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull);
             entity.HasQueryFilter(d => !d.IsDeleted);
        });

        modelBuilder.Entity<DocumentTemplate>(entity =>
        {
            entity.Property(e => e.DocumentTypeId).HasConversion<int>();
            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreateDate)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ApprovalStatus).HasConversion<short>();
            entity.Property(e => e.Name).HasMaxLength(1000);
            entity.Property(e => e.UpdateDate)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.DocumentTemplateCreatedByUsers)
                .HasForeignKey(d => d.CreatedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.DocumentType).WithMany(p => p.DocumentTemplates)
                .HasForeignKey(d => d.DocumentTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DocumentTemplates_DocumentTypes");

            entity.HasOne(d => d.UpdatedByUser).WithMany(p => p.DocumentTemplateUpdatedByUsers)
                .HasForeignKey(d => d.UpdatedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull);
             entity.HasQueryFilter(d => !d.IsDeleted);
            entity.HasMany(d => d.DocumentSectionTemplates).WithOne(p => p.DocumentTemplate)
                .HasForeignKey(d => d.DocumentTemplateId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DocumnetSectionTemplates_DocumentTemplates");
            entity.Property(e => e.Price).HasColumnType("money");
        });

        modelBuilder.Entity<DocumentType>(entity =>
        {
            entity.Property(e => e.Id).HasConversion<int>();
            entity.Property(e => e.CreateDate)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Name).HasMaxLength(500);
            entity.Property(e => e.UpdateDate)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.DocumentTypeCreatedByUsers)
                .HasForeignKey(d => d.CreatedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.UpdatedByUser).WithMany(p => p.DocumentTypeUpdatedByUsers)
                .HasForeignKey(d => d.UpdatedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull);

             entity.HasQueryFilter(d => !d.IsDeleted);
        });

        modelBuilder.Entity<Education>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Table_1_1");
            entity.Property(e => e.TextTypeId).HasConversion<short>();
            entity.ToTable("Education");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreateDate)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Major).HasMaxLength(1000);
            entity.Property(e => e.UpdateDate)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.EducationCreatedByUsers)
                .HasForeignKey(d => d.CreatedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Institution).WithMany(p => p.Educations)
                .HasForeignKey(d => d.InstitutionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Education_Institutions");

            entity.HasOne(d => d.UpdatedByUser).WithMany(p => p.EducationUpdatedByUsers)
                .HasForeignKey(d => d.UpdatedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.User).WithMany(p => p.EducationUsers)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Education_Users");
             entity.HasQueryFilter(d => !d.IsDeleted);
        });

        modelBuilder.Entity<Institution>(entity =>
        {
            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.TextTypeId).HasConversion<short>();
            entity.Property(e => e.City).HasMaxLength(500);
            entity.Property(e => e.Country).HasMaxLength(500);
            entity.Property(e => e.CreateDate)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Name).HasMaxLength(500);
            entity.Property(e => e.State).HasMaxLength(500);
            entity.Property(e => e.UpdateDate)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Url).HasMaxLength(1000);

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.InstitutionCreatedByUsers)
                .HasForeignKey(d => d.CreatedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.UpdatedByUser).WithMany(p => p.InstitutionUpdatedByUsers)
                .HasForeignKey(d => d.UpdatedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull);
             entity.HasQueryFilter(d => !d.IsDeleted);
        });

        modelBuilder.Entity<Position>(entity =>
        {
            entity.Ignore(e => e.Name);
            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.TextTypeId).HasConversion<short>();
            entity.Property(e => e.CreateDate)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.SortOrder)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.Title).HasMaxLength(500);
            entity.Property(e => e.UpdateDate)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Company).WithMany(p => p.Positions)
                .HasForeignKey(d => d.CompanyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Positions_Companies");
            entity.Navigation(d => d.Company).AutoInclude();
            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.PositionCreatedByUsers)
                .HasForeignKey(d => d.CreatedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.UpdatedByUser).WithMany(p => p.PositionUpdatedByUsers)
                .HasForeignKey(d => d.UpdatedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull);
            
            entity.HasOne(d => d.User).WithMany(p => p.PositionUsers)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Positions_Users");
             entity.HasQueryFilter(d => !d.IsDeleted);
        });
        modelBuilder.Entity<Project>(entity =>
        {
            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.TextTypeId).HasConversion<short>();
            entity.Property(e => e.CreateDate)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.SortOrder)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.UpdateDate)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.User).WithMany(p => p.Projects)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Projects_Users");

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.ProjectCreatedByUsers)
                .HasForeignKey(d => d.CreatedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.UpdatedByUser).WithMany(p => p.ProjectUpdatedByUsers)
                .HasForeignKey(d => d.UpdatedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Position).WithMany(p => p.Projects)
                .HasForeignKey(d => d.PositionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Projects_Positions");
            entity.HasQueryFilter(d => !d.IsDeleted);
        });
        modelBuilder.Entity<PositionSkill>(entity =>
        {
            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.TextTypeId).HasConversion<short>();
            entity.Property(e => e.CreateDate)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.UpdateDate)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.PositionSkillCreatedByUsers)
                .HasForeignKey(d => d.CreatedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Position).WithMany(p => p.PositionSkills)
                .HasForeignKey(d => d.PositionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PositionSkills_Positions");

            entity.HasOne(d => d.Skill).WithMany(p => p.PositionSkills)
                .HasForeignKey(d => d.SkillId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PositionSkills_Skills");

            entity.HasOne(d => d.UpdatedByUser).WithMany(p => p.PositionSkillUpdatedByUsers)
                .HasForeignKey(d => d.UpdatedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull);
             entity.HasQueryFilter(d => !d.IsDeleted);
        });
        modelBuilder.Entity<ProjectSkill>(entity =>
        {
            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.TextTypeId).HasConversion<short>();
            entity.Property(e => e.CreateDate)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.UpdateDate)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.ProjectSkillCreatedByUsers)
                .HasForeignKey(d => d.CreatedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Project).WithMany(p => p.ProjectSkills)
                .HasForeignKey(d => d.ProjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProjectSkills_Projects");

            entity.HasOne(d => d.Skill).WithMany(p => p.ProjectSkills)
                .HasForeignKey(d => d.SkillId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProjectSkills_Skills");

            entity.HasOne(d => d.UpdatedByUser).WithMany(p => p.ProjectSkillUpdatedByUsers)
                .HasForeignKey(d => d.UpdatedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull);
            entity.HasQueryFilter(d => !d.IsDeleted);
        });
        modelBuilder.Entity<Posting>(entity =>
        {
            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreateDate)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Name).HasMaxLength(1000);
            entity.Property(e => e.UpdateDate)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.DocumentTemplate).WithMany(p => p.Postings)
                .HasForeignKey(d => d.DocumentTemplateId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Postings_DocumentTemplates");

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.PostingCreatedByUsers)
                .HasForeignKey(d => d.CreatedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.UpdatedByUser).WithMany(p => p.PostingUpdatedByUsers)
                .HasForeignKey(d => d.UpdatedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.User).WithMany(p => p.PostingUsers)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasQueryFilter(d => !d.IsDeleted);
        });

        modelBuilder.Entity<Skill>(entity =>
        {
            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreateDate)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Name)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.UpdateDate)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.SkillCreatedByUsers)
                .HasForeignKey(d => d.CreatedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.UpdatedByUser).WithMany(p => p.SkillUpdatedByUsers)
                .HasForeignKey(d => d.UpdatedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull);
             entity.HasQueryFilter(d => !d.IsDeleted);
        });
        modelBuilder.Entity<Page>(entity =>
        {
            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreateDate)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Name)
                .HasMaxLength(100);
            entity.Property(e => e.UpdateDate)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.PageCreatedByUsers)
                .HasForeignKey(d => d.CreatedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.UpdatedByUser).WithMany(p => p.PageUpdatedByUsers)
                .HasForeignKey(d => d.UpdatedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull);
            entity.HasQueryFilter(d => !d.IsDeleted);
        });
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.ObjectId, "IX_Users").IsUnique();
            
            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.City).HasMaxLength(500);
            entity.Property(e => e.Country).HasMaxLength(500);
            entity.Property(e => e.CreateDate)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.EmailAddress).HasMaxLength(500);
            entity.Property(e => e.FirstName).HasMaxLength(500);
            entity.Property(e => e.GitHubUrl).HasMaxLength(2000);
            entity.Property(e => e.LastName).HasMaxLength(500);
            entity.Property(e => e.LinkedInUrl).HasMaxLength(2000);
            entity.Property(e => e.ObjectId).HasMaxLength(50);
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PortfolioUrl).HasMaxLength(2000);
            entity.Property(e => e.State).HasMaxLength(500);
            entity.Property(e => e.Title).HasMaxLength(100);
            entity.Property(e => e.ResumeGenerationsLeft).HasDefaultValue(15);
            entity.Property(e => e.UpdateDate)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.InverseCreatedByUser).HasForeignKey(d => d.CreatedByUserId);
            entity.HasOne(d => d.UpdatedByUser).WithMany(p => p.InverseUpdatedByUser).HasForeignKey(d => d.UpdatedByUserId);
            entity.HasQueryFilter(d => !d.IsDeleted);
            entity.HasMany(d => d.Recruiters).WithMany(p => p.Recruits)
                .UsingEntity<Dictionary<string, object>>(
                    "RecruiterUsers",
                    r => r.HasOne<User>().WithMany()
                        .HasForeignKey("RecruiterUserId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_RecruiterUsers_Users"),
                    l => l.HasOne<User>().WithMany()
                        .HasForeignKey("TargetUserId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_RecruiterUsers_Users1"),
                    j =>
                    {
                        j.HasKey("RecruiterUserId", "TargetUserId");
                        j.ToTable("RecruiterUsers");
                    });
        });
        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasIndex(e => e.Name, "IX_Roles").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreateDate).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.UpdateDate).HasDefaultValueSql("(getutcdate())");

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.RoleCreatedByUsers)
                .HasForeignKey(d => d.CreatedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Roles_Users");

            entity.HasOne(d => d.UpdatedByUser).WithMany(p => p.RoleUpdatedByUsers)
                .HasForeignKey(d => d.UpdatedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Roles_Users1");

            entity.HasMany(d => d.Users).WithMany(p => p.Roles)
                .UsingEntity<Dictionary<string, object>>(
                    "RolesUser",
                    r => r.HasOne<User>().WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_RolesUsers_Users"),
                    l => l.HasOne<Role>().WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_RolesUsers_Roles"),
                    j =>
                    {
                        j.HasKey("RoleId", "UserId");
                        j.ToTable("RolesUsers");
                    });
            entity.HasQueryFilter(d => !d.IsDeleted);
        });
        modelBuilder.Entity<Publication>(entity =>
        {
            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.TextTypeId).HasConversion<short>();
            entity.Property(e => e.CreateDate)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Title).HasMaxLength(500);
            entity.Property(e => e.UpdateDate)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Url).HasMaxLength(1000);

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.PublicationCreatedByUsers)
                .HasForeignKey(d => d.CreatedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Publications_Users1");

            entity.HasOne(d => d.UpdatedByUser).WithMany(p => p.PublicationUpdatedByUsers)
                .HasForeignKey(d => d.UpdatedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Publications_Users2");

            entity.HasOne(d => d.User).WithMany(p => p.PublicationUsers)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Publications_Users");
            entity.HasQueryFilter(d => !d.IsDeleted);
        });

        modelBuilder.Entity<Recommendation>(entity =>
        {
            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.TextTypeId).HasConversion<short>();
            entity.Property(e => e.CreateDate)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Name).HasMaxLength(1000);
            entity.Property(e => e.Body);
            entity.Property(e => e.SortOrder)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.UpdateDate)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.RecommendationCreatedByUsers)
                .HasForeignKey(d => d.CreatedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Reccomendations_Users1");

            entity.HasOne(d => d.Position).WithMany(p => p.Recommendations)
                .HasForeignKey(d => d.PositionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Reccomendations_Positions");

            entity.HasOne(d => d.UpdatedByUser).WithMany(p => p.RecommendationUpdatedByUsers)
                .HasForeignKey(d => d.UpdatedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Reccomendations_Users2");

            entity.HasOne(d => d.User).WithMany(p => p.RecommendationUsers)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Reccomendations_Users");
            entity.HasQueryFilter(d => !d.IsDeleted);
        });
        modelBuilder.Entity<Package>(entity =>
        {
            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreateDate)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Price).HasColumnType("money");
            entity.Property(e => e.StripePriceId).HasMaxLength(1000);
            entity.Property(e => e.StripeProductId).HasMaxLength(1000);
            entity.Property(e => e.StripeUrl).HasMaxLength(1000);
            entity.Property(e => e.UpdateDate)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.PackageCreatedByUsers)
                .HasForeignKey(d => d.CreatedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Packages_Users");

            entity.HasOne(d => d.UpdatedByUser).WithMany(p => p.PackageUpdatedByUsers)
                .HasForeignKey(d => d.UpdatedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Packages_Users1");
            entity.HasQueryFilter(d => !d.IsDeleted);
        });
        modelBuilder.Entity<Purchase>(entity =>
        {
            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreateDate).HasColumnType("datetime");
            entity.Property(e => e.PricePaid).HasColumnType("money");
            entity.Property(e => e.StripeSessionUrl).HasMaxLength(1000);
            entity.Property(e => e.UpdateDate).HasColumnType("datetime");

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.PurchaseCreatedByUsers)
                .HasForeignKey(d => d.CreatedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Purchases_Users1");

            entity.HasOne(d => d.Package).WithMany(p => p.Purchases)
                .HasForeignKey(d => d.PackageId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Purchases_Packages");

            entity.HasOne(d => d.UpdatedByUser).WithMany(p => p.PurchaseUpdatedByUsers)
                .HasForeignKey(d => d.UpdatedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Purchases_Users2");

            entity.HasOne(d => d.User).WithMany(p => p.PurchaseUsers)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Purchases_Users");
            entity.HasQueryFilter(d => !d.IsDeleted);
        });
        modelBuilder.Entity<SectionTemplate>(entity =>
        {
            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.SectionId).HasConversion<int>();
            entity.Property(e => e.CreateDate).HasColumnType("datetime");
            entity.Property(e => e.UpdateDate).HasColumnType("datetime");
            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.SectionTemplateCreatedByUsers)
                .HasForeignKey(d => d.CreatedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SectionTemplates_Users");
            entity.HasOne(d => d.UpdatedByUser).WithMany(p => p.SectionTemplateUpdatedByUsers)
                .HasForeignKey(d => d.UpdatedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SectionTemplates_Users1");
            entity.HasQueryFilter(d => !d.IsDeleted);
            entity.HasMany(d => d.DocumentSectionTemplates).WithOne(p => p.SectionTemplate)
                .HasForeignKey(d => d.SectionTemplateId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DocumnetSectionTemplates_SectionTemplates");
            entity.HasOne(d => d.Owner).WithMany(p => p.SectionTemplates)
                .HasForeignKey(d => d.OwnerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SectionTemplates_Users2");

        });
        modelBuilder.Entity<DocumentSectionTemplate>(entity =>
        {
            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreateDate).HasColumnType("datetime");
            entity.Property(e => e.UpdateDate).HasColumnType("datetime");
            entity.HasOne(d => d.DocumentTemplate).WithMany(p => p.DocumentSectionTemplates)
                    .HasForeignKey(d => d.DocumentTemplateId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_DocumnetSectionTemplates_DocumentTemplates");
            entity.HasOne(d => d.SectionTemplate).WithMany(p => p.DocumentSectionTemplates)
                .HasForeignKey(d => d.SectionTemplateId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DocumnetSectionTemplates_SectionTemplates");
            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.DocumentSectionTemplateCreatedByUsers)
                .HasForeignKey(d => d.CreatedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DocumnetSectionTemplates_Users");
            entity.HasOne(d => d.UpdatedByUser).WithMany(p => p.DocumentSectionTemplateUpdatedByUsers)
                .HasForeignKey(d => d.UpdatedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DocumnetSectionTemplates_Users1");
            entity.HasQueryFilter(d => !d.IsDeleted);
        });
        modelBuilder.Entity<DocumentTemplatePurchase>(entity =>
        {
            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreateDate).HasColumnType("datetime");
            entity.Property(e => e.UpdateDate).HasColumnType("datetime");
            entity.Property(e => e.PricePaid).HasColumnType("money");
            entity.HasOne(d => d.DocumentTemplate).WithMany(p => p.TemplatePurchases)
                    .HasForeignKey(d => d.DocumentTemplateId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_DocumentTemplatePurchases_DocumentTemplates");
            entity.HasOne(d => d.User).WithMany(p => p.DocumentTemplatePurchases)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DocumentTemplatePurchases_Users");
            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.DocumentTemplatePurchaseCreatedByUsers)
                .HasForeignKey(d => d.CreatedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DocumentTemplatePurchases_Users1");
            entity.HasOne(d => d.UpdatedByUser).WithMany(p => p.DocumentTemplatePurchaseUpdatedByUsers)
                .HasForeignKey(d => d.UpdatedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DocumentTemplatePurchases_Users2");
            entity.HasQueryFilter(d => !d.IsDeleted);
        });
        modelBuilder.Entity<EmailMessageTemplate>(entity =>
        {
            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreateDate)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Name).HasMaxLength(500);
            entity.Property(e => e.UpdateDate)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.EmailMessageTemplateCreatedByUsers)
                .HasForeignKey(d => d.CreatedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.UpdatedByUser).WithMany(p => p.EmailMessageTemplateUpdatedByUsers)
                .HasForeignKey(d => d.UpdatedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull);
            entity.HasQueryFilter(d => !d.IsDeleted);
            entity.ToTable("EmailMessageTemplates");
        });
        modelBuilder.Entity<FAQ>(entity =>
        {
            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Title).HasMaxLength(100);
            entity.Property(e => e.CreateDate)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.UpdateDate)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.FAQCreatedByUsers)
                .HasForeignKey(d => d.CreatedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.UpdatedByUser).WithMany(p => p.FAQUpdatedByUsers)
                .HasForeignKey(d => d.UpdatedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull);
            entity.HasQueryFilter(d => !d.IsDeleted);
            entity.ToTable("FAQs");
        });
        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
