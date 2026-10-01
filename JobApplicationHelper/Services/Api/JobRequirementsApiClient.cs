using JobApplicationHelper.Contracts.JobRequirements;
using JobApplicationHelper.Domain.Models;
using JobApplicationHelper.ApiMappings.ToDomain;
using System.Net.Http;
using System.Net.Http.Json;
using JobApplicationHelper.Services.Api.Exceptions;

namespace JobApplicationHelper.Services.Api;

public sealed class JobRequirementsApiClient
{
    private readonly HttpClient _httpClient;

    public JobRequirementsApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<BackgroundJobId> ExtractAsync(
        JobApplicationId jobApplicationId,
        CancellationToken cancellationToken = default)
    {
        var request = new ExtractJobRequirementsRequest(jobApplicationId.Value, BackgroundJobPriority.High.ToString());

        using var httpResponse = await _httpClient.PostAsJsonAsync(
            "api/job-requirements/extract",
            request,
            cancellationToken);

        httpResponse.EnsureSuccessStatusCode();

        var apiResponse = await httpResponse.Content.ReadFromJsonAsync<ExtractJobRequirementsResponse>(cancellationToken)
            ?? throw new InvalidOperationException("The job requirements API returned an empty response.");

        return new BackgroundJobId(apiResponse.BackgroundJobId);
    }

    public async Task<JobRequirements> GetAsync(
        JobApplicationId jobApplicationId,
        CancellationToken cancellationToken = default)
    {
        using var httpResponse = await _httpClient.GetAsync(
            $"api/job-requirements/{jobApplicationId.Value}",
            cancellationToken);
        if (httpResponse.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            throw new JobRequirementsNotFoundException($"No job requirements found for job application ID '{jobApplicationId.Value}'.");
        }
        httpResponse.EnsureSuccessStatusCode();
        var apiResponse = await httpResponse.Content.ReadFromJsonAsync<GetExtractedJobRequirementsResponse>(cancellationToken)
            ?? throw new InvalidOperationException("The job requirements API returned an empty response.");
        
        return new JobRequirements
        {
            Requirements = apiResponse.Requirements
                .Select(r => r.ToDomain())
                .ToList()
        };
    }
}