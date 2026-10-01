
using JobApplicationHelper.Contracts.BackgroundJobs;
using JobApplicationHelper.Domain.Models;
using JobApplicationHelper.Services.Api;
using Microsoft.Extensions.Options;

namespace JobApplicationHelper.Services.BackgroundJobs;

public sealed class BackgroundJobPollingService(
    BackgroundJobsApiClient backgroundJobsApiClient)
    : IBackgroundJobPollingService
{
    private static readonly TimeSpan PollingInterval = TimeSpan.FromSeconds(1);

    public async Task<GetBackgroundJobResponse> WaitForCompletionAsync(BackgroundJobId backgroundJobId, CancellationToken cancellationToken = default)
    {
        while (true)
        {
            var response = await backgroundJobsApiClient.GetAsync(backgroundJobId, cancellationToken);

            var status = ParseStatus(response);

            if (IsTerminal(status))
            {
                return response;
            }

            await Task.Delay(PollingInterval, cancellationToken);
        }
    }

    private static BackgroundJobStatus ParseStatus(GetBackgroundJobResponse response)
    {
        if (!Enum.TryParse<BackgroundJobStatus>(response.Status, ignoreCase: true, out var status))
        {
            throw new InvalidOperationException($"The background jobs API returned an unknown status '{response.Status}'.");
        }

        return status;
    }

    private static bool IsTerminal(BackgroundJobStatus status) =>
        status is
            BackgroundJobStatus.Completed or
            BackgroundJobStatus.Failed or
            BackgroundJobStatus.Cancelled;
}