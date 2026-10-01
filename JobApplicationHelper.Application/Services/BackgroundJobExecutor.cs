using JobApplicationHelper.Application.Repositories;
using JobApplicationHelper.Domain.Models;

namespace JobApplicationHelper.Application.Services;

public sealed class BackgroundJobExecutor(
    IJobApplicationRepository jobApplicationRepository,
    IExtractedJobRequirementsRepository extractedJobRequirementsRepository,
    IJobRequirementService jobRequirementService)
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
}