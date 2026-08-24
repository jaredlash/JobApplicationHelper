namespace JobApplicationHelper.Data.Entities;

public sealed class ExperienceEvidenceEntity
{
    public int Id { get; set; }

    public string ExperienceId { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    public string Text { get; set; } = string.Empty;

    public ExperienceEntity Experience { get; set; } = null!;
}
