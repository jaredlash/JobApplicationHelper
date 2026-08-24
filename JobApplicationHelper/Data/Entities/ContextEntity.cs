namespace JobApplicationHelper.Data.Entities;

public sealed class ContextEntity
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public ICollection<ExperienceContextEntity> Experiences { get; set; } = [];
}