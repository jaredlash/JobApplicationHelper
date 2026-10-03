using JobApplicationHelper.Application.Services;
using JobApplicationHelper.ApiMappings.ToDto;

namespace JobApplicationHelper.Api.Endpoints;

public static class ExperienceBankEndpoints
{
    public static IEndpointRouteBuilder MapExperienceBankEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/experiences");

        group.MapGet("", GetAllExperiencesAsync);

        return endpoints;
    }

    private static async Task<IResult> GetAllExperiencesAsync(
        IExperienceBankService experienceBankService,
        CancellationToken cancellationToken)
    {
        var experiences = await experienceBankService.GetAllAsync(cancellationToken);

        var response = experiences
            .Select(e => e.ToDto())
            .ToList();

        return Results.Ok(response);
    }
}