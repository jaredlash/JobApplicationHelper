using JobApplicationHelper.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobApplicationHelper.Infrastructure.Persistence.Configurations;

public sealed class ExperienceLinkEntityConfiguration : IEntityTypeConfiguration<ExperienceLinkEntity>
{
    public void Configure(EntityTypeBuilder<ExperienceLinkEntity> builder)
    {
        builder.ToTable("ExperienceLinks");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .ValueGeneratedNever();

        builder.Property(e => e.Url)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(e => e.Description)
            .HasMaxLength(500);

        builder.Property(e => e.SortOrder)
            .IsRequired();

        builder.HasIndex(e => new
        {
            e.ExperienceId,
            e.SortOrder
        });
    }
}