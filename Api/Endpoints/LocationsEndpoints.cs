using JobApplicationHelper.Application.Services;
using JobApplicationHelper.Contracts.Locations;

namespace JobApplicationHelper.Api.Endpoints;

public static class LocationsEndpoints
{
    public static IEndpointRouteBuilder MapLocationsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/locations");

        group.MapGet("", GetAllLocationsAsync);

        return endpoints;
    }

    private static async Task<IResult> GetAllLocationsAsync(
        LocationService locationService,
        CancellationToken cancellationToken)
    {
        var locations = locationService.GetLocations();

        var response = locations
            .Select(location => new LocationDto(location.CountryCode, location.CountryName))
            .ToList();

        return Results.Ok(response);
    }
}