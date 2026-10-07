using JobApplicationHelper.Contracts.Experiences;
using JobApplicationHelper.Domain.Models;

namespace JobApplicationHelper.ApiMappings.ToDto;

public static class ExperienceMappingExtensions
{
    public static ExperienceDto ToDto(this Experience experience)
    {
        return new ExperienceDto(
                experience.Id.Value,
                experience.Title,
                experience.Type.ToString(),
                experience.Organization,
                experience.DateRange is null
                    ? null
                    : new DateRangeDto(
                        experience.DateRange.Start is null
                            ? null
                            : new PartialDateDto(experience.DateRange.Start.Year, experience.DateRange.Start.Month, experience.DateRange.Start.Day),
                        experience.DateRange.End is null
                            ? null
                            : new PartialDateDto(experience.DateRange.End.Year, experience.DateRange.End.Month, experience.DateRange.End.Day)),
                experience.Summary,
                experience.Skills,
                experience.Evidence,
                experience.Contexts,
                experience.Links,
                experience.Notes);
    }

    public static PendingExperienceImportDto ToDto(this PendingExperienceImport pendingExperienceImport)
    {
        return new PendingExperienceImportDto(pendingExperienceImport.Current.ToDto(), pendingExperienceImport.Pending.ToDto());
    }
}
