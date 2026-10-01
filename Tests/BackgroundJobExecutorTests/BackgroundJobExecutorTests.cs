using JobApplicationHelper.Application.Repositories;
using JobApplicationHelper.Application.Services;
using JobApplicationHelper.Domain.Models;
using System.Text.Json;

namespace JobApplicationHelper.Tests.BackgroundJobExecutorTests;

public sealed class BackgroundJobExecutorTests
{
    [Fact]
    public async Task Executes_extract_job_requirements()
    {
        var jobApplicationId = new JobApplicationId(Guid.NewGuid());

        var jobApplication = new JobApplication
        {
            Id = jobApplicationId,
            CountryCode = "US",
            IncludeCoverLetter = true,
            CompanyName = "Test Company",
            PositionTitle = "Software Engineer",
            URL = "https://example.com/job",
            City = "Chicago",
            JobPosting = "We are looking for a software engineer.",
            CreatedAt = DateTime.UtcNow,
            ApplicationFolder = "test-folder"
        };

        var expectedRequirements = new JobRequirements
        {
            Requirements =
            [
                new JobRequirement
                {
                    Requirement = "C#",
                    Category = RequirementCategory.TechnicalSkill,
                    Priority = RequirementPriority.Required
                }
            ]
        };

        var jobApplicationRepository = new FakeJobApplicationRepository(jobApplication);
        var jobRequirementService = new FakeJobRequirementService(expectedRequirements);
        var extractedJobRequirementsRepository = new FakeExtractedJobRequirementsRepository();
        var coverLetterDraftRepository = new FakeCoverLetterDraftRepository();
        var verifyCoverLetterResultRepository = new FakeVerifyCoverLetterResultRepository();
        var coverLetterService = new FakeCoverLetterService();

        var executor = new BackgroundJobExecutor(
            jobApplicationRepository,
            extractedJobRequirementsRepository,
            jobRequirementService,
            coverLetterDraftRepository,
            verifyCoverLetterResultRepository,
            coverLetterService);

        var job = BackgroundJob.Create(BackgroundJobType.ExtractJobRequirements, BackgroundJobPriority.Normal, jobApplicationId);

        await executor.ExecuteAsync(job, TestContext.Current.CancellationToken);

        Assert.Equal(jobApplicationId, jobApplicationRepository.RequestedId);
        Assert.Equal(jobApplication.JobPosting, jobRequirementService.ReceivedJobPosting);

        Assert.NotNull(extractedJobRequirementsRepository.SavedRequirements);

        Assert.Equal(jobApplicationId, extractedJobRequirementsRepository.SavedRequirements.JobApplicationId);

        var saved = extractedJobRequirementsRepository.SavedRequirements;

        Assert.NotNull(saved);
        Assert.Equal(jobApplicationId, saved.JobApplicationId);
        Assert.Single(saved.Requirements.Requirements);

        var requirement = saved.Requirements.Requirements[0];

        Assert.Equal("C#", requirement.Requirement);
        Assert.Equal(RequirementCategory.TechnicalSkill, requirement.Category);
        Assert.Equal(RequirementPriority.Required, requirement.Priority);
    }

    [Fact]
    public async Task Throws_when_job_application_does_not_exist()
    {
        var jobApplicationId = new JobApplicationId(Guid.NewGuid());

        var jobApplicationRepository = new FakeJobApplicationRepository();
        var jobRequirementService = new FakeJobRequirementService();
        var extractedJobRequirementsRepository = new FakeExtractedJobRequirementsRepository();
        var coverLetterDraftRepository = new FakeCoverLetterDraftRepository();
        var verifyCoverLetterResultRepository = new FakeVerifyCoverLetterResultRepository();
        var coverLetterService = new FakeCoverLetterService();

        var executor = new BackgroundJobExecutor(
            jobApplicationRepository,
            extractedJobRequirementsRepository,
            jobRequirementService,
            coverLetterDraftRepository,
            verifyCoverLetterResultRepository,
            coverLetterService);

        var job = BackgroundJob.Create(BackgroundJobType.ExtractJobRequirements, BackgroundJobPriority.Normal, jobApplicationId);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => executor.ExecuteAsync(job, TestContext.Current.CancellationToken));

        Assert.Equal($"Job application '{jobApplicationId.Value}' was not found.", exception.Message);

        Assert.Null(jobRequirementService.ReceivedJobPosting);
        Assert.Null(extractedJobRequirementsRepository.SavedRequirements);
    }

    [Fact]
    public async Task Throws_for_unsupported_background_job_type()
    {
        var jobApplicationRepository = new FakeJobApplicationRepository();
        var jobRequirementService = new FakeJobRequirementService();
        var extractedJobRequirementsRepository = new FakeExtractedJobRequirementsRepository();
        var coverLetterDraftRepository = new FakeCoverLetterDraftRepository();
        var verifyCoverLetterResultRepository = new FakeVerifyCoverLetterResultRepository();
        var coverLetterService = new FakeCoverLetterService();

        var executor = new BackgroundJobExecutor(
            jobApplicationRepository,
            extractedJobRequirementsRepository,
            jobRequirementService,
            coverLetterDraftRepository,
            verifyCoverLetterResultRepository,
            coverLetterService);

        var job = BackgroundJob.Create(BackgroundJobType.Llm, BackgroundJobPriority.Normal, new JobApplicationId(Guid.NewGuid()));

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => executor.ExecuteAsync(job, TestContext.Current.CancellationToken));

        Assert.Equal("Unsupported background job type 'Llm'.", exception.Message);

        Assert.Null(extractedJobRequirementsRepository.SavedRequirements);
    }

    [Fact]
    public async Task Executes_generate_cover_letter()
    {
        var jobApplicationId = new JobApplicationId(Guid.NewGuid());

        var jobApplicationRepository = new FakeJobApplicationRepository();
        var jobRequirementService = new FakeJobRequirementService();
        var extractedJobRequirementsRepository = new FakeExtractedJobRequirementsRepository();
        var coverLetterDraftRepository = new FakeCoverLetterDraftRepository();
        var verifyCoverLetterResultRepository = new FakeVerifyCoverLetterResultRepository();
        var coverLetterService = new FakeCoverLetterService();

        var executor = new BackgroundJobExecutor(
            jobApplicationRepository,
            extractedJobRequirementsRepository,
            jobRequirementService,
            coverLetterDraftRepository,
            verifyCoverLetterResultRepository,
            coverLetterService);

        var testDraftParameters = CreateCoverLetterDraftParameters();
        var generateCoverLetterRequest = new GenerateCoverLetterJobPayload(testDraftParameters);
        var payloadJson = JsonSerializer.Serialize(generateCoverLetterRequest, BackgroundJobPayloadJson.Options);

        var job = BackgroundJob.Create(BackgroundJobType.GenerateCoverLetter, BackgroundJobPriority.Normal, jobApplicationId, payloadJson);

        await executor.ExecuteAsync(job, TestContext.Current.CancellationToken);

        var generatedDraft = await coverLetterDraftRepository.GetAsync(jobApplicationId, TestContext.Current.CancellationToken);


        Assert.NotNull(job.Payload);
        Assert.NotNull(generatedDraft);
        
        var containsTechnicalSkillAsString = job.Payload.Contains("TechnicalSkill", StringComparison.InvariantCultureIgnoreCase);
        // Test that we have serialized the payload enums as strings and not integers
        Assert.True(containsTechnicalSkillAsString);

        Assert.Equal("C#-TechnicalSkill-Required", generatedDraft.Draft);

    }

    [Fact]
    public async Task Throws_when_no_cover_letter_payload()
    {
        var jobApplicationId = new JobApplicationId(Guid.NewGuid());

        var jobApplicationRepository = new FakeJobApplicationRepository();
        var jobRequirementService = new FakeJobRequirementService();
        var extractedJobRequirementsRepository = new FakeExtractedJobRequirementsRepository();
        var coverLetterDraftRepository = new FakeCoverLetterDraftRepository();
        var verifyCoverLetterResultRepository = new FakeVerifyCoverLetterResultRepository();
        var coverLetterService = new FakeCoverLetterService();

        var executor = new BackgroundJobExecutor(
            jobApplicationRepository,
            extractedJobRequirementsRepository,
            jobRequirementService,
            coverLetterDraftRepository,
            verifyCoverLetterResultRepository,
            coverLetterService);

        var job = BackgroundJob.Create(BackgroundJobType.GenerateCoverLetter, BackgroundJobPriority.Normal, jobApplicationId, null);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => executor.ExecuteAsync(job, TestContext.Current.CancellationToken));

        Assert.Equal($"Background job '{job.Id.Value}' does not contain a payload.", exception.Message);
    }

    [Fact]
    public async Task Throws_when_invalid_cover_letter_payload()
    {
        var jobApplicationId = new JobApplicationId(Guid.NewGuid());

        var jobApplicationRepository = new FakeJobApplicationRepository();
        var jobRequirementService = new FakeJobRequirementService();
        var extractedJobRequirementsRepository = new FakeExtractedJobRequirementsRepository();
        var coverLetterDraftRepository = new FakeCoverLetterDraftRepository();
        var verifyCoverLetterResultRepository = new FakeVerifyCoverLetterResultRepository();
        var coverLetterService = new FakeCoverLetterService();

        var executor = new BackgroundJobExecutor(
            jobApplicationRepository,
            extractedJobRequirementsRepository,
            jobRequirementService,
            coverLetterDraftRepository,
            verifyCoverLetterResultRepository,
            coverLetterService);

        var payloadJson = "{ \"Invalid\": \"Payload\" }"; // Invalid payload for GenerateCoverLetterJobPayload

        var job = BackgroundJob.Create(BackgroundJobType.GenerateCoverLetter, BackgroundJobPriority.Normal, jobApplicationId, payloadJson);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => executor.ExecuteAsync(job, TestContext.Current.CancellationToken));

        Assert.Equal($"Background job '{job.Id.Value}' contains an invalid payload.", exception.Message);
    }

    private CoverLetterDraftParameters CreateCoverLetterDraftParameters()
    {
        return new CoverLetterDraftParameters
        {
            CountryCode = "US",
            JobPosting = "Do stuff, get paid",
            CandidateNotes = "Seems legit",
            Tone = "Super Serious",
            Style = "Informal",
            TargetAudience = "Millennials",
            Requirements = new JobRequirements
            {
                Requirements =
                [
                    CreateJobRequirement()
                ]
            },
            DesiredWordCount = 420
        };
    }

    private JobRequirement CreateJobRequirement()
    {
        var jobRequirement = new JobRequirement
        {
            Requirement = "C#",
            Category = RequirementCategory.TechnicalSkill,
            Priority = RequirementPriority.Required
        };
        jobRequirement.Evidence.NoSupportingEvidence = true;

        return jobRequirement;
    }
}