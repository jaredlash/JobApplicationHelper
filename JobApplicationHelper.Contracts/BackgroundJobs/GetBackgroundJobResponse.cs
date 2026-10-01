namespace JobApplicationHelper.Contracts.BackgroundJobs;

public sealed record GetBackgroundJobResponse(
    Guid Id,
    string Type,
    string Priority,
    string Status,
    DateTime CreatedAt,
    DateTime? StartedAt,
    DateTime? CompletedAt,
    string? Error);