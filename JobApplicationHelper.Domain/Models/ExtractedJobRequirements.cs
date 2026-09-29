namespace JobApplicationHelper.Domain.Models;

public sealed class ExtractedJobRequirements
{
    public JobApplicationId JobApplicationId { get; private set; }

    public JobRequirements Requirements { get; private set; } = null!;

    public DateTime CreatedAt { get; private set; }

    private ExtractedJobRequirements()
    {
    }

    public static ExtractedJobRequirements Create(
        JobApplicationId jobApplicationId,
        JobRequirements requirements)
    {
        ArgumentNullException.ThrowIfNull(requirements);

        return new ExtractedJobRequirements
        {
            JobApplicationId = jobApplicationId,
            Requirements = requirements,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static ExtractedJobRequirements Rehydrate(
        JobApplicationId jobApplicationId,
        JobRequirements requirements,
        DateTime createdAt)
    {
        ArgumentNullException.ThrowIfNull(requirements);

        return new ExtractedJobRequirements
        {
            JobApplicationId = jobApplicationId,
            Requirements = requirements,
            CreatedAt = createdAt
        };
    }
}