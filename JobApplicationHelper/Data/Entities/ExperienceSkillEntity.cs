namespace JobApplicationHelper.Data.Entities;

public sealed class ExperienceSkillEntity
{
    public string ExperienceId { get; set; } = string.Empty;

    public int SkillId { get; set; }

    public ExperienceEntity Experience { get; set; } = null!;

    public SkillEntity Skill { get; set; } = null!;
}