using JobApplicationHelper.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobApplicationHelper.Infrastructure.Persistence.Configurations;

public sealed class ExperienceEvidenceEntityConfiguration : IEntityTypeConfiguration<ExperienceEvidenceEntity>
{
    public void Configure(EntityTypeBuilder<ExperienceEvidenceEntity> builder)
    {
        builder.ToTable("ExperienceEvidence");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .ValueGeneratedNever();

        builder.Property(e => e.Text)
            .IsRequired();

        builder.Property(e => e.SortOrder)
            .IsRequired();

        builder.HasIndex(e => new
        {
            e.ExperienceId,
            e.SortOrder
        });
    }
}