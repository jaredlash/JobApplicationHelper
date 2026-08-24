using JobApplicationHelper.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace JobApplicationHelper.Data;

public sealed class ExperienceBankDbContext : DbContext
{
    public ExperienceBankDbContext(DbContextOptions<ExperienceBankDbContext> options)
        : base(options)
    {
    }

    public DbSet<ExperienceEntity> Experiences => Set<ExperienceEntity>();

    public DbSet<ExperienceEvidenceEntity> ExperienceEvidence => Set<ExperienceEvidenceEntity>();

    public DbSet<ExperienceLinkEntity> ExperienceLinks => Set<ExperienceLinkEntity>();

    public DbSet<ExperienceSkillEntity> ExperienceSkills => Set<ExperienceSkillEntity>();

    public DbSet<ExperienceContextEntity> ExperienceContexts => Set<ExperienceContextEntity>();

    public DbSet<SkillEntity> Skills => Set<SkillEntity>();

    public DbSet<ContextEntity> Contexts => Set<ContextEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureExperience(modelBuilder);
        ConfigureExperienceEvidence(modelBuilder);
        ConfigureExperienceLink(modelBuilder);
        ConfigureSkill(modelBuilder);
        ConfigureContext(modelBuilder);
        ConfigureExperienceSkill(modelBuilder);
        ConfigureExperienceContext(modelBuilder);
    }

    private static void ConfigureExperience(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<ExperienceEntity>();

        entity.HasKey(e => e.Id);

        entity.Property(e => e.Id)
            .HasMaxLength(200)
            .IsRequired();

        entity.Property(e => e.Title)
            .HasMaxLength(500)
            .IsRequired();

        entity.Property(e => e.Type)
            .HasConversion<string>()
            .HasMaxLength(100)
            .IsRequired();

        entity.Property(e => e.Organization)
            .HasMaxLength(500);

        entity.Property(e => e.StartMonth);

        entity.Property(e => e.StartYear);

        entity.Property(e => e.EndMonth);

        entity.Property(e => e.EndYear);

        entity.Property(e => e.Summary)
            .IsRequired();

        entity.Property(e => e.Notes);

        entity.HasMany(e => e.Evidence)
            .WithOne(e => e.Experience)
            .HasForeignKey(e => e.ExperienceId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasMany(e => e.Links)
            .WithOne(e => e.Experience)
            .HasForeignKey(e => e.ExperienceId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasMany(e => e.Skills)
            .WithOne(e => e.Experience)
            .HasForeignKey(e => e.ExperienceId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasMany(e => e.Contexts)
            .WithOne(e => e.Experience)
            .HasForeignKey(e => e.ExperienceId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasIndex(e => e.Type);
    }

    private static void ConfigureExperienceEvidence(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<ExperienceEvidenceEntity>();

        entity.HasKey(e => e.Id);

        entity.Property(e => e.Text)
            .IsRequired();

        entity.Property(e => e.SortOrder)
            .IsRequired();

        entity.HasIndex(e => new
        {
            e.ExperienceId,
            e.SortOrder
        });
    }

    private static void ConfigureExperienceLink(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<ExperienceLinkEntity>();

        entity.HasKey(e => e.Id);

        entity.Property(e => e.Url)
            .HasMaxLength(2000)
            .IsRequired();

        entity.Property(e => e.Description)
            .HasMaxLength(500);

        entity.Property(e => e.SortOrder)
            .IsRequired();

        entity.HasIndex(e => new
        {
            e.ExperienceId,
            e.SortOrder
        });
    }

    private static void ConfigureSkill(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<SkillEntity>();

        entity.HasKey(e => e.Id);

        entity.Property(e => e.Name)
            .HasMaxLength(200)
            .IsRequired()
            .UseCollation("NOCASE");

        entity.HasIndex(e => e.Name)
            .IsUnique();
    }

    private static void ConfigureContext(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<ContextEntity>();

        entity.HasKey(e => e.Id);

        entity.Property(e => e.Name)
            .HasMaxLength(300)
            .IsRequired()
            .UseCollation("NOCASE");

        entity.HasIndex(e => e.Name)
            .IsUnique();
    }

    private static void ConfigureExperienceSkill(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<ExperienceSkillEntity>();

        entity.HasKey(e => new
        {
            e.ExperienceId,
            e.SkillId
        });

        entity.HasOne(e => e.Experience)
            .WithMany(e => e.Skills)
            .HasForeignKey(e => e.ExperienceId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasOne(e => e.Skill)
            .WithMany(e => e.Experiences)
            .HasForeignKey(e => e.SkillId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private static void ConfigureExperienceContext(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<ExperienceContextEntity>();

        entity.HasKey(e => new
        {
            e.ExperienceId,
            e.ContextId
        });

        entity.HasOne(e => e.Experience)
            .WithMany(e => e.Contexts)
            .HasForeignKey(e => e.ExperienceId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasOne(e => e.Context)
            .WithMany(e => e.Experiences)
            .HasForeignKey(e => e.ContextId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}