using JobApplicationHelper.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobApplicationHelper.Infrastructure.Persistence.Configurations;

public sealed class PendingExperienceImportConfiguration
    : IEntityTypeConfiguration<PendingExperienceImportEntity>
{
    public void Configure(EntityTypeBuilder<PendingExperienceImportEntity> builder)
    {
        builder.HasKey(x => x.ExperienceId);

        builder.Property(x => x.ExperienceId)
            .ValueGeneratedNever();

        builder.Property(x => x.SerializedExperience)
            .IsRequired();

        builder.HasOne<ExperienceEntity>()
            .WithOne()
            .HasForeignKey<PendingExperienceImportEntity>(x => x.ExperienceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}