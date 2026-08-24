namespace JobApplicationHelper.Data.Entities;

public sealed class ExperienceLinkEntity
{
    public int Id { get; set; }

    public string ExperienceId { get; set; } = string.Empty;

    public string Url { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    public string? Description { get; set; }

    public ExperienceEntity Experience { get; set; } = null!;
}