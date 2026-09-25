using JobApplicationHelper.Contracts.Locations;
using JobApplicationHelper.Domain.Models;
using System.Net.Http;
using System.Net.Http.Json;

namespace JobApplicationHelper.Services.Api;

public sealed class LocationsApiClient
{
    private readonly HttpClient _httpClient;

    public LocationsApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<Location>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        using var httpResponse = await _httpClient.GetAsync(
            "api/locations",
            cancellationToken);

        httpResponse.EnsureSuccessStatusCode();

        var locations =
            await httpResponse.Content.ReadFromJsonAsync<List<LocationDto>>(
                cancellationToken)
            ?? throw new InvalidOperationException(
                "The locations API returned an empty response.");

        return locations
            .Select(location => new Location(location.CountryCode, location.CountryName))
            .ToList();
    }
}