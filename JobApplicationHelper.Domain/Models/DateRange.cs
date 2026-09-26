namespace JobApplicationHelper.Domain.Models;

public sealed class DateRange
{
    public PartialDate? Start { get; set; }

    public PartialDate? End { get; set; }

    public override string ToString()
    {
        var start = Start?.ToString();
        var end = End?.ToString();

        return (start, end) switch
        {
            (not null, not null) => $"{start} - {end}",
            (not null, null) => $"{start} - Present",
            (null, not null) => end!,
            _ => string.Empty
        };
    }
}
