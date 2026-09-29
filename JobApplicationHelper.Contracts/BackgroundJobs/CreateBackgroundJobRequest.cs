namespace JobApplicationHelper.Contracts.BackgroundJobs;

public sealed record CreateBackgroundJobRequest(string Type, string Priority);