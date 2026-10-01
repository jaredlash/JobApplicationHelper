using JobApplicationHelper.Domain.Models;
using System.Net.Http;
using System.Net.Http.Json;
using JobApplicationHelper.ApiMappings.ToDto;
using JobApplicationHelper.ApiMappings.ToDomain;
using JobApplicationHelper.Contracts.CoverLetters;

namespace JobApplicationHelper.Services.Api;

public sealed class CoverLettersApiClient
{
    private readonly HttpClient _httpClient;

    public CoverLettersApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> GenerateCoverLetterAsync(
        CoverLetterDraftParameters draftParameters,
        CancellationToken cancellationToken = default)
    {
        var request = new GenerateCoverLetterRequest(Guid.NewGuid(), "High", draftParameters.ToDto());

        using var httpResponse = await _httpClient.PostAsJsonAsync(
            "api/cover-letters",
            request,
            cancellationToken);

        httpResponse.EnsureSuccessStatusCode();

        var apiResponse = await httpResponse.Content.ReadFromJsonAsync<GenerateCoverLetterResponse>(cancellationToken)
            ?? throw new InvalidOperationException("The cover letter API returned an empty response.");

        return apiResponse.BackgroundJobId.ToString();
    }

    public async Task<VerificationResult> VerifyDraftAsync(
        CoverLetterDraftParameters draftParameters,
        string draft,
        CancellationToken cancellationToken = default)
    {
        var request = new VerifyCoverLetterRequest(Guid.NewGuid(), "High", draftParameters.ToDto(), draft);

        using var httpResponse = await _httpClient.PostAsJsonAsync(
            "api/cover-letters/verify",
            request,
            cancellationToken);

        httpResponse.EnsureSuccessStatusCode();

        var apiResponse = await httpResponse.Content.ReadFromJsonAsync<GetVerifyCoverLetterResponse>(cancellationToken)
            ?? throw new InvalidOperationException("The cover letter API returned an empty response.");

        return apiResponse.ToDomain();
    }
}