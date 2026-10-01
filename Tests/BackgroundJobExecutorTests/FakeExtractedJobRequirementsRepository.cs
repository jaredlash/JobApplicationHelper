using JobApplicationHelper.Application.Repositories;
using JobApplicationHelper.Domain.Models;

namespace JobApplicationHelper.Tests.BackgroundJobExecutorTests;

public sealed class FakeExtractedJobRequirementsRepository
    : IExtractedJobRequirementsRepository
{
    public ExtractedJobRequirements? SavedRequirements { get; private set; }

    public Task AddOrReplaceAsync(ExtractedJobRequirements requirements, CancellationToken cancellationToken = default)
    {
        SavedRequirements = requirements;

        return Task.CompletedTask;
    }

    public Task<ExtractedJobRequirements?> GetAsync(JobApplicationId jobApplicationId, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }

    public Task DeleteAsync(JobApplicationId jobApplicationId, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }
}