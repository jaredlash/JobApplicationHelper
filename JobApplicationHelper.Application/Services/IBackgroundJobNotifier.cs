using JobApplicationHelper.Domain.Models;

namespace JobApplicationHelper.Application.Services;

public interface IBackgroundJobNotifier
{
    Task NotifyStatusChangedAsync(BackgroundJob job, CancellationToken cancellationToken = default);
}