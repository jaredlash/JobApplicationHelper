using JobApplicationHelper.Application.Services;
using JobApplicationHelper.Domain.Models;

namespace JobApplicationHelper.Tests.BackgroundJobs;

internal sealed class RecordingBackgroundJobExecutor
    : IBackgroundJobExecutor
{
    private readonly List<BackgroundJobId> executedJobs = [];
    private readonly object sync = new();

    public IReadOnlyList<BackgroundJobId> ExecutedJobs
    {
        get
        {
            lock (sync)
            {
                return executedJobs.ToList();
            }
        }
    }

    public Task ExecuteAsync(
        BackgroundJob job,
        CancellationToken cancellationToken = default)
    {
        lock (sync)
        {
            executedJobs.Add(job.Id);
        }

        return Task.CompletedTask;
    }
}