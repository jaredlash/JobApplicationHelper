namespace JobApplicationHelper.Contracts.CoverLetters;

public sealed record GetVerifyCoverLetterResponse(
    IReadOnlyList<string> Errors,
    IReadOnlyList<string> UnsupportedClaims,
    IReadOnlyList<string> StyleViolations,
    IReadOnlyList<string> RequiredCorrections);