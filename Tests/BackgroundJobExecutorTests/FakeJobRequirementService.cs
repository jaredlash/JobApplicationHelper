using JobApplicationHelper.Application.Services;
using JobApplicationHelper.Domain.Models;

namespace JobApplicationHelper.Tests.BackgroundJobExecutorTests;

public sealed class FakeJobRequirementService(JobRequirements? requirements = null)
    : IJobRequirementService
{
    private readonly JobRequirements? requirements = requirements;

    public string? ReceivedJobPosting { get; private set; }

    public Task<JobRequirements> ExtractRequirementsAsync(string jobPosting, CancellationToken cancellationToken = default)
    {
        ReceivedJobPosting = jobPosting;

        return Task.FromResult(
            requirements
            ?? new JobRequirements
            {
                Requirements =
                [
                    new JobRequirement
                    {
                        Requirement = "Test requirement",
                        Category = RequirementCategory.TechnicalSkill,
                        Priority = RequirementPriority.Required
                    }
                ]
            });
    }
}