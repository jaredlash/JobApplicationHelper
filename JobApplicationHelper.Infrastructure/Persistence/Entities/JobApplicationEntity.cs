namespace JobApplicationHelper.Infrastructure.Persistence.Entities;

public sealed class JobApplicationEntity
{
    public Guid Id { get; set; }

    public string CountryCode { get; set; } = string.Empty;
    public bool IncludeCoverLetter { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string PositionTitle { get; set; } = string.Empty;
    public string URL { get; set; } = string.Empty;
    public string? City { get; set; }
    public string JobPosting { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public string ApplicationFolder { get; set; } = string.Empty;
}