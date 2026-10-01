namespace JobApplicationHelper.Infrastructure.Persistence.Entities;

public sealed class CoverLetterDraftEntity
{
    public Guid JobApplicationId { get; set; }

    public string Draft { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
}