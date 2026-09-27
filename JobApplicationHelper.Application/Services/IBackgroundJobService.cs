using JobApplicationHelper.Domain.Models;

namespace JobApplicationHelper.Application.Services;

public interface IBackgroundJobService
{
    Task<BackgroundJobId> CreateAsync(
        BackgroundJobType type,
        CancellationToken cancellationToken = default);

    Task<BackgroundJob?> GetAsync(
        BackgroundJobId id,
        CancellationToken cancellationToken = default);
}