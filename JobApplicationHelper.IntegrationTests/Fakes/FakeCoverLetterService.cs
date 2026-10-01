using JobApplicationHelper.Application.Services;
using JobApplicationHelper.Domain.Models;

namespace JobApplicationHelper.IntegrationTests.Fakes;

internal class FakeCoverLetterService : ICoverLetterService
{
    public Task<string> GenerateCoverLetterAsync(CoverLetterDraftParameters draftParameters, CancellationToken cancellationToken = default)
    {
        if (draftParameters.Requirements.Requirements.Count == 0)
            throw new InvalidOperationException("Job requirements are missing.");

        var firstRequirement = draftParameters.Requirements.Requirements[0];
        var draftTestString = $"{firstRequirement.Requirement}-{firstRequirement.Category.ToString()}-{firstRequirement.Priority.ToString()}";

        return Task.FromResult(draftTestString);
    }

    public Task<VerificationResult> VerifyDraftAsync(CoverLetterDraftParameters draftParameters, string draft, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
