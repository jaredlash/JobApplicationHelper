namespace JobApplicationHelper.Infrastructure.Persistence.Entities;

public sealed class ExperienceContextEntity
{
    public Guid ExperienceId { get; set; }

    public Guid ContextId { get; set; }

    public ExperienceEntity Experience { get; set; } = null!;

    public ContextEntity Context { get; set; } = null!;
}