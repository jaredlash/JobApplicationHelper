using JobApplicationHelper.Domain.Models;

namespace JobApplicationHelper.Application.Repositories;

public interface IVerifyCoverLetterResultRepository
{
    Task AddOrReplaceAsync(VerifyCoverLetterResult result, CancellationToken cancellationToken = default);

    Task<VerifyCoverLetterResult?> GetAsync(JobApplicationId jobApplicationId, CancellationToken cancellationToken = default);

    Task DeleteAsync(JobApplicationId jobApplicationId, CancellationToken cancellationToken = default);
}
