namespace JobApplicationHelper.Domain.Models;

public sealed class JobApplication
{
    public required JobApplicationId Id { get; init; }

    public required string CountryCode { get; init; }

    public bool IncludeCoverLetter { get; init; }

    public required string CompanyName { get; init; }

    public required string PositionTitle { get; init; }

    public required string URL { get; init; }

    public string? City { get; init; }

    public required string JobPosting { get; init; }

    public DateTime CreatedAt { get; init; }

    public required string ApplicationFolder { get; init; }
}