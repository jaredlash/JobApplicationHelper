namespace JobApplicationHelper.Contracts.Experiences;

public sealed record ExperienceDto(
    string Id,
    string Title,
    string Type,
    string Organization,
    DateRangeDto? DateRange,
    string Summary,
    IReadOnlyList<string> Skills,
    IReadOnlyList<string> Evidence,
    IReadOnlyList<string> Contexts,
    IReadOnlyList<string> Links,
    string Notes);