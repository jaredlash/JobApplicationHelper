using JobApplicationHelper.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobApplicationHelper.Infrastructure.Persistence.Configurations;

public sealed class VerifyCoverLetterResultEntityConfiguration
    : IEntityTypeConfiguration<VerifyCoverLetterResultEntity>
{
    public void Configure(EntityTypeBuilder<VerifyCoverLetterResultEntity> builder)
    {
        builder.ToTable("VerifyCoverLetterResults");

        builder.HasKey(x => x.JobApplicationId);

        builder.Property(x => x.JobApplicationId)
            .ValueGeneratedNever();

        builder.Property(x => x.VerificationResultJson)
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.HasOne<JobApplicationEntity>()
            .WithOne()
            .HasForeignKey<VerifyCoverLetterResultEntity>(
                x => x.JobApplicationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
