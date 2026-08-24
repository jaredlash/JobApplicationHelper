using JobApplicationHelper.Models;

namespace JobApplicationHelper.Services;

public interface IExperienceBankService
{
    Task<IReadOnlyList<Experience>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Experience?> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    Task AddAsync(Experience experience, CancellationToken cancellationToken = default);

    Task UpdateAsync(Experience experience, CancellationToken cancellationToken = default);

    Task DeleteAsync(string id, CancellationToken cancellationToken = default);
}