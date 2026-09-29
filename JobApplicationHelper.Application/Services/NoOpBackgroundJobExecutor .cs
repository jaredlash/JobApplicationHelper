using JobApplicationHelper.Domain.Models;

namespace JobApplicationHelper.Application.Services;

public sealed class NoOpBackgroundJobExecutor : IBackgroundJobExecutor
{
    public Task ExecuteAsync(
        BackgroundJob job,
        CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}