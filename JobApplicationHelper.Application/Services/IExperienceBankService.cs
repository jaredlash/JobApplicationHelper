using JobApplicationHelper.Domain.Models;

namespace JobApplicationHelper.Application.Services;

public interface IExperienceBankService
{
    Task<IReadOnlyList<Experience>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Experience?> GetByIdAsync(ExperienceId id, CancellationToken cancellationToken = default);

    Task AddAsync(Experience experience, CancellationToken cancellationToken = default);

    Task UpdateAsync(Experience experience, CancellationToken cancellationToken = default);

    Task DeleteAsync(ExperienceId id, CancellationToken cancellationToken = default);
}