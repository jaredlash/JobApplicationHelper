using JobApplicationHelper.ApiMappings.ToDomain;
using JobApplicationHelper.ApiMappings.ToDto;
using JobApplicationHelper.Application.Repositories;
using JobApplicationHelper.Application.Services;
using JobApplicationHelper.Contracts.CoverLetters;
using JobApplicationHelper.Domain.Models;
using System.Text.Json;

namespace JobApplicationHelper.Api.Endpoints;

public static class CoverLetterEndpoints
{
    public static IEndpointRouteBuilder MapCoverLetterEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/cover-letters");

        group.MapPost("", GenerateCoverLetterAsync);
        group.MapGet("/{jobApplicationId:guid}", GetCoverLetterAsync);

        group.MapPost("/verify", VerifyCoverLetterAsync);
        group.MapGet("/verify/{jobApplicationId:guid}", GetVerifyCoverLetterResultAsync);

        return endpoints;
    }

    private static async Task<IResult> GenerateCoverLetterAsync(
        GenerateCoverLetterRequest request,
        IBackgroundJobService backgroundJobService,
        CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<BackgroundJobPriority>(request.Priority, ignoreCase: true, out var priority))
        {
            return Results.BadRequest($"Invalid background job priority '{request.Priority}'.");
        }

        var payload = new GenerateCoverLetterJobPayload(request.DraftParameters.ToDomain());

        var payloadJson = JsonSerializer.Serialize(payload, BackgroundJobPayloadJson.Options);

        var backgroundJobId = await backgroundJobService.CreateAsync(
            BackgroundJobType.GenerateCoverLetter,
            priority,
            new JobApplicationId(request.JobApplicationId),
            payloadJson,
            cancellationToken);

        return Results.Ok(new GenerateCoverLetterResponse(backgroundJobId.Value));
    }

    private static async Task<IResult> GetCoverLetterAsync(
        Guid jobApplicationId,
        ICoverLetterDraftRepository repository,
        CancellationToken cancellationToken)
    {
        var coverLetterDraft = await repository.GetAsync(new JobApplicationId(jobApplicationId), cancellationToken);

        if (coverLetterDraft is null)
        {
            return Results.NotFound();
        }

        var response = new GetCoverLetterDraftResponse(coverLetterDraft.Draft);

        return Results.Ok(response);
    }

    private static async Task<IResult> VerifyCoverLetterAsync(
        VerifyCoverLetterRequest request,
        IBackgroundJobService backgroundJobService,
        CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<BackgroundJobPriority>(request.Priority, ignoreCase: true, out var priority))
        {
            return Results.BadRequest($"Invalid background job priority '{request.Priority}'.");
        }

        var payload = new VerifyCoverLetterJobPayload(request.DraftParameters.ToDomain(), request.Draft);

        var payloadJson = JsonSerializer.Serialize(payload, BackgroundJobPayloadJson.Options);

        var backgroundJobId = await backgroundJobService.CreateAsync(
            BackgroundJobType.VerifyCoverLetter,
            priority,
            new JobApplicationId(request.JobApplicationId),
            payloadJson,
            cancellationToken);

        return Results.Ok(new VerifyCoverLetterResponse(backgroundJobId.Value));
    }

    private static async Task<IResult> GetVerifyCoverLetterResultAsync(
        Guid jobApplicationId,
        IVerifyCoverLetterResultRepository repository,
        CancellationToken cancellationToken)
    {
        var verifyCoverLetterResult = await repository.GetAsync(new JobApplicationId(jobApplicationId), cancellationToken);

        if (verifyCoverLetterResult is null)
        {
            return Results.NotFound();
        }

        var response = verifyCoverLetterResult.VerificationResult.ToDto();

        return Results.Ok(response);
    }
}