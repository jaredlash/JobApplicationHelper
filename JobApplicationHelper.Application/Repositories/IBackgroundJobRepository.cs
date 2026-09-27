using JobApplicationHelper.Domain.Models;

namespace JobApplicationHelper.Application.Repositories;

public interface IBackgroundJobRepository
{
    Task AddAsync(BackgroundJob job, CancellationToken cancellationToken = default);

    Task<BackgroundJob?> GetAsync(BackgroundJobId id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<BackgroundJob>> GetPendingAsync(CancellationToken cancellationToken = default);

    Task UpdateAsync(BackgroundJob job, CancellationToken cancellationToken = default);
}