using JobApplicationHelper.Domain.Models;

namespace JobApplicationHelper.Application.Repositories;

public interface IExtractedJobRequirementsRepository
{
    Task AddOrReplaceAsync(ExtractedJobRequirements requirements, CancellationToken cancellationToken = default);

    Task<ExtractedJobRequirements?> GetAsync(JobApplicationId jobApplicationId, CancellationToken cancellationToken = default);

    Task DeleteAsync(JobApplicationId jobApplicationId, CancellationToken cancellationToken = default);
}
