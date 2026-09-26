namespace JobApplicationHelper.Contracts.JobApplications;

public record CreateJobApplicationRequest(string CountryCode, bool IncludeCoverLetter, string CompanyName, string PositionTitle, string URL, string? City, string JobPosting);
