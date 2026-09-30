using JobApplicationHelper.Domain.Models;

namespace JobApplicationHelper.Infrastructure.Persistence.Entities;

public sealed class BackgroundJobEntity
{
    public Guid Id { get; set; }

    public BackgroundJobType Type { get; set; }

    public BackgroundJobPriority Priority { get; set; }

    public BackgroundJobStatus Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? StartedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public string? Error { get; set; }

    public Guid JobApplicationId { get; set; }

    public string? Payload { get; set; }
}