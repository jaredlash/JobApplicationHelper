namespace JobApplicationHelper.Contracts.Experiences;

public sealed record ImportExperiencesResponse(int AddedCount, int SkippedCount, int PendingCount);