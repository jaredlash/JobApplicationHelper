using System.Globalization;
using JobApplicationHelper.Domain.Models;

public static class PartialDateParser
{
    public static bool TryParse(string? input, out PartialDate? result)
    {
        result = null;

        if (string.IsNullOrWhiteSpace(input))
            return true;

        var parts = input.Trim().Split('-');

        if (parts.Length is < 1 or > 3 ||
            !int.TryParse(parts[0], out var year) ||
            year < 1)
        {
            return false;
        }

        if (parts.Length == 1)
        {
            result = new PartialDate { Year = year };
            return true;
        }

        if (!int.TryParse(parts[1], out var month) ||
            month is < 1 or > 12)
        {
            return false;
        }

        if (parts.Length == 2)
        {
            result = new PartialDate
            {
                Year = year,
                Month = month
            };

            return true;
        }

        if (!int.TryParse(parts[2], out var day))
            return false;

        try
        {
            _ = new DateTime(year, month, day);
        }
        catch (ArgumentOutOfRangeException)
        {
            return false;
        }

        result = new PartialDate
        {
            Year = year,
            Month = month,
            Day = day
        };

        return true;
    }

    public static string FormatForEditing(PartialDate? date)
    {
        if (date is null)
            return string.Empty;

        if (date.Month is null)
            return date.Year.ToString(CultureInfo.InvariantCulture);

        if (date.Day is null)
        {
            return $"{date.Year:D4}-{date.Month:D2}";
        }

        return $"{date.Year:D4}-{date.Month:D2}-{date.Day:D2}";
    }
}