using JobApplicationHelper.Domain.Models;

namespace JobApplicationHelper.Application.Services;

public interface IPendingExperienceImportService
{
    Task<int> GetCountAsync(CancellationToken cancellationToken = default);

    Task<PendingExperienceImport?> GetNextAsync(CancellationToken cancellationToken = default);

    Task ResolveAsync(ExperienceId id, PendingExperienceImportResolution resolution, CancellationToken cancellationToken = default);
}