using JobApplicationHelper.Contracts.BackgroundJobs;
using JobApplicationHelper.Domain.Models;
using System.Net.Http;
using System.Net.Http.Json;

namespace JobApplicationHelper.Services.Api;

public sealed class BackgroundJobsApiClient
{
    private readonly HttpClient _httpClient;

    public BackgroundJobsApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<GetBackgroundJobResponse> GetAsync(BackgroundJobId backgroundJobId, CancellationToken cancellationToken = default)
    {
        using var httpResponse = await _httpClient.GetAsync($"api/background-jobs/{backgroundJobId.Value}", cancellationToken);

        httpResponse.EnsureSuccessStatusCode();

        var response = await httpResponse.Content.ReadFromJsonAsync<GetBackgroundJobResponse>(cancellationToken)
            ?? throw new InvalidOperationException("The background jobs API returned an empty response.");

        return response;
    }
}