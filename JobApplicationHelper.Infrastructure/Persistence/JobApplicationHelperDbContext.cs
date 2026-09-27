using JobApplicationHelper.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace JobApplicationHelper.Infrastructure.Persistence;

public sealed class JobApplicationHelperDbContext(DbContextOptions<JobApplicationHelperDbContext> options)
    : DbContext(options)
{
    public DbSet<JobApplicationEntity> JobApplications => Set<JobApplicationEntity>();


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(JobApplicationHelperDbContext).Assembly);
    }
}