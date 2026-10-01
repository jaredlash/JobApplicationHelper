using JobApplicationHelper.Domain.Models;

// TODO: Not a record because of the CreatedAt property, but consider changing it to a record
public sealed class CoverLetterDraft
{
    private CoverLetterDraft()
    {
    }

    private CoverLetterDraft(JobApplicationId jobApplicationId, string draft, DateTime createdAt)
    {
        JobApplicationId = jobApplicationId;
        Draft = draft;
        CreatedAt = createdAt;
    }

    public JobApplicationId JobApplicationId { get; private set; }

    public string Draft { get; private set; } = string.Empty;

    public DateTime CreatedAt { get; private set; }

    public static CoverLetterDraft Create(JobApplicationId jobApplicationId, string draft)
    {
        return new CoverLetterDraft(jobApplicationId, draft, DateTime.UtcNow);
    }
}