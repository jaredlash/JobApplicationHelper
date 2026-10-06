namespace JobApplicationHelper.Infrastructure.Persistence.Entities;

public sealed class PendingExperienceImportEntity()
{
    public Guid ExperienceId { get; set; } = default;

    public string SerializedExperience { get; set;  } = string.Empty;
}