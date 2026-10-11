using JobApplicationHelper.Domain.Models;
using JobApplicationHelper.Infrastructure.Persistence.Entities;

namespace JobApplicationHelper.Infrastructure.Persistence.Mapping;

public static class ExperiencesMapping
{
    // This file does not contain a .ToEntity() mapping as the logic is more involved
    // and better suited to the Repository implementation

    public static Experience ToDomain(this ExperienceEntity entity)
    {
        return new Experience
        {
            Id = new ExperienceId(entity.Id),
            Title = entity.Title,
            Type = entity.Type,
            Organization = entity.Organization ?? string.Empty,

            DateRange = MapEntityDateRangeToDomain(entity),

            Summary = entity.Summary,

            Skills = entity.Skills
                .OrderBy(e => e.Skill.Name, StringComparer.OrdinalIgnoreCase)
                .Select(e => e.Skill.Name)
                .ToList(),

            Evidence = entity.Evidence
                .OrderBy(e => e.Text, StringComparer.OrdinalIgnoreCase)
                .Select(e => e.Text)
                .ToList(),

            Contexts = entity.Contexts
                .OrderBy(e => e.Context.Name, StringComparer.OrdinalIgnoreCase)
                .Select(e => e.Context.Name)
                .ToList(),

            Links = entity.Links
                .OrderBy(e => e.Url, StringComparer.OrdinalIgnoreCase)
                .Select(e => e.Url)
                .ToList(),

            Notes = entity.Notes ?? string.Empty
        };
    }

    private static DateRange? MapEntityDateRangeToDomain(ExperienceEntity entity)
    {
        return new DateRange
        {
            Start = entity.StartYear is not int startYear
                ? null
                : new PartialDate
                {
                    Year = startYear,
                    Month = entity.StartMonth,
                    Day = entity.StartDay
                },

            End = entity.EndYear is not int endYear
                ? null
                : new PartialDate
                {
                    Year = endYear,
                    Month = entity.EndMonth,
                    Day = entity.EndDay
                }
        };
    }

    public static void MapDateRange(this ExperienceEntity entity, DateRange? dateRange)
    {
        entity.StartYear = dateRange?.Start?.Year;
        entity.StartMonth = dateRange?.Start?.Month;
        entity.StartDay = dateRange?.Start?.Day;

        entity.EndYear = dateRange?.End?.Year;
        entity.EndMonth = dateRange?.End?.Month;
        entity.EndDay = dateRange?.End?.Day;
    }
}
