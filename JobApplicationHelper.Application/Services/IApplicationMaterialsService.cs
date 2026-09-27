using JobApplicationHelper.Domain.Models;

namespace JobApplicationHelper.Application.Services;

public interface IApplicationMaterialsService
{
    Task<JobApplicationId> CreateApplicationMaterialsAsync(ApplicationFile application, CancellationToken cancellationToken = default);

    Task SaveCoverLetterDraftAsync(JobApplicationId applicationId, string draft, CancellationToken cancellationToken = default);

    Task<string> GetApplicationFolderAsync(JobApplicationId applicationId, CancellationToken cancellationToken = default);
}
