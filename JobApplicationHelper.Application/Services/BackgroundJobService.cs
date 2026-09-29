using JobApplicationHelper.Application.Repositories;
using JobApplicationHelper.Domain.Models;

namespace JobApplicationHelper.Application.Services;

public sealed class BackgroundJobService(
    IBackgroundJobRepository backgroundJobRepository,
    IBackgroundJobQueue backgroundJobQueue,
    IBackgroundJobExecutor backgroundJobExecutor)
    : IBackgroundJobService
{
    private readonly IBackgroundJobExecutor backgroundJobExecutor = backgroundJobExecutor;

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

    public async Task ExecuteAsync(BackgroundJobId id, CancellationToken cancellationToken = default)
    {
        var job = await backgroundJobRepository.GetAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Background job '{id.Value}' was not found.");

        if (job.Status != BackgroundJobStatus.Pending)
        {
            return;
        }

        job.Start();

        await backgroundJobRepository.UpdateAsync(job, cancellationToken);

        try
        {
            await backgroundJobExecutor.ExecuteAsync(job, cancellationToken);

            job.Complete();

            await backgroundJobRepository.UpdateAsync(job, cancellationToken);
        }
        catch (Exception ex)
        {
            job.Fail(ex.Message);

            await backgroundJobRepository.UpdateAsync(job, cancellationToken);

            throw;
        }
    }
    public Task<BackgroundJob?> GetAsync(BackgroundJobId id, CancellationToken cancellationToken = default)
        => backgroundJobRepository.GetAsync(id, cancellationToken);

    public async Task RecoverPendingJobsAsync(CancellationToken cancellationToken = default)
    {
        await backgroundJobRepository.RecoverRunningAsync(cancellationToken);

        var pendingJobs = await backgroundJobRepository.GetPendingAsync(cancellationToken);

        foreach (var job in pendingJobs)
        {
            await backgroundJobQueue.EnqueueAsync(
                job.Id,
                job.Priority,
                job.CreatedAt,
                cancellationToken);
        }
    }
}