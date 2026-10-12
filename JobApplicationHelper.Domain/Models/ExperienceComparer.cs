namespace JobApplicationHelper.Domain.Models;

public static class ExperienceComparer
{
    public static bool AreEquivalent(Experience? first, Experience? second)
    {
        if (first is null || second is null)
        {
            return first is null && second is null;
        }

        return first.Title == second.Title
            && first.Type == second.Type
            && first.Organization == second.Organization
            && AreEquivalent(first.DateRange, second.DateRange)
            && first.Summary == second.Summary
            && AreEquivalentUnordered(first.Skills, second.Skills)
            && AreEquivalentUnordered(first.Evidence, second.Evidence)
            && AreEquivalentUnordered(first.Contexts, second.Contexts)
            && AreEquivalentUnordered(first.Links, second.Links)
            && first.Notes == second.Notes;
    }

    private static bool AreEquivalentUnordered(IReadOnlyList<string> first, IReadOnlyList<string> second)
    {
        return first.Count == second.Count
            && first.ToHashSet(StringComparer.OrdinalIgnoreCase).SetEquals(second);
    }

    private static bool AreEquivalent(DateRange first, DateRange second)
    {
        return AreEquivalent(first.Start, second.Start)
            && AreEquivalent(first.End, second.End);
    }

    private static bool AreEquivalent(PartialDate? first, PartialDate? second)
    {
        if (first is null || second is null)
        {
            return first is null && second is null;
        }

        return first.Year == second.Year
            && first.Month == second.Month
            && first.Day == second.Day;
    }
}
