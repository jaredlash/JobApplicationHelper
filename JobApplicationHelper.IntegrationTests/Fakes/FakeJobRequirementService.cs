using JobApplicationHelper.Application.Services;
using JobApplicationHelper.Domain.Models;

namespace JobApplicationHelper.IntegrationTests.Fakes;

public sealed class FakeJobRequirementService(JobRequirements requirements)
    : IJobRequirementService
{
    public Task<JobRequirements> ExtractRequirementsAsync(string jobPosting, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(requirements);
    }
}