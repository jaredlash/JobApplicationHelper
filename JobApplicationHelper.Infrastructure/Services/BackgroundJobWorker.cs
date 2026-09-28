using JobApplicationHelper.Application.Configuration;
using JobApplicationHelper.Application.Services;
using JobApplicationHelper.Infrastructure.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JobApplicationHelper.Infrastructure.Services;

public sealed class BackgroundJobWorker(
    IBackgroundJobService backgroundJobService,
    IBackgroundJobQueue backgroundJobQueue,
    IOptions<BackgroundJobOptions> options,
    ILogger<BackgroundJobWorker> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        await backgroundJobService.RecoverPendingJobsAsync(
            stoppingToken);

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
            try
            {
                var jobId = await backgroundJobQueue.DequeueAsync(
                    cancellationToken);

                await backgroundJobService.ExecuteAsync(
                    jobId,
                    cancellationToken);
            }
            catch (OperationCanceledException)
                when (cancellationToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "An error occurred while executing a background job.");
            }
        }
    }
}