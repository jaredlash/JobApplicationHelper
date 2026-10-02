
using JobApplicationHelper.Contracts.BackgroundJobs;
using JobApplicationHelper.Domain.Models;
using JobApplicationHelper.Services.Api;

namespace JobApplicationHelper.Services.BackgroundJobs;

public sealed class BackgroundJobPollingService(
    BackgroundJobsApiClient backgroundJobsApiClient,
    IBackgroundJobNotificationService backgroundJobNotificationService)
    : IBackgroundJobPollingService
{

    public async Task<GetBackgroundJobResponse> WaitForCompletionAsync(
        BackgroundJobId backgroundJobId,
        CancellationToken cancellationToken = default)
    {
        var response = await backgroundJobsApiClient.GetAsync(
            backgroundJobId,
            cancellationToken);

        if (IsTerminal(ParseStatus(response.Status)))
            return response;

        while (true)
        {
            var notification =
                await backgroundJobNotificationService.WaitForStatusChangeAsync(
                    backgroundJobId,
                    cancellationToken);

            if (IsTerminal(ParseStatus(notification.Status)))
            {
                return await backgroundJobsApiClient.GetAsync(backgroundJobId, cancellationToken);
            }
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