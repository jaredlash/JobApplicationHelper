using JobApplicationHelper.Contracts.Experiences;
using JobApplicationHelper.Domain.Models;

namespace JobApplicationHelper.ApiMappings.ToDomain;

public static class ExperienceMappingExtensions
{
    public static Experience ToDomain(this ExperienceDto experienceDto)
    {
        return new Experience
        {
            Id = new ExperienceId(experienceDto.Id),
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

    public static Experience ToDomain(this UpsertExperienceRequest upsertDto, Guid id)
    {
        return new Experience
        {
            Id = new ExperienceId(id),
            Title = upsertDto.Title,
            Type = Enum.Parse<ExperienceType>(
                upsertDto.Type,
                ignoreCase: true),
            Organization = upsertDto.Organization,
            DateRange = upsertDto.DateRange is null
                ? null
                : new DateRange
                {
                    Start = upsertDto.DateRange.Start is null
                        ? null
                        : new PartialDate
                        {
                            Year = upsertDto.DateRange.Start.Year,
                            Month = upsertDto.DateRange.Start.Month,
                            Day = upsertDto.DateRange.Start.Day
                        },
                    End = upsertDto.DateRange.End is null
                        ? null
                        : new PartialDate
                        {
                            Year = upsertDto.DateRange.End.Year,
                            Month = upsertDto.DateRange.End.Month,
                            Day = upsertDto.DateRange.End.Day
                        }
                },
            Summary = upsertDto.Summary,
            Skills = upsertDto.Skills.ToList(),
            Evidence = upsertDto.Evidence.ToList(),
            Contexts = upsertDto.Contexts.ToList(),
            Links = upsertDto.Links.ToList(),
            Notes = upsertDto.Notes
        };
    }
}
