namespace JobApplicationHelper.Domain.Models;

public sealed class BackgroundJob
{
    public required BackgroundJobId Id { get; init; }

    public required BackgroundJobType Type { get; init; }

    public BackgroundJobPriority Priority { get; init; }

    public BackgroundJobStatus Status { get; private set; } = BackgroundJobStatus.Pending;

    public DateTime CreatedAt { get; init; }

    public DateTime? StartedAt { get; private set; }

    public DateTime? CompletedAt { get; private set; }

    public string? Error { get; private set; }

    public required JobApplicationId JobApplicationId { get; init; }

    public string? Payload { get; init; }

    public void Start()
    {
        if (Status != BackgroundJobStatus.Pending)
        {
            throw new InvalidOperationException($"Cannot start a background job with status '{Status}'.");
        }

        Status = BackgroundJobStatus.Running;
        StartedAt = DateTime.UtcNow;
    }

    public void Complete()
    {
        if (Status != BackgroundJobStatus.Running)
        {
            throw new InvalidOperationException($"Cannot complete a background job with status '{Status}'.");
        }

        Status = BackgroundJobStatus.Completed;
        CompletedAt = DateTime.UtcNow;
    }

    public void Fail(string error)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(error);

        if (Status != BackgroundJobStatus.Running)
        {
            throw new InvalidOperationException($"Cannot fail a background job with status '{Status}'.");
        }

        Status = BackgroundJobStatus.Failed;
        CompletedAt = DateTime.UtcNow;
        Error = error;
    }

    public void Cancel()
    {
        if (Status is not (
            BackgroundJobStatus.Pending or
            BackgroundJobStatus.Running))
        {
            throw new InvalidOperationException($"Cannot cancel a background job with status '{Status}'.");
        }

        Status = BackgroundJobStatus.Cancelled;
        CompletedAt = DateTime.UtcNow;
    }

    public static BackgroundJob Create(BackgroundJobType type, BackgroundJobPriority priority, JobApplicationId jobApplicationId, string? payload = null)
    {
        return new BackgroundJob
        {
            Id = new BackgroundJobId(Guid.NewGuid()),
            Type = type,
            Priority = priority,
            CreatedAt = DateTime.UtcNow,
            JobApplicationId = jobApplicationId,
            Payload = payload
        };
    }

    public static BackgroundJob Rehydrate(
        BackgroundJobId id,
        BackgroundJobType type,
        BackgroundJobPriority priority,
        BackgroundJobStatus status,
        DateTime createdAt,
        DateTime? startedAt,
        DateTime? completedAt,
        string? error,
        JobApplicationId jobApplicationId,
        string? payload)
    {
        return new BackgroundJob
        {
            Id = id,
            Type = type,
            Priority = priority,
            Status = status,
            CreatedAt = createdAt,
            StartedAt = startedAt,
            CompletedAt = completedAt,
            Error = error,
            JobApplicationId = jobApplicationId,
            Payload = payload
        };
    }
}