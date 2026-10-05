using JobApplicationHelper.ApiMappings.ToDto;
using JobApplicationHelper.ApiMappings.ToDomain;
using JobApplicationHelper.Application.Services;
using JobApplicationHelper.Contracts.Experiences;
using JobApplicationHelper.Domain.Models;
using Microsoft.AspNetCore.Http.HttpResults;

namespace JobApplicationHelper.Api.Endpoints;

public static class ExperienceEndpoints
{
    public static IEndpointRouteBuilder MapExperienceEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/experiences");

        group.MapGet("/", GetAllExperiencesAsync);
        group.MapGet("/{id:guid}", GetExperienceAsync);
        group.MapPut("/{id:guid}", UpsertExperienceAsync);
        group.MapDelete("/{id:guid}", DeleteExperienceAsync);

        return endpoints;
    }

    private static async Task<Ok<List<ExperienceDto>>> GetAllExperiencesAsync(
        IExperienceService experienceService,
        CancellationToken cancellationToken)
    {
        var experiences = await experienceService.GetAllAsync(cancellationToken);

        var response = experiences
            .Select(e => e.ToDto())
            .ToList();

        return TypedResults.Ok(response);
    }

    private static async Task<Results<Ok<ExperienceDto>, NotFound>> GetExperienceAsync(
        Guid id,
        IExperienceService experienceService,
        CancellationToken cancellationToken)
    {
        var experience = await experienceService.GetAsync(
            new ExperienceId(id),
            cancellationToken);

        return experience is null
            ? TypedResults.NotFound()
            : TypedResults.Ok(experience.ToDto());
    }

    private static async Task<NoContent> UpsertExperienceAsync(
        Guid id,
        UpsertExperienceRequest request,
        IExperienceService experienceService,
        CancellationToken cancellationToken)
    {
        var experience = request.ToDomain(id);

        await experienceService.AddOrReplaceAsync(experience, cancellationToken);

        return TypedResults.NoContent();
    }

    private static async Task<NoContent> DeleteExperienceAsync(
        Guid id,
        IExperienceService experienceService,
        CancellationToken cancellationToken)
    {
        await experienceService.DeleteAsync(new ExperienceId(id), cancellationToken);

        return TypedResults.NoContent();
    }
}