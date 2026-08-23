namespace JobApplicationHelper.Models;

public record ApplicationFile(string CountryCode, bool IncludeCoverLetter, string CompanyName, string PositionTitle, string URL, string? City, string JobPosting);
