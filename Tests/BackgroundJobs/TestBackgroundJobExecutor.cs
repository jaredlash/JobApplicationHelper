using JobApplicationHelper.Application.Services;
using JobApplicationHelper.Domain.Models;

namespace JobApplicationHelper.Tests.BackgroundJobs;

internal sealed class TestBackgroundJobExecutor : IBackgroundJobExecutor
{
    private readonly TaskCompletionSource<BackgroundJob> completion =
        new(TaskCreationOptions.RunContinuationsAsynchronously);

    public Task<BackgroundJob> Completion => completion.Task;

    public Task ExecuteAsync(
        BackgroundJob job,
        CancellationToken cancellationToken = default)
    {
        var snapshot = BackgroundJob.Rehydrate(
            job.Id,
            job.Type,
            job.Priority,
            job.Status,
            job.CreatedAt,
            job.StartedAt,
            job.CompletedAt,
            job.Error);

        completion.TrySetResult(snapshot);

        return Task.CompletedTask;
    }
}
