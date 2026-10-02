using JobApplicationHelper.Domain.Models;
using System.Net.Http;
using System.Net.Http.Json;
using JobApplicationHelper.ApiMappings.ToDto;
using JobApplicationHelper.ApiMappings.ToDomain;
using JobApplicationHelper.Contracts.CoverLetters;
using JobApplicationHelper.Services.Api.Exceptions;

namespace JobApplicationHelper.Services.Api;

public sealed class CoverLettersApiClient
{
    private readonly HttpClient _httpClient;

    public CoverLettersApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<BackgroundJobId> GenerateCoverLetterAsync(
        JobApplicationId jobApplicationId,
        CoverLetterDraftParameters draftParameters,
        CancellationToken cancellationToken = default)
    {
        var request = new GenerateCoverLetterRequest(jobApplicationId.Value, BackgroundJobPriority.High.ToString(), draftParameters.ToDto());

        using var httpResponse = await _httpClient.PostAsJsonAsync(
            "api/cover-letters",
            request,
            cancellationToken);

        httpResponse.EnsureSuccessStatusCode();

        var apiResponse = await httpResponse.Content.ReadFromJsonAsync<GenerateCoverLetterResponse>(cancellationToken)
            ?? throw new InvalidOperationException("The cover letter API returned an empty response.");

        return new BackgroundJobId(apiResponse.BackgroundJobId);
    }

    public async Task<string> GetCoverLetterDraftAsync(
        JobApplicationId jobApplicationId,
        CancellationToken cancellationToken = default)
    {
        using var httpResponse = await _httpClient.GetAsync(
            $"api/cover-letters/{jobApplicationId.Value}",
            cancellationToken);
        if (httpResponse.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            throw new DraftNotFoundException($"No cover letter draft found for job application ID '{jobApplicationId.Value}'.");
        }
        httpResponse.EnsureSuccessStatusCode();
        var apiResponse = await httpResponse.Content.ReadFromJsonAsync<GetCoverLetterDraftResponse>(cancellationToken)
            ?? throw new InvalidOperationException("The cover letter API returned an empty response.");
        return apiResponse.Draft;
    }

    public async Task<BackgroundJobId> VerifyDraftAsync(
        JobApplicationId jobApplicationId,
        CoverLetterDraftParameters draftParameters,
        string draft,
        CancellationToken cancellationToken = default)
    {
        var request = new VerifyCoverLetterRequest(jobApplicationId.Value, BackgroundJobPriority.High.ToString(), draftParameters.ToDto(), draft);

        using var httpResponse = await _httpClient.PostAsJsonAsync(
            "api/cover-letters/verify",
            request,
            cancellationToken);

        httpResponse.EnsureSuccessStatusCode();

        var apiResponse = await httpResponse.Content.ReadFromJsonAsync<VerifyCoverLetterResponse>(cancellationToken)
            ?? throw new InvalidOperationException("The verify cover letter API returned an empty response.");

        return new BackgroundJobId(apiResponse.BackgroundJobId);
    }

    public async Task<VerificationResult> GetVerifyCoverLetterResultAsync(
        JobApplicationId jobApplicationId,
        CancellationToken cancellationToken = default)
    {
        using var httpResponse = await _httpClient.GetAsync(
            $"api/cover-letters/verify/{jobApplicationId.Value}",
            cancellationToken);
        if (httpResponse.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            throw new VerificationResultNotFoundException($"No verification result found for job application ID '{jobApplicationId.Value}'.");
        }
        httpResponse.EnsureSuccessStatusCode();
        var apiResponse = await httpResponse.Content.ReadFromJsonAsync<GetVerifyCoverLetterResponse>(cancellationToken)
            ?? throw new InvalidOperationException("The verify cover letter API returned an empty response.");
        return apiResponse.ToDomain();
    }
}
