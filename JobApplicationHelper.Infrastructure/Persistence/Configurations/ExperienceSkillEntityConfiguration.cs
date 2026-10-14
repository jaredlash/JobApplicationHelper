using JobApplicationHelper.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobApplicationHelper.Infrastructure.Persistence.Configurations;

public sealed class ExperienceSkillEntityConfiguration : IEntityTypeConfiguration<ExperienceSkillEntity>
{
    public void Configure(EntityTypeBuilder<ExperienceSkillEntity> builder)
    {
        builder.ToTable("ExperienceSkills");

        builder.HasKey(e => new
        {
            e.ExperienceId,
            e.SkillId
        });

        builder.HasOne(e => e.Experience)
            .WithMany(e => e.Skills)
            .HasForeignKey(e => e.ExperienceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Skill)
            .WithMany(e => e.Experiences)
            .HasForeignKey(e => e.SkillId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}