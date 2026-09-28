using JobApplicationHelper.Application.Services;
using JobApplicationHelper.Domain.Models;

namespace JobApplicationHelper.Tests.BackgroundJobs;

internal sealed class BlockingBackgroundJobExecutor
    : IBackgroundJobExecutor
{
    private readonly TaskCompletionSource<bool> firstJobStarted = new(TaskCreationOptions.RunContinuationsAsynchronously);

    private readonly TaskCompletionSource<bool> releaseJobs = new(TaskCreationOptions.RunContinuationsAsynchronously);

    private int activeJobs;
    private int maximumActiveJobs;

    public int MaximumActiveJobs => maximumActiveJobs;

    public Task FirstJobStarted => firstJobStarted.Task;

    public void Release()
    {
        releaseJobs.TrySetResult(true);
    }

    public async Task ExecuteAsync(
        BackgroundJob job,
        CancellationToken cancellationToken = default)
    {
        var active = Interlocked.Increment(ref activeJobs);

        UpdateMaximumActiveJobs(active);

        firstJobStarted.TrySetResult(true);

        try
        {
            await releaseJobs.Task.WaitAsync(cancellationToken);
        }
        finally
        {
            Interlocked.Decrement(ref activeJobs);
        }
    }

    private void UpdateMaximumActiveJobs(int value)
    {
        while (true)
        {
            var current = Volatile.Read(ref maximumActiveJobs);

            if (value <= current)
            {
                return;
            }

            if (Interlocked.CompareExchange(ref maximumActiveJobs, value, current) == current)
            {
                return;
            }
        }
    }
}