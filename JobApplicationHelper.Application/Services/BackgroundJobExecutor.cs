using JobApplicationHelper.Application.Repositories;
using JobApplicationHelper.Domain.Models;
using System.Text.Json;

namespace JobApplicationHelper.Application.Services;

public sealed class BackgroundJobExecutor(
    IJobApplicationRepository jobApplicationRepository,
    IExtractedJobRequirementsRepository extractedJobRequirementsRepository,
    IJobRequirementService jobRequirementService,
    ICoverLetterDraftRepository coverLetterDraftRepository,
    ICoverLetterService coverLetterService)
    : IBackgroundJobExecutor
{
    public async Task ExecuteAsync(
        BackgroundJob job,
        CancellationToken cancellationToken = default)
    {
        switch (job.Type)
        {
            case BackgroundJobType.ExtractJobRequirements:
                await ExecuteExtractJobRequirementsAsync(
                    job,
                    cancellationToken);
                break;

            case BackgroundJobType.GenerateCoverLetter:
                await ExecuteGenerateCoverLetterAsync(
                    job,
                    cancellationToken);
                break;

            default:
                // This should never happen because the BackgroundJobService should only enqueue supported job types.
                throw new InvalidOperationException($"Unsupported background job type '{job.Type}'.");
        }
    }

    private async Task ExecuteExtractJobRequirementsAsync(
        BackgroundJob job,
        CancellationToken cancellationToken)
    {
        var jobApplication = await jobApplicationRepository.GetAsync(job.JobApplicationId, cancellationToken)
            ?? throw new InvalidOperationException($"Job application '{job.JobApplicationId.Value}' was not found.");

        var requirements = await jobRequirementService.ExtractRequirementsAsync(jobApplication.JobPosting, cancellationToken);

        var extractedJobRequirements = ExtractedJobRequirements.Create(job.JobApplicationId, requirements);

        await extractedJobRequirementsRepository.AddOrReplaceAsync(extractedJobRequirements, cancellationToken);
    }

    private async Task ExecuteGenerateCoverLetterAsync(
        BackgroundJob job,
        CancellationToken cancellationToken)
    {
        if (job.Payload is null)
        {
            throw new InvalidOperationException($"Background job '{job.Id.Value}' does not contain a payload.");
        }

        GenerateCoverLetterJobPayload? payload = null;
        try {
            payload = JsonSerializer.Deserialize<GenerateCoverLetterJobPayload>(job.Payload, BackgroundJobPayloadJson.Options);
        }
        catch (JsonException)
        {
            throw new InvalidOperationException($"Background job '{job.Id.Value}' contains an invalid payload.");
        }

        if (payload is null) throw new InvalidOperationException($"Background job '{job.Id.Value}' contains an invalid payload.");

        var draft = await coverLetterService.GenerateCoverLetterAsync(payload.DraftParameters, cancellationToken);

        var coverLetterDraft = CoverLetterDraft.Create(job.JobApplicationId, draft);

        await coverLetterDraftRepository.AddOrReplaceAsync(coverLetterDraft, cancellationToken);
    }
}