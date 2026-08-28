namespace JobApplicationHelper.Models;

public sealed class Experience
{
    public required string Id { get; set; }

    public required string Title { get; set; }

    public required ExperienceType Type { get; set; }

    public string Organization { get; set; } = string.Empty;

    public DateRange? DateRange { get; set; }

    public string Summary { get; set; } = string.Empty;

    public List<string> Skills { get; set; } = [];

    public List<string> Evidence { get; set; } = [];

    public List<string> Contexts { get; set; } = [];

    public List<string> Links { get; set; } = [];

    public string Notes { get; set; } = string.Empty;

    public bool MatchFilter(string filter)
    {
        return Id.Contains(filter, StringComparison.OrdinalIgnoreCase)
            || Title.Contains(filter, StringComparison.OrdinalIgnoreCase)
            || Organization.Contains(filter, StringComparison.OrdinalIgnoreCase)
            || Summary.Contains(filter, StringComparison.OrdinalIgnoreCase)
            || Skills.Any(s => s.Contains(filter, StringComparison.OrdinalIgnoreCase))
            || Evidence.Any(e => e.Contains(filter, StringComparison.OrdinalIgnoreCase))
            || Contexts.Any(c => c.Contains(filter, StringComparison.OrdinalIgnoreCase))
            || Notes.Contains(filter, StringComparison.OrdinalIgnoreCase);
    }
}