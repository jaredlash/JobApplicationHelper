namespace JobApplicationHelper.Contracts.CoverLetters;

public sealed record CoverLetterDraftParametersDto(
    string CountryCode,
    string JobPosting,
    string CandidateNotes,
    string Tone,
    string Style,
    string TargetAudience,
    JobRequirementsDto Requirements,
    int DesiredWordCount);