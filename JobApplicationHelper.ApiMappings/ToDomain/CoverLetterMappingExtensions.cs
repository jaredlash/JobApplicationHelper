using JobApplicationHelper.Contracts.CoverLetters;
using JobApplicationHelper.Domain.Models;

namespace JobApplicationHelper.ApiMappings.ToDomain;

public static class CoverLetterMappingExtensions
{
    public static CoverLetterDraftParameters ToDomain(this CoverLetterDraftParametersDto coverLetterDraftParametersDto)
    {
        return new CoverLetterDraftParameters
        {
            CountryCode = coverLetterDraftParametersDto.CountryCode,
            JobPosting = coverLetterDraftParametersDto.JobPosting,
            CandidateNotes = coverLetterDraftParametersDto.CandidateNotes,
            Tone = coverLetterDraftParametersDto.Tone,
            Style = coverLetterDraftParametersDto.Style,
            TargetAudience = coverLetterDraftParametersDto.TargetAudience,
            Requirements = new JobRequirements
            {
                Requirements = coverLetterDraftParametersDto.Requirements.Requirements.Select(r => r.ToDomain()).ToList()
            },
            DesiredWordCount = coverLetterDraftParametersDto.DesiredWordCount
        };
    }

    public static VerificationResult ToDomain(this GetVerifyCoverLetterResponse verifyCoverLetterResponse)
    {
        return new VerificationResult
        { 
            Errors = verifyCoverLetterResponse.Errors.ToList(),
            UnsupportedClaims = verifyCoverLetterResponse.UnsupportedClaims.ToList(),
            StyleViolations = verifyCoverLetterResponse.StyleViolations.ToList(),
            RequiredCorrections = verifyCoverLetterResponse.RequiredCorrections.ToList()
        };
    }
}
