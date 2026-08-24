namespace JobApplicationHelper.Data.Entities;

public sealed class ExperienceContextEntity
{
    public string ExperienceId { get; set; } = string.Empty;

    public int ContextId { get; set; }

    public ExperienceEntity Experience { get; set; } = null!;

    public ContextEntity Context { get; set; } = null!;
}