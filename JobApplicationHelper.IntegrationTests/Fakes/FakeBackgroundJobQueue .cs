using JobApplicationHelper.Application.Services;
using JobApplicationHelper.Domain.Models;

namespace JobApplicationHelper.IntegrationTests.Fakes;

public sealed class FakeBackgroundJobQueue : IBackgroundJobQueue
{
    public List<BackgroundJobId> EnqueuedJobs { get; } = [];

    public ValueTask EnqueueAsync(
        BackgroundJobId jobId,
        BackgroundJobPriority priority,
        DateTime createdAt,
        CancellationToken cancellationToken = default)
    {
        EnqueuedJobs.Add(jobId);

        return ValueTask.CompletedTask;
    }

    public ValueTask<BackgroundJobId> DequeueAsync(CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }
}