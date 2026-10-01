using JobApplicationHelper.Domain.Models;

namespace JobApplicationHelper.Application.Repositories;

public interface ICoverLetterDraftRepository
{
    Task<CoverLetterDraft?> GetAsync(JobApplicationId jobApplicationId, CancellationToken cancellationToken = default);

    Task AddOrReplaceAsync(CoverLetterDraft draft, CancellationToken cancellationToken = default);
}