namespace JobApplicationHelper.Infrastructure.Persistence.Entities;

public sealed class ExtractedJobRequirementsEntity
{
    public Guid JobApplicationId { get; set; }

    public string RequirementsJson { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
}