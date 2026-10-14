namespace JobApplicationHelper.Application.Services;

public sealed record ExperienceImportResult(int AddedCount, int SkippedCount, int PendingCount);