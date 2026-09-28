using JobApplicationHelper.Application.Services;
using JobApplicationHelper.Domain.Models;

namespace JobApplicationHelper.Tests.BackgroundJobs;

internal sealed class FailOnceBackgroundJobExecutor
    : IBackgroundJobExecutor
{
    private int executionCount;

    public int ExecutionCount => Volatile.Read(ref executionCount);

    public Task ExecuteAsync(BackgroundJob job, CancellationToken cancellationToken = default)
    {
        var count = Interlocked.Increment(ref executionCount);

        if (count == 1)
        {
            return Task.FromException(new InvalidOperationException("Test execution failure."));
        }

        return Task.CompletedTask;
    }
}