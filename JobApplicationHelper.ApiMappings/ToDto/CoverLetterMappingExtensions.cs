using JobApplicationHelper.Contracts.CoverLetters;
using JobApplicationHelper.Domain.Models;

namespace JobApplicationHelper.ApiMappings.ToDto;

public static class CoverLetterMappingExtensions
{
    public static CoverLetterDraftParametersDto ToDto(this CoverLetterDraftParameters coverLetterDraftParameters)
    {
        return new CoverLetterDraftParametersDto(
            coverLetterDraftParameters.CountryCode,
            coverLetterDraftParameters.JobPosting,
            coverLetterDraftParameters.CandidateNotes,
            coverLetterDraftParameters.Tone,
            coverLetterDraftParameters.Style,
            coverLetterDraftParameters.TargetAudience,
            coverLetterDraftParameters.Requirements.ToDto(),
            coverLetterDraftParameters.DesiredWordCount);
    }

    public static VerifyCoverLetterResponse ToDto(this VerificationResult verificationResult)
    {
        return new VerifyCoverLetterResponse(
            verificationResult.Errors,
            verificationResult.UnsupportedClaims,
            verificationResult.StyleViolations,
            verificationResult.RequiredCorrections);
    }
}
