using JobApplicationHelper.Contracts.JobApplications;
using JobApplicationHelper.ApiMappings.ToDto;
using JobApplicationHelper.Domain.Models;
using System.Net.Http;
using System.Net.Http.Json;

namespace JobApplicationHelper.Services.Api;

public sealed class JobApplicationsApiClient
{
    private readonly HttpClient _httpClient;

    public JobApplicationsApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<JobApplicationId> CreateJobApplicationAsync(ApplicationFile application,
        CancellationToken cancellationToken = default)
    {
        var request = application.ToDto();
        using var httpResponse = await _httpClient.PostAsJsonAsync(
            "api/job-applications",
            request,
            cancellationToken);

        httpResponse.EnsureSuccessStatusCode();

        var response =
            await httpResponse.Content.ReadFromJsonAsync<CreateJobApplicationResponse>(cancellationToken)
            ?? throw new InvalidOperationException("The job application API returned an empty response.");

        return new JobApplicationId(response.JobApplicationId);
    }

    public async Task<string> GetApplicationFolderAsync(JobApplicationId jobApplicationId,
        CancellationToken cancellationToken = default)
    {
        var applicationId = Uri.EscapeDataString(jobApplicationId.Value);

        using var httpResponse = await _httpClient.GetAsync(
            $"api/job-applications/{applicationId}/folder",
            cancellationToken);

        httpResponse.EnsureSuccessStatusCode();

        var applicationFolder =
            await httpResponse.Content.ReadFromJsonAsync<ApplicationFolderResponse>(cancellationToken)
            ?? throw new InvalidOperationException("The job application folder API returned an empty response.");

        return applicationFolder.Path;
    }

    public async Task SaveCoverLetter(JobApplicationId jobApplicationId,
        string coverLetter,
        CancellationToken cancellationToken = default)
    {
        var applicationId = Uri.EscapeDataString(jobApplicationId.Value);

        using var httpResponse = await _httpClient.PutAsJsonAsync(
            $"api/job-applications/{applicationId}/cover-letter",
            new SaveCoverLetterRequest(coverLetter),
            cancellationToken);

        httpResponse.EnsureSuccessStatusCode();
    }
}