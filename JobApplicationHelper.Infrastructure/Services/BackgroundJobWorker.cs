using JobApplicationHelper.Application.Services;
using JobApplicationHelper.Infrastructure.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JobApplicationHelper.Infrastructure.Services;

public sealed class BackgroundJobWorker(
    IBackgroundJobService backgroundJobService,
    IBackgroundJobQueue backgroundJobQueue,
    IBackgroundJobExecutor backgroundJobExecutor,
    IOptions<BackgroundJobOptions> options,
    ILogger<BackgroundJobWorker> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        await backgroundJobService.RecoverPendingJobsAsync(stoppingToken);

        var workers = Enumerable
            .Range(0, options.Value.MaxConcurrency)
            .Select(_ => ProcessJobsAsync(stoppingToken));

        await Task.WhenAll(workers);
    }

    private async Task ProcessJobsAsync(
        CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var jobId = await backgroundJobQueue.DequeueAsync(
                cancellationToken);

            var job = await backgroundJobService.GetAsync(
                jobId,
                cancellationToken);

            if (job is null)
            {
                logger.LogWarning(
                    "Background job {JobId} was not found.",
                    jobId);

                continue;
            }

            // Execution/lifecycle handling comes next.
            await backgroundJobExecutor.ExecuteAsync(
                job,
                cancellationToken);
        }
    }
}
