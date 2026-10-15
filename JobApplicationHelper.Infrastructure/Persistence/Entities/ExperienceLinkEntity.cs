namespace JobApplicationHelper.Infrastructure.Persistence.Entities;

public sealed class ExperienceLinkEntity
{
    public Guid Id { get; set; }

    public Guid ExperienceId { get; set; }

    public string Url { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public ExperienceEntity Experience { get; set; } = null!;
}