namespace JobApplicationHelper.Contracts.Experiences;

public sealed record PendingExperienceImportDto(ExperienceDto Current, ExperienceDto Pending);