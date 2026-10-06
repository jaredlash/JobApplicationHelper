using JobApplicationHelper.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace JobApplicationHelper.Infrastructure.Persistence;

public sealed class JobApplicationHelperDbContext(DbContextOptions<JobApplicationHelperDbContext> options)
    : DbContext(options)
{
    public DbSet<JobApplicationEntity> JobApplications => Set<JobApplicationEntity>();
    public DbSet<BackgroundJobEntity> BackgroundJobs => Set<BackgroundJobEntity>();
    public DbSet<ExtractedJobRequirementsEntity> ExtractedJobRequirements => Set<ExtractedJobRequirementsEntity>();
    public DbSet<CoverLetterDraftEntity> CoverLetterDrafts => Set<CoverLetterDraftEntity>();
    public DbSet<VerifyCoverLetterResultEntity> VerifyCoverLetterResults => Set<VerifyCoverLetterResultEntity>();

    public DbSet<ExperienceEntity> Experiences => Set<ExperienceEntity>();
    public DbSet<ExperienceEvidenceEntity> ExperienceEvidence => Set<ExperienceEvidenceEntity>();
    public DbSet<ExperienceLinkEntity> ExperienceLinks => Set<ExperienceLinkEntity>();
    public DbSet<ExperienceSkillEntity> ExperienceSkills => Set<ExperienceSkillEntity>();
    public DbSet<ExperienceContextEntity> ExperienceContexts => Set<ExperienceContextEntity>();
    public DbSet<SkillEntity> Skills => Set<SkillEntity>();
    public DbSet<ContextEntity> Contexts => Set<ContextEntity>();
    public DbSet<PendingExperienceImportEntity> PendingExperienceImports => Set<PendingExperienceImportEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasPostgresExtension("citext");

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(JobApplicationHelperDbContext).Assembly);
    }
}