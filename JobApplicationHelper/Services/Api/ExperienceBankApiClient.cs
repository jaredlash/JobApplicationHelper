using JobApplicationHelper.ApiMappings.ToDomain;
using JobApplicationHelper.Contracts.Experiences;
using JobApplicationHelper.Domain.Models;
using System.Net.Http;
using System.Net.Http.Json;

namespace JobApplicationHelper.Services.Api;

public sealed class ExperienceBankApiClient
{
    private readonly HttpClient _httpClient;

    public ExperienceBankApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<Experience>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        using var httpResponse = await _httpClient.GetAsync("api/experiences", cancellationToken);

        httpResponse.EnsureSuccessStatusCode();

        var experiences = await httpResponse.Content.ReadFromJsonAsync<List<ExperienceDto>>(cancellationToken)
            ?? throw new InvalidOperationException("The experience API returned an empty response.");

        return experiences.Select(e => e.ToDomain()).ToList();
    }
}