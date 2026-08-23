namespace JobApplicationHelper.Models;

public sealed class Experience
{
    public required string Id { get; set; }

    public required string Title { get; set; }

    public required ExperienceType Type { get; set; }

    public string? Organization { get; set; }

    public DateRange? DateRange { get; set; }

    public string? Summary { get; set; }

    public List<string> Skills { get; set; } = [];

    public List<string> Evidence { get; set; } = [];

    public List<string> Contexts { get; set; } = [];

    public List<string> Links { get; set; } = [];

    public string? Notes { get; set; }
}