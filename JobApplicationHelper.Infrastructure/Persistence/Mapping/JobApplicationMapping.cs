using JobApplicationHelper.Domain.Models;
using JobApplicationHelper.Infrastructure.Persistence.Entities;

namespace JobApplicationHelper.Infrastructure.Persistence.Mapping;

internal static class JobApplicationMapping
{
    public static JobApplicationEntity ToEntity(this JobApplication application)
    {
        return new JobApplicationEntity
        {
            Id = application.Id.Value,
            CountryCode = application.CountryCode,
            IncludeCoverLetter = application.IncludeCoverLetter,
            CompanyName = application.CompanyName,
            PositionTitle = application.PositionTitle,
            URL = application.URL,
            City = application.City,
            JobPosting = application.JobPosting,
            CreatedAt = application.CreatedAt,
            ApplicationFolder = application.ApplicationFolder
        };
    }

    public static JobApplication ToDomain(this JobApplicationEntity entity)
    {
        return new JobApplication
        {
            Id = new JobApplicationId(entity.Id),
            CountryCode = entity.CountryCode,
            IncludeCoverLetter = entity.IncludeCoverLetter,
            CompanyName = entity.CompanyName,
            PositionTitle = entity.PositionTitle,
            URL = entity.URL,
            City = entity.City,
            JobPosting = entity.JobPosting,
            CreatedAt = entity.CreatedAt,
            ApplicationFolder = entity.ApplicationFolder
        };
    }
}