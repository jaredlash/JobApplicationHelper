using JobApplicationHelper.Api.Services;
using JobApplicationHelper.Contracts.BackgroundJobs;
using JobApplicationHelper.Domain.Models;
using Microsoft.AspNetCore.SignalR;

namespace JobApplicationHelper.Api.Hubs;

public sealed class BackgroundJobHub : Hub<IBackgroundJobClient>
{
    public Task SubscribeToJobApplication(Guid jobApplicationId)
    {
        return Groups.AddToGroupAsync(Context.ConnectionId, SignalRBackgroundJobNotifier.GetGroupName(new JobApplicationId(jobApplicationId)));
    }

    public Task UnsubscribeFromJobApplication(Guid jobApplicationId)
    {
        return Groups.RemoveFromGroupAsync(Context.ConnectionId, SignalRBackgroundJobNotifier.GetGroupName(new JobApplicationId(jobApplicationId)));
    }
}