using JobApplicationHelper.Application.Services;
using JobApplicationHelper.Domain.Models;

namespace JobApplicationHelper.Tests.BackgroundJobs;

internal sealed class FailingBackgroundJobExecutor : IBackgroundJobExecutor
{
    private readonly Exception exception;

    public FailingBackgroundJobExecutor(Exception exception)
    {
        this.exception = exception;
    }

    public Task ExecuteAsync(
        BackgroundJob job,
        CancellationToken cancellationToken = default)
    {
        return Task.FromException(exception);
    }
}