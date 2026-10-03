using JobApplicationHelper.Application.Services;
using JobApplicationHelper.Domain.Models;
using JobApplicationHelper.Infrastructure.Persistence;
using JobApplicationHelper.Infrastructure.Persistence.Mapping;
using JobApplicationHelper.Infrastructure.Persistence.Repositories;
using JobApplicationHelper.IntegrationTests.Fakes;
using Microsoft.EntityFrameworkCore;

namespace JobApplicationHelper.IntegrationTests.BackgroundJobs;

public sealed class BackgroundJobExecutionTests
    : IClassFixture<PostgreSqlFixture>
{
    private readonly PostgreSqlFixture database;

    public BackgroundJobExecutionTests(PostgreSqlFixture database)
    {
        this.database = database;
    }

    private JobApplicationHelperDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<JobApplicationHelperDbContext>()
            .UseNpgsql(database.ConnectionString)
            .Options;

        return new JobApplicationHelperDbContext(options);
    }

    [Fact]
    public async Task Executes_extract_job_requirements()
    {
        var jobApplicationId = new JobApplicationId(Guid.NewGuid());

        var expectedRequirements = new JobRequirements
        {
            Requirements =
            [
                new JobRequirement
            {
                Requirement = "C#",
                Category = RequirementCategory.TechnicalSkill,
                Priority = RequirementPriority.Required
            },
            new JobRequirement
            {
                Requirement = "ASP.NET Core",
                Category = RequirementCategory.TechnicalSkill,
                Priority = RequirementPriority.Preferred
            }
            ]
        };

        await using (var dbContext = CreateDbContext())
        {
            await CreateJobApplicationAsync(
                dbContext,
                jobApplicationId);
        }

        var fakeJobRequirementService = new FakeJobRequirementService(expectedRequirements);
        var fakeCoverLetterService = new FakeCoverLetterService();

        var queue = new FakeBackgroundJobQueue();

        BackgroundJobId jobId;

        await using (var dbContext = CreateDbContext())
        {
            var backgroundJobRepository = new BackgroundJobRepository(dbContext);
            var jobApplicationRepository = new JobApplicationRepository(dbContext);
            var extractedJobRequirementsRepository = new ExtractedJobRequirementsRepository(dbContext);
            var coverLetterDraftRepository = new CoverLetterDraftRepository(dbContext);
            var verifyCoverLetterResultRepository = new VerifyCoverLetterResultRepository(dbContext);

            var notifier = new FakeBackgroundJobNotifier();

            var executor = new BackgroundJobExecutor(
                jobApplicationRepository,
                extractedJobRequirementsRepository,
                fakeJobRequirementService,
                coverLetterDraftRepository,
                verifyCoverLetterResultRepository,
                fakeCoverLetterService);

            var backgroundJobService = new BackgroundJobService(
                backgroundJobRepository,
                queue,
                executor,
                notifier);

            jobId = await backgroundJobService.CreateAsync(
                BackgroundJobType.ExtractJobRequirements,
                BackgroundJobPriority.Normal,
                jobApplicationId);

            await backgroundJobService.ExecuteAsync(jobId);
        }

        await using (var dbContext = CreateDbContext())
        {
            var backgroundJobRepository =
                new BackgroundJobRepository(dbContext);

            var extractedJobRequirementsRepository =
                new ExtractedJobRequirementsRepository(dbContext);

            var job = await backgroundJobRepository.GetAsync(jobId);

            var extractedRequirements =
                await extractedJobRequirementsRepository.GetAsync(
                    jobApplicationId);

            Assert.NotNull(job);
            Assert.Equal(
                BackgroundJobStatus.Completed,
                job.Status);

            Assert.NotNull(extractedRequirements);

            Assert.Equal(
                jobApplicationId,
                extractedRequirements.JobApplicationId);

            Assert.Equal(
                expectedRequirements.Requirements.Count,
                extractedRequirements.Requirements.Requirements.Count);

            Assert.Equal(
                expectedRequirements.Requirements[0].Requirement,
                extractedRequirements.Requirements.Requirements[0].Requirement);

            Assert.Equal(
                expectedRequirements.Requirements[1].Requirement,
                extractedRequirements.Requirements.Requirements[1].Requirement);
        }
    }

    private static async Task<JobApplication> CreateJobApplicationAsync(
        JobApplicationHelperDbContext dbContext,
        JobApplicationId id)
    {
        var jobApplication = new JobApplication
        {
            Id = id,
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

        dbContext.JobApplications.Add(jobApplication.ToEntity());
        await dbContext.SaveChangesAsync();

        return jobApplication;
    }
}