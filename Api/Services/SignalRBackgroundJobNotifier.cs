using JobApplicationHelper.Api.Hubs;
using JobApplicationHelper.Application.Services;
using JobApplicationHelper.Contracts.BackgroundJobs;
using JobApplicationHelper.Domain.Models;
using Microsoft.AspNetCore.SignalR;

namespace JobApplicationHelper.Api.Services;

public sealed class SignalRBackgroundJobNotifier(IHubContext<BackgroundJobHub, IBackgroundJobClient> hubContext)
    : IBackgroundJobNotifier
{
    public Task NotifyStatusChangedAsync(BackgroundJob job, CancellationToken cancellationToken = default)
    {
        return hubContext.Clients
            .Group(GetGroupName(job.JobApplicationId))
            .BackgroundJobStatusChanged(
                new BackgroundJobStatusChanged(
                    job.Id.Value,
                    job.Status.ToString(),
                    job.Error));
    }

    public static string GetGroupName(JobApplicationId jobApplicationId)
        => $"job-application:{jobApplicationId.Value}";
}