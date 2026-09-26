using JobApplicationHelper.Contracts.JobApplications;
using JobApplicationHelper.Domain.Models;

namespace JobApplicationHelper.ApiMappings.ToDomain;

public static class JobApplicationMappingExtensions
{
    public static ApplicationFile ToDomain(this CreateJobApplicationRequest request)
    {
        return new ApplicationFile(
            request.CountryCode,
            request.IncludeCoverLetter,
            request.CompanyName,
            request.PositionTitle,
            request.URL,
            request.City,
            request.JobPosting
        );
    }
}
