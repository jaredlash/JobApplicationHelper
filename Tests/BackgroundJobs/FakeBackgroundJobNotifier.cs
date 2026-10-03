using JobApplicationHelper.Application.Services;
using JobApplicationHelper.Domain.Models;

namespace JobApplicationHelper.Tests.BackgroundJobs;

internal class FakeBackgroundJobNotifier : IBackgroundJobNotifier
{
    public Task NotifyStatusChangedAsync(BackgroundJob job, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
