using JobApplicationHelper.Domain.Models;

namespace JobApplicationHelper.Application.Services;

public interface ICandidateContentProvider
{
    Task<CandidateContent> GetAsync(string countryCode, CancellationToken cancellationToken = default);
}
