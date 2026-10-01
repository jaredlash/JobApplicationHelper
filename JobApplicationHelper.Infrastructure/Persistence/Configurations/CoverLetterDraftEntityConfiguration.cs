using JobApplicationHelper.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobApplicationHelper.Infrastructure.Persistence.Configurations;

public sealed class CoverLetterDraftEntityConfiguration
    : IEntityTypeConfiguration<CoverLetterDraftEntity>
{
    public void Configure(EntityTypeBuilder<CoverLetterDraftEntity> builder)
    {
        builder.ToTable("CoverLetterDrafts");

        builder.HasKey(x => x.JobApplicationId);

        builder.Property(x => x.JobApplicationId)
            .ValueGeneratedNever();

        builder.Property(x => x.Draft)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.HasOne<JobApplicationEntity>()
            .WithOne()
            .HasForeignKey<CoverLetterDraftEntity>(
                x => x.JobApplicationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}