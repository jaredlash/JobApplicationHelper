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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(JobApplicationHelperDbContext).Assembly);
    }
}