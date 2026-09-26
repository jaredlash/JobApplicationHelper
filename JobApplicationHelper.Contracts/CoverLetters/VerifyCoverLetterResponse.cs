namespace JobApplicationHelper.Contracts.CoverLetters;

public sealed record VerifyCoverLetterResponse(
    IReadOnlyList<string> Errors,
    IReadOnlyList<string> UnsupportedClaims,
    IReadOnlyList<string> StyleViolations,
    IReadOnlyList<string> RequiredCorrections);