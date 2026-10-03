namespace JobApplicationHelper.Contracts.BackgroundJobs;

public sealed record BackgroundJobStatusChanged(
    Guid BackgroundJobId,
    string Status,
    string? Error);