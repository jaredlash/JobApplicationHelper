using JobApplicationHelper.Domain.Models;
using JobApplicationHelper.Infrastructure.Persistence.Entities;

namespace JobApplicationHelper.Infrastructure.Persistence.Mapping;

internal static class BackgroundJobMapping
{
    public static BackgroundJobEntity ToEntity(this BackgroundJob job) => new()
    {
        Id = job.Id.Value,
        Type = job.Type,
        Priority = job.Priority,
        Status = job.Status,
        CreatedAt = job.CreatedAt,
        StartedAt = job.StartedAt,
        CompletedAt = job.CompletedAt,
        Error = job.Error,
        JobApplicationId = job.JobApplicationId.Value,
        Payload = job.Payload
    };

    public static BackgroundJob ToDomain(this BackgroundJobEntity entity) =>
        BackgroundJob.Rehydrate(
            new BackgroundJobId(entity.Id),
            entity.Type,
            entity.Priority,
            entity.Status,
            entity.CreatedAt,
            entity.StartedAt,
            entity.CompletedAt,
            entity.Error,
            new JobApplicationId(entity.JobApplicationId),
            entity.Payload
        );
}