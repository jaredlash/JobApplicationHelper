using JobApplicationHelper.ApiMappings.ToDomain;
using JobApplicationHelper.ApiMappings.ToDto;
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
        group.MapPut("/{id:guid}", AddOrReplaceExperienceAsync);
        group.MapDelete("/{id:guid}", DeleteExperienceAsync);

        group.MapGet("/suggestions/contexts", GetContextSuggestions);
        group.MapGet("/suggestions/skills", GetSkillSuggestions);

        group.MapPost("/import", ImportExperiencesAsync)
            .DisableAntiforgery();

        group.MapGet("pending-imports/count", GetPendingExperienceImportCountAsync);
        group.MapGet("/pending-imports/next", GetNextPendingExperienceImportAsync);

        group.MapPost("/pending-imports/{id:guid}/resolve", ResolvePendingExperienceImportAsync);

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

    private static async Task<NoContent> AddOrReplaceExperienceAsync(
        Guid id,
        ExperienceDto experienceDto,
        IExperienceService experienceService,
        CancellationToken cancellationToken)
    {
        var experience = experienceDto.ToDomain();

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

    private static async Task<Ok<CompletionSuggestionsResponse>> GetContextSuggestions(
        IExperienceService experienceService,
        CancellationToken cancellationToken)
    {
        var suggestions = await experienceService.GetContextSuggestions(cancellationToken);

        return TypedResults.Ok(new CompletionSuggestionsResponse(suggestions));
    }

    private static async Task<Ok<CompletionSuggestionsResponse>> GetSkillSuggestions(
        IExperienceService experienceService,
        CancellationToken cancellationToken)
    {
        var suggestions = await experienceService.GetSkillSuggestions(cancellationToken);

        return TypedResults.Ok(new CompletionSuggestionsResponse(suggestions));
    }

    private static async Task<Results<Ok<ImportExperiencesResponse>, BadRequest<string>>> ImportExperiencesAsync(
        IFormFile file,
        IExperienceImporter importService,
        IExperienceImportService experienceImportService,
        CancellationToken cancellationToken)
    {
        if (file.Length == 0)
        {
            return TypedResults.BadRequest("The uploaded experience bank is empty.");
        }

        try
        {
            await using var stream = file.OpenReadStream();

            var import = await importService.ImportAsync(
                stream,
                file.FileName,
                cancellationToken);
            
            var result = await experienceImportService.ImportAsync(
                import,
                cancellationToken);
            
            var response = new ImportExperiencesResponse(
                result.AddedCount,
                result.SkippedCount,
                result.PendingCount);

            return TypedResults.Ok(response);
        }
        catch (InvalidDataException ex)
        {
            return TypedResults.BadRequest(ex.Message);
        }
    }

    private static async Task<Ok<PendingExperienceImportCountResponse>>GetPendingExperienceImportCountAsync(
        IPendingExperienceImportService pendingExperienceImportService,
        CancellationToken cancellationToken)
    {
        var count = await pendingExperienceImportService.GetCountAsync(cancellationToken);

        return TypedResults.Ok(new PendingExperienceImportCountResponse(count));
    }

    private static async Task<Results<Ok<PendingExperienceImportDto>, NotFound>>GetNextPendingExperienceImportAsync(
        IPendingExperienceImportService service,
        CancellationToken cancellationToken)
    {
        var pending = await service.GetNextAsync(cancellationToken);

        if (pending is null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(pending.ToDto());
    }

    private static async Task<Results<NoContent, BadRequest>>ResolvePendingExperienceImportAsync(
        Guid id,
        ResolvePendingExperienceImportRequest request,
        IPendingExperienceImportService service,
        CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<PendingExperienceImportResolution>(
                request.Resolution,
                ignoreCase: true,
                out var resolution))
        {
            return TypedResults.BadRequest();
        }

        await service.ResolveAsync(new ExperienceId(id), resolution, cancellationToken);

        return TypedResults.NoContent();
    }
}