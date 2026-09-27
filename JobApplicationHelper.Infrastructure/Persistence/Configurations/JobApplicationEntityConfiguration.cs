using JobApplicationHelper.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobApplicationHelper.Infrastructure.Persistence.Configurations;

public sealed class JobApplicationEntityConfiguration : IEntityTypeConfiguration<JobApplicationEntity>
{
    public void Configure(EntityTypeBuilder<JobApplicationEntity> builder)
    {
        builder.ToTable("JobApplications");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.CountryCode)
            .HasMaxLength(2)
            .IsRequired();

        builder.Property(x => x.CompanyName)
            .IsRequired();

        builder.Property(x => x.PositionTitle)
            .IsRequired();

        builder.Property(x => x.URL)
            .IsRequired();

        builder.Property(x => x.JobPosting)
            .IsRequired();

        builder.Property(x => x.ApplicationFolder)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();
    }
}