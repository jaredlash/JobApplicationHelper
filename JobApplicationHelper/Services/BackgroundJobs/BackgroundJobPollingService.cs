using JobApplicationHelper.Contracts.BackgroundJobs;
using JobApplicationHelper.Domain.Models;
using JobApplicationHelper.Exceptions;
using JobApplicationHelper.Services.Api;
using Microsoft.Extensions.Logging;

namespace JobApplicationHelper.Services.BackgroundJobs;

public sealed class BackgroundJobPollingService(
    BackgroundJobsApiClient backgroundJobsApiClient,
    IBackgroundJobNotificationService backgroundJobNotificationService,
    ILogger<BackgroundJobPollingService> logger)
    : IBackgroundJobPollingService
{
    private static readonly TimeSpan PollingInterval = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan BackgroundJobNotificationTimeout = TimeSpan.FromMinutes(10);

    public async Task<GetBackgroundJobResponse> WaitForCompletionAsync(BackgroundJobId backgroundJobId, CancellationToken cancellationToken = default)
    {
        var response = await backgroundJobsApiClient.GetAsync(backgroundJobId, cancellationToken);

        if (IsTerminal(ParseStatus(response.Status)))
            return response;

        try
        {
            await backgroundJobNotificationService.ConnectAsync(cancellationToken);

            return await WaitForCompletionViaSignalRAsync(backgroundJobId, cancellationToken);
        }
        catch (TimeoutException)
        {
            logger.LogWarning("SignalR notification timed out while waiting for background job {BackgroundJobId}.", backgroundJobId.Value);
            var timeoutJob = await backgroundJobsApiClient.GetAsync(backgroundJobId, cancellationToken);
            if (IsTerminal(ParseStatus(timeoutJob.Status)))
                return timeoutJob;
            throw new TimeoutException($"Timed out while waiting for background job {backgroundJobId.Value}.");
        }
        catch (BackgroundJobNotificationException ex)
        {
            logger.LogWarning(ex, "SignalR unavailable while waiting for background job {BackgroundJobId}. Falling back to polling.", backgroundJobId.Value);

            return await WaitForCompletionViaPollingAsync(backgroundJobId, cancellationToken);
        }
    }


    private async Task<GetBackgroundJobResponse> WaitForCompletionViaSignalRAsync(BackgroundJobId backgroundJobId, CancellationToken cancellationToken)
    {
        while (true)
        {
            var notification = await backgroundJobNotificationService.WaitForStatusChangeAsync(backgroundJobId, BackgroundJobNotificationTimeout, cancellationToken);

            var status = ParseStatus(notification.Status);

            if (IsTerminal(status))
            {
                return await backgroundJobsApiClient.GetAsync(backgroundJobId, cancellationToken);
            }
        }
    }

    private async Task<GetBackgroundJobResponse> WaitForCompletionViaPollingAsync(BackgroundJobId backgroundJobId, CancellationToken cancellationToken)
    {
        using var timeoutCts = new CancellationTokenSource(BackgroundJobNotificationTimeout);
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

        var waitCancellationToken = linkedCts.Token;

        while (true)
        {
            var response = await backgroundJobsApiClient.GetAsync(backgroundJobId, waitCancellationToken);

            var status = ParseStatus(response.Status);

            if (IsTerminal(status))
            {
                return response;
            }

            await Task.Delay(PollingInterval, waitCancellationToken);
        }
    }

    private static BackgroundJobStatus ParseStatus(string status)
    {
        if (!Enum.TryParse<BackgroundJobStatus>(status, ignoreCase: true, out var statusResult))
        {
            throw new InvalidOperationException($"The background jobs API returned an unknown status '{status}'.");
        }

        return statusResult;
    }

    private static bool IsTerminal(BackgroundJobStatus status) =>
        status is
            BackgroundJobStatus.Completed or
            BackgroundJobStatus.Failed or
            BackgroundJobStatus.Cancelled;
}