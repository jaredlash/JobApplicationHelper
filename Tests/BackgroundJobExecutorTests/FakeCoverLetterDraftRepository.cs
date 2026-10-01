using JobApplicationHelper.Application.Repositories;
using JobApplicationHelper.Domain.Models;

namespace JobApplicationHelper.Tests.BackgroundJobExecutorTests;

internal class FakeCoverLetterDraftRepository : ICoverLetterDraftRepository
{
    private readonly Dictionary<JobApplicationId, CoverLetterDraft> _drafts = new();

    public Task AddOrReplaceAsync(CoverLetterDraft draft, CancellationToken cancellationToken = default)
    {
        _drafts[draft.JobApplicationId] = draft;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(JobApplicationId jobApplicationId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<CoverLetterDraft?> GetAsync(JobApplicationId jobApplicationId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_drafts.TryGetValue(jobApplicationId, out var draft) ? draft : null);
    }
}
