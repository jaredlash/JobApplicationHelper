namespace JobApplicationHelper.Domain.Models;

public sealed class VerifyCoverLetterResult
{
    public JobApplicationId JobApplicationId { get; private set; }

    public VerificationResult VerificationResult { get; private set; } = null!;

    public DateTime CreatedAt { get; private set; }

    private VerifyCoverLetterResult()
    { 
    }

    public static VerifyCoverLetterResult Create(
        JobApplicationId jobApplicationId,
        VerificationResult verificationResult)
    {
        ArgumentNullException.ThrowIfNull(verificationResult);
        return new VerifyCoverLetterResult
        {
            JobApplicationId = jobApplicationId,
            VerificationResult = verificationResult,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static VerifyCoverLetterResult Rehydrate(
        JobApplicationId jobApplicationId,
        VerificationResult verificationResult,
        DateTime createdAt)
    {
        ArgumentNullException.ThrowIfNull(verificationResult);
        return new VerifyCoverLetterResult
        {
            JobApplicationId = jobApplicationId,
            VerificationResult = verificationResult,
            CreatedAt = createdAt
        };
    }
}
