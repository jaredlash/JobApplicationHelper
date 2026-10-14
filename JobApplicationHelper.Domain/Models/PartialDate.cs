namespace JobApplicationHelper.Domain.Models;

public sealed class PartialDate
{
    public PartialDate()
    {
    }

    public PartialDate(int year)
    {
        Year = year;
    }

    public PartialDate(int year, int month)
    {
        Year = year;
        Month = month;
    }

    public PartialDate(int year, int month, int day)
    {
        Year = year;
        Month = month;
        Day = day;
    }

    public int Year { get; set; }

    public int? Month { get; set; }

    public int? Day { get; set; }

    public override string ToString()
    {
        if (Month is null)
            return Year.ToString();

        var month = new DateTime(Year, Month.Value, 1).ToString("MMM");

        return Day is null
            ? $"{month} {Year}"
            : $"{month} {Day.Value}, {Year}";
    }
}