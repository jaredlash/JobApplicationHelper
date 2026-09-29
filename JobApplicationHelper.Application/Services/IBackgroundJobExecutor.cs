using JobApplicationHelper.Domain.Models;

namespace JobApplicationHelper.Application.Services;

public interface IBackgroundJobExecutor
{
    Task ExecuteAsync(BackgroundJob job, CancellationToken cancellationToken = default);
}
