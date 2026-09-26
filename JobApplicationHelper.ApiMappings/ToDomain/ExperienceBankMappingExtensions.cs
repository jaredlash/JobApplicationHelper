using JobApplicationHelper.Contracts.Experiences;
using JobApplicationHelper.Domain.Models;

namespace JobApplicationHelper.ApiMappings.ToDomain;

public static class ExperienceBankMappingExtensions
{
    public static Experience ToDomain(this ExperienceDto experienceDto)
    {
        return new Experience
        {
            Id = experienceDto.Id,
            Title = experienceDto.Title,
            Type = Enum.Parse<ExperienceType>(
                experienceDto.Type,
                ignoreCase: true),
            Organization = experienceDto.Organization,
            DateRange = experienceDto.DateRange is null
                ? null
                : new DateRange
                {
                    Start = experienceDto.DateRange.Start is null
                        ? null
                        : new PartialDate
                        {
                            Year = experienceDto.DateRange.Start.Year,
                            Month = experienceDto.DateRange.Start.Month,
                            Day = experienceDto.DateRange.Start.Day
                        },
                    End = experienceDto.DateRange.End is null
                        ? null
                        : new PartialDate
                        {
                            Year = experienceDto.DateRange.End.Year,
                            Month = experienceDto.DateRange.End.Month,
                            Day = experienceDto.DateRange.End.Day
                        }
                },
            Summary = experienceDto.Summary,
            Skills = experienceDto.Skills.ToList(),
            Evidence = experienceDto.Evidence.ToList(),
            Contexts = experienceDto.Contexts.ToList(),
            Links = experienceDto.Links.ToList(),
            Notes = experienceDto.Notes
        };
    }
}
