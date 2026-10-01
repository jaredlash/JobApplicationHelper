namespace JobApplicationHelper.Infrastructure.Persistence.Entities;

public sealed class VerifyCoverLetterResultEntity
{
    public Guid JobApplicationId { get; set; }

    public string VerificationResultJson { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
}