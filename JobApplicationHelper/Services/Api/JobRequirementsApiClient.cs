using JobApplicationHelper.Contracts.JobRequirements;
using JobApplicationHelper.Domain.Models;
using System.Net.Http;
using System.Net.Http.Json;

namespace JobApplicationHelper.Services.Api;

public sealed class JobRequirementsApiClient
{
    private readonly HttpClient _httpClient;

    public JobRequirementsApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<JobRequirements> ExtractAsync(
        string jobPosting,
        CancellationToken cancellationToken = default)
    {
        var request = new ExtractJobRequirementsRequest(jobPosting);

        using var httpResponse = await _httpClient.PostAsJsonAsync(
            "api/job-requirements",
            request,
            cancellationToken);

        httpResponse.EnsureSuccessStatusCode();

        var apiResponse = await httpResponse.Content.ReadFromJsonAsync<ExtractJobRequirementsResponse>(cancellationToken)
            ?? throw new InvalidOperationException("The job requirements API returned an empty response.");

        return new JobRequirements
        {
            Requirements = apiResponse.Requirements
                .Select(r => new JobRequirement
                {
                    Requirement = r.Requirement,
                    Category = Enum.Parse<RequirementCategory>(
                        r.Category,
                        ignoreCase: true),
                    Priority = Enum.Parse<RequirementPriority>(
                        r.Priority,
                        ignoreCase: true)
                })
                .ToList()
        };
    }
}