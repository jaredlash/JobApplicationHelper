using JobApplicationHelper.Application.Repositories;
using JobApplicationHelper.Application.Services;
using JobApplicationHelper.Contracts.JobRequirements;
using JobApplicationHelper.Domain.Models;
using Microsoft.AspNetCore.Http.HttpResults;

namespace JobApplicationHelper.Api.Endpoints;

public static class JobRequirementsEndpoints
{
    public static IEndpointRouteBuilder MapJobRequirementsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/job-requirements");

        group.MapPost("/extract", ExtractJobRequirementsAsync);
        group.MapGet("/{jobApplicationId:guid}", GetExtractedJobRequirementsAsync);

        return endpoints;
    }

    private static async Task<Results<Ok<ExtractJobRequirementsResponse>, BadRequest<string>>> ExtractJobRequirementsAsync(
        ExtractJobRequirementsRequest request,
        IBackgroundJobService backgroundJobService,
        CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<BackgroundJobPriority>(request.Priority, ignoreCase: true, out var priority))
        {
            return TypedResults.BadRequest($"Invalid background job priority '{request.Priority}'.");
        }

        var backgroundJobId = await backgroundJobService.CreateAsync(
            BackgroundJobType.ExtractJobRequirements,
            priority,
            new JobApplicationId(request.JobApplicationId),
            cancellationToken: cancellationToken);

        return TypedResults.Ok(new ExtractJobRequirementsResponse(backgroundJobId.Value));
    }

    private static async Task<Results<Ok<GetExtractedJobRequirementsResponse>, NotFound>> GetExtractedJobRequirementsAsync(
        Guid jobApplicationId,
        IExtractedJobRequirementsRepository repository,
        CancellationToken cancellationToken)
    {
        var requirements = await repository.GetAsync(new JobApplicationId(jobApplicationId), cancellationToken);

        if (requirements is null)
        {
            return TypedResults.NotFound();
        }

        var response = new GetExtractedJobRequirementsResponse(
            requirements.JobApplicationId.Value,
            requirements.Requirements.Requirements
                .Select(x => new JobRequirementDto(
                    x.Requirement,
                    x.Category.ToString(),
                    x.Priority.ToString()))
                .ToList());

        return TypedResults.Ok(response);
    }
}