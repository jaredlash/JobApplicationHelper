using JobApplicationHelper.ApiMappings.ToDomain;
using JobApplicationHelper.Contracts.Experiences;
using JobApplicationHelper.Domain.Models;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace JobApplicationHelper.Services.Api;

public sealed class ExperiencesApiClient
{
    private readonly HttpClient _httpClient;

    public ExperiencesApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<Experience>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        using var httpResponse = await _httpClient.GetAsync("api/experiences", cancellationToken);

        httpResponse.EnsureSuccessStatusCode();


        var experiences = await ReadRequiredJsonAsync<List<ExperienceDto>>(httpResponse.Content,
            "The experience API returned an empty response.", cancellationToken);
        
        return experiences.Select(e => e.ToDomain()).ToList();
    }

    public async Task<Experience?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var httpResponse = await _httpClient.GetAsync($"api/experiences/{id}", cancellationToken);

        if (httpResponse.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        httpResponse.EnsureSuccessStatusCode();

        var experience = await ReadRequiredJsonAsync<ExperienceDto>(
            httpResponse.Content,
            "The experience API returned an empty response.",
            cancellationToken);

        return experience.ToDomain();
    }

    public async Task UpsertAsync(Guid id, UpsertExperienceRequest request, CancellationToken cancellationToken = default)
    {
        using var httpResponse = await _httpClient.PutAsJsonAsync($"api/experiences/{id}", request, cancellationToken);

        httpResponse.EnsureSuccessStatusCode();
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var httpResponse = await _httpClient.DeleteAsync($"api/experiences/{id}", cancellationToken);

        httpResponse.EnsureSuccessStatusCode();
    }

    public async Task<ImportExperiencesResponse> ImportAsync(Stream stream, string fileName, CancellationToken cancellationToken = default)
    {
        using var multipartContent = new MultipartFormDataContent();

        using var fileContent = new StreamContent(stream);

        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/x-yaml");

        multipartContent.Add(fileContent, "file", fileName);

        using var httpResponse = await _httpClient.PostAsync("api/experiences/import", multipartContent, cancellationToken);

        httpResponse.EnsureSuccessStatusCode();

        return await ReadRequiredJsonAsync<ImportExperiencesResponse>(
            httpResponse.Content,
            "The experience import API returned an empty response.",
            cancellationToken);
    }

    private static async Task<T> ReadRequiredJsonAsync<T>(HttpContent content, string errorMessage, CancellationToken cancellationToken)
    {
        try
        {
            return await content.ReadFromJsonAsync<T>(cancellationToken)
                ?? throw new InvalidOperationException(errorMessage);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException(errorMessage, ex);
        }
    }
}