using JobApplicationHelper.Contracts.BackgroundJobs;
using JobApplicationHelper.Domain.Models;

namespace JobApplicationHelper.Services.BackgroundJobs;

public interface IBackgroundJobNotificationService
{
    Task ConnectAsync(CancellationToken cancellationToken = default);

    Task DisconnectAsync(CancellationToken cancellationToken = default);

    Task<BackgroundJobStatusChanged> WaitForStatusChangeAsync(BackgroundJobId jobId, CancellationToken cancellationToken = default);
}