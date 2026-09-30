using JobApplicationHelper.Application.Services;
using JobApplicationHelper.Contracts.JobRequirements;
using JobApplicationHelper.Domain.Models;

namespace JobApplicationHelper.Api.Endpoints;

public static class JobRequirementsEndpoints
{
    public static IEndpointRouteBuilder MapJobRequirementsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/job-requirements");

        group.MapPost("/extract", ExtractJobRequirementsAsync);

        return endpoints;
    }

    private static async Task<IResult> ExtractJobRequirementsAsync(
        ExtractJobRequirementsRequest request,
        IBackgroundJobService backgroundJobService,
        CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<BackgroundJobPriority>(request.Priority, ignoreCase: true, out var priority))
        {
            return Results.BadRequest($"Invalid background job priority '{request.Priority}'.");
        }

        var backgroundJobId = await backgroundJobService.CreateAsync(
            BackgroundJobType.ExtractJobRequirements,
            priority,
            new JobApplicationId(request.JobApplicationId),
            cancellationToken: cancellationToken);

        return Results.Ok(new ExtractJobRequirementsResponse(backgroundJobId.Value));
    }
}