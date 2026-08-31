namespace JobApplicationHelper.Models;

public sealed class CoverLetterDraftParameters
{
    public string Cv { get; set; } = string.Empty;
    public string JobPosting { get; set; } = string.Empty;
    public string CandidateNotes { get; set; } = string.Empty;
    public string Tone { get; set; } = string.Empty;
    public string Style { get; set; } = string.Empty;
    public string TargetAudience { get; set; } = string.Empty;
    public JobRequirements Requirements { get; set; } = new JobRequirements { Requirements = [] };
    public int DesiredWordCount { get; set; } = 400;
}