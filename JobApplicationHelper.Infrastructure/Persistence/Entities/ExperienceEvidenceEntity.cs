namespace JobApplicationHelper.Infrastructure.Persistence.Entities;

public sealed class ExperienceEvidenceEntity
{
    public Guid Id { get; set; }

    public Guid ExperienceId { get; set; }

    public int SortOrder { get; set; }

    public string Text { get; set; } = string.Empty;

    public ExperienceEntity Experience { get; set; } = null!;
}
