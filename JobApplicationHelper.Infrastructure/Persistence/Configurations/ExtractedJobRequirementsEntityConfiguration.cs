using JobApplicationHelper.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobApplicationHelper.Infrastructure.Persistence.Configurations;

public sealed class ExtractedJobRequirementsEntityConfiguration
    : IEntityTypeConfiguration<ExtractedJobRequirementsEntity>
{
    public void Configure(EntityTypeBuilder<ExtractedJobRequirementsEntity> builder)
    {
        builder.ToTable("ExtractedJobRequirements");

        builder.HasKey(x => x.JobApplicationId);

        builder.Property(x => x.JobApplicationId)
            .ValueGeneratedNever();

        builder.Property(x => x.RequirementsJson)
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.HasOne<JobApplicationEntity>()
            .WithOne()
            .HasForeignKey<ExtractedJobRequirementsEntity>(
                x => x.JobApplicationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
