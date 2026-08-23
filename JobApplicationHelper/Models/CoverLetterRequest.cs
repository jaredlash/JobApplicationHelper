namespace JobApplicationHelper.Models;

public sealed record CoverLetterRequest(
    string Cv,
    string JobPosting,
    string CandidateNotes,
    string Tone,
    string Style,
    string TargetAudience,
    ExperienceBank? ExperienceBank = null, // TODO: to be made non-nullable in the future
    int DesiredWordCount = 400);