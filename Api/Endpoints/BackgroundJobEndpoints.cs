using JobApplicationHelper.Application.Services;
using JobApplicationHelper.Contracts.BackgroundJobs;
using JobApplicationHelper.Domain.Models;
using Microsoft.AspNetCore.Http.HttpResults;

namespace JobApplicationHelper.Api.Endpoints;

public static class BackgroundJobEndpoints
{
    public static IEndpointRouteBuilder MapBackgroundJobEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/background-jobs");

        group.MapGet("/{id:guid}", GetBackgroundJobAsync);

        return endpoints;
    }

    private static async Task<Results<Ok<GetBackgroundJobResponse>, NotFound>> GetBackgroundJobAsync(
        Guid id,
        IBackgroundJobService backgroundJobService,
        CancellationToken cancellationToken)
    {
        var job = await backgroundJobService.GetAsync(new BackgroundJobId(id), cancellationToken);

        if (job is null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(
            new GetBackgroundJobResponse(
                job.Id.Value,
                job.Type.ToString(),
                job.Priority.ToString(),
                job.Status.ToString(),
                job.CreatedAt,
                job.StartedAt,
                job.CompletedAt,
                job.Error));
    }
}