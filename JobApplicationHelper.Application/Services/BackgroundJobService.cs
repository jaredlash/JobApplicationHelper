using JobApplicationHelper.Application.Repositories;
using JobApplicationHelper.Domain.Models;

namespace JobApplicationHelper.Application.Services;

public sealed class BackgroundJobService(
    IBackgroundJobRepository backgroundJobRepository,
    IBackgroundJobQueue backgroundJobQueue)
    : IBackgroundJobService
{
    public async Task<BackgroundJobId> CreateAsync(
        BackgroundJobType type,
        BackgroundJobPriority priority,
        CancellationToken cancellationToken = default)
    {
        var job = BackgroundJob.Create(type, priority);

        await backgroundJobRepository.AddAsync(job, cancellationToken);

        await backgroundJobQueue.EnqueueAsync(
            job.Id,
            job.Priority,
            job.CreatedAt,
            cancellationToken);

        return job.Id;
    }

    public Task<BackgroundJob?> GetAsync(BackgroundJobId id, CancellationToken cancellationToken = default)
        => backgroundJobRepository.GetAsync(id, cancellationToken);
}