namespace JobApplicationHelper.Data.Entities;

public sealed class SkillEntity
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public ICollection<ExperienceSkillEntity> Experiences { get; set; } = [];
}