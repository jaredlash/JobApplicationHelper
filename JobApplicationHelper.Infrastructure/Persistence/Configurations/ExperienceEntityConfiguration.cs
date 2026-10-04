using JobApplicationHelper.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobApplicationHelper.Infrastructure.Persistence.Configurations;

public sealed class ExperienceEntityConfiguration : IEntityTypeConfiguration<ExperienceEntity>
{
    public void Configure(EntityTypeBuilder<ExperienceEntity> builder)
    {
        builder.ToTable("Experiences");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.Title)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(e => e.Type)
            .HasConversion<string>()
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.Organization)
            .HasMaxLength(500);

        builder.Property(e => e.StartMonth);

        builder.Property(e => e.StartYear);

        builder.Property(e => e.EndMonth);

        builder.Property(e => e.EndYear);

        builder.Property(e => e.Summary)
            .IsRequired();

        builder.Property(e => e.Notes);

        builder.HasMany(e => e.Evidence)
            .WithOne(e => e.Experience)
            .HasForeignKey(e => e.ExperienceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.Links)
            .WithOne(e => e.Experience)
            .HasForeignKey(e => e.ExperienceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.Skills)
            .WithOne(e => e.Experience)
            .HasForeignKey(e => e.ExperienceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.Contexts)
            .WithOne(e => e.Experience)
            .HasForeignKey(e => e.ExperienceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => e.Type);
    }
}