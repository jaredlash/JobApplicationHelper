using JobApplicationHelper.Domain.Models;

namespace JobApplicationHelper.Application.Services;

public interface IBackgroundJobService
{
    Task<BackgroundJobId> CreateAsync(
        BackgroundJobType type,
        BackgroundJobPriority priority,
        CancellationToken cancellationToken = default);

    Task<BackgroundJob?> GetAsync(
        BackgroundJobId id,
        CancellationToken cancellationToken = default);

    Task ExecuteAsync(BackgroundJobId id, CancellationToken cancellationToken = default);

    Task RecoverPendingJobsAsync(CancellationToken cancellationToken = default);
}