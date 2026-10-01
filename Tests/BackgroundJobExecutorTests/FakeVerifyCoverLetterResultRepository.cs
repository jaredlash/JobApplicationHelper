using JobApplicationHelper.Application.Repositories;
using JobApplicationHelper.Domain.Models;

namespace JobApplicationHelper.Tests.BackgroundJobExecutorTests;

internal class FakeVerifyCoverLetterResultRepository : IVerifyCoverLetterResultRepository
{
    public Task AddOrReplaceAsync(VerifyCoverLetterResult result, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task DeleteAsync(JobApplicationId jobApplicationId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<VerifyCoverLetterResult?> GetAsync(JobApplicationId jobApplicationId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
