namespace JobApplicationHelper.Infrastructure.Persistence.Entities;

public sealed class ExperienceSkillEntity
{
    public Guid ExperienceId { get; set; }

    public Guid SkillId { get; set; }

    public ExperienceEntity Experience { get; set; } = null!;

    public SkillEntity Skill { get; set; } = null!;
}