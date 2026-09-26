using JobApplicationHelper.Contracts.JobApplications;
using JobApplicationHelper.Domain.Models;

namespace JobApplicationHelper.ApiMappings.ToDto;

public static class JobApplicationMappingExtensions
{
    public static CreateJobApplicationRequest ToDto(this ApplicationFile application)
    {
        return new CreateJobApplicationRequest(
            application.CountryCode,
            application.IncludeCoverLetter,
            application.CompanyName,
            application.PositionTitle,
            application.URL,
            application.City,
            application.JobPosting
        );
    }
}
