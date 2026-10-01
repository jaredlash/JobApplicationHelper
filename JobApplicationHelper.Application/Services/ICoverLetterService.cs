using JobApplicationHelper.Domain.Models;

namespace JobApplicationHelper.Application.Services
{
    public interface ICoverLetterService
    {
        Task<string> GenerateCoverLetterAsync(CoverLetterDraftParameters draftParameters, CancellationToken cancellationToken = default);
        Task<VerificationResult> VerifyDraftAsync(CoverLetterDraftParameters draftParameters, string draft, CancellationToken cancellationToken = default);
    }
}