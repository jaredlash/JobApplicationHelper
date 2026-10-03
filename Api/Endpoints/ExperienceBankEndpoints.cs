using JobApplicationHelper.Application.Services;
using JobApplicationHelper.ApiMappings.ToDto;
using Microsoft.AspNetCore.Http.HttpResults;
using JobApplicationHelper.Contracts.Experiences;

namespace JobApplicationHelper.Api.Endpoints;

public static class ExperienceBankEndpoints
{
    public static IEndpointRouteBuilder MapExperienceBankEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/experiences");

        group.MapGet("", GetAllExperiencesAsync);

        return endpoints;
    }

    private static async Task<Ok<List<ExperienceDto>>> GetAllExperiencesAsync(
        IExperienceBankService experienceBankService,
        CancellationToken cancellationToken)
    {
        var experiences = await experienceBankService.GetAllAsync(cancellationToken);

        var response = experiences
            .Select(e => e.ToDto())
            .ToList();

        return TypedResults.Ok(response);
    }
}