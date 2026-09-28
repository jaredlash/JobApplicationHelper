using JobApplicationHelper.Application.Services;
using JobApplicationHelper.Domain.Models;

namespace JobApplicationHelper.Infrastructure.Services;

public sealed class BackgroundJobQueue : IBackgroundJobQueue, IDisposable
{
    private readonly PriorityQueue<BackgroundJobId, (int Priority, DateTime CreatedAt)> queue = new();

    private readonly SemaphoreSlim itemsAvailable = new(0);
    private readonly Lock sync = new();

    public ValueTask EnqueueAsync(
        BackgroundJobId jobId,
        BackgroundJobPriority priority,
        DateTime createdAt,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var queuePriority = (GetPriorityValue(priority), createdAt);

        lock (sync)
        {
            queue.Enqueue(jobId, queuePriority);
        }

        itemsAvailable.Release();

        return ValueTask.CompletedTask;
    }

    public async ValueTask<BackgroundJobId> DequeueAsync(CancellationToken cancellationToken = default)
    {
        await itemsAvailable.WaitAsync(cancellationToken);

        lock (sync)
        {
            return queue.Dequeue();
        }
    }

    private static int GetPriorityValue(BackgroundJobPriority priority) =>
        priority switch
        {
            BackgroundJobPriority.High => 0,
            BackgroundJobPriority.Normal => 1,
            _ => throw new ArgumentOutOfRangeException(nameof(priority), priority, null)
        };

    public void Dispose()
    {
        itemsAvailable.Dispose();
    }
}
