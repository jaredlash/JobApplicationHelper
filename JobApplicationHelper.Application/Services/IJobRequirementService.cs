using JobApplicationHelper.Domain.Models;

namespace JobApplicationHelper.Application.Services
{
    public interface IJobRequirementService
    {
        Task<JobRequirements> ExtractRequirementsAsync(string jobPosting, CancellationToken cancellationToken = default);
    }
}