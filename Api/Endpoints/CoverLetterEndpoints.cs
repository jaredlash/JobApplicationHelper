using JobApplicationHelper.ApiMappings.ToDomain;
using JobApplicationHelper.Application.Repositories;
using JobApplicationHelper.Application.Services;
using JobApplicationHelper.Contracts.CoverLetters;
using JobApplicationHelper.Contracts.JobRequirements;
using JobApplicationHelper.Domain.Models;

namespace JobApplicationHelper.Api.Endpoints;

public static class CoverLetterEndpoints
{
    public static IEndpointRouteBuilder MapCoverLetterEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/cover-letters");

        group.MapPost("", GenerateCoverLetterAsync);
        group.MapGet("/{jobApplicationId:guid}", GetCoverLetterAsync);

        return endpoints;
    }


    //app.MapPost(
    //"/api/cover-letters/verify",
    //async(
    //    VerifyCoverLetterRequest request,
    //    ICoverLetterService coverLetterService,
    //    CancellationToken cancellationToken) =>
    //{
    //    var verificationResult = await coverLetterService.VerifyDraftAsync(
    //        request.DraftParameters.ToDomain(),
    //        request.Draft,
    //        cancellationToken);

    //    return Results.Ok(verificationResult.ToDto());
    //});

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

        var payloadJson = System.Text.Json.JsonSerializer.Serialize(payload, BackgroundJobPayloadJson.Options);

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
}