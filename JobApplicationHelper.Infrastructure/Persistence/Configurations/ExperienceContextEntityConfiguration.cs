using JobApplicationHelper.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobApplicationHelper.Infrastructure.Persistence.Configurations;

public sealed class ExperienceContextEntityConfiguration : IEntityTypeConfiguration<ExperienceContextEntity>
{
    public void Configure(EntityTypeBuilder<ExperienceContextEntity> builder)
    {
        builder.ToTable("ExperienceContexts");

        builder.HasKey(e => new
        {
            e.ExperienceId,
            e.ContextId
        });

        builder.HasOne(e => e.Experience)
            .WithMany(e => e.Contexts)
            .HasForeignKey(e => e.ExperienceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Context)
            .WithMany(e => e.Experiences)
            .HasForeignKey(e => e.ContextId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}