using JobApplicationHelper.Application.Repositories;
using JobApplicationHelper.Domain.Models;

namespace JobApplicationHelper.Tests.BackgroundJobs;

internal sealed class FakeBackgroundJobRepository
    : IBackgroundJobRepository
{
    private readonly Dictionary<BackgroundJobId, BackgroundJob> jobs = [];

    public Task AddAsync(BackgroundJob job, CancellationToken cancellationToken = default)
    {
        jobs.Add(job.Id, job);
        return Task.CompletedTask;
    }

    public Task<BackgroundJob?> GetAsync(BackgroundJobId id, CancellationToken cancellationToken = default)
    {
        jobs.TryGetValue(id, out var job);
        return Task.FromResult(job);
    }

    public Task<IReadOnlyList<BackgroundJob>> GetPendingAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<BackgroundJob> result = jobs.Values
            .Where(job => job.Status == BackgroundJobStatus.Pending)
            .OrderByDescending(job => job.Priority)
            .ThenBy(job => job.CreatedAt)
            .ToList();

        return Task.FromResult(result);
    }

    public Task UpdateAsync(BackgroundJob job, CancellationToken cancellationToken = default)
    {
        jobs[job.Id] = job;
        return Task.CompletedTask;
    }

    public Task RecoverRunningAsync(CancellationToken cancellationToken = default)
    {
        var runningJobs = jobs.Values
            .Where(job => job.Status == BackgroundJobStatus.Running)
            .ToList();

        foreach (var job in runningJobs)
        {
            jobs[job.Id] = BackgroundJob.Rehydrate(
                job.Id,
                job.Type,
                job.Priority,
                BackgroundJobStatus.Pending,
                job.CreatedAt,
                null,
                null,
                null,
                job.JobApplicationId,
                job.Payload);
        }

        return Task.CompletedTask;
    }
}