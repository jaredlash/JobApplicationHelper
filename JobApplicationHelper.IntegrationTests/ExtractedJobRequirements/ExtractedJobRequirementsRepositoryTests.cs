using JobApplicationHelper.Domain.Models;
using JobApplicationHelper.Infrastructure.Persistence;
using JobApplicationHelper.Infrastructure.Persistence.Repositories;
using JobApplicationHelper.Infrastructure.Persistence.Mapping;
using Microsoft.EntityFrameworkCore;

namespace JobApplicationHelper.IntegrationTests.ExtractedJobRequirements;

public sealed class ExtractedJobRequirementsRepositoryTests
    : IClassFixture<PostgreSqlFixture>
{
    private readonly PostgreSqlFixture database;

    public ExtractedJobRequirementsRepositoryTests(PostgreSqlFixture database)
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
    public async Task Adds_and_retrieves_extracted_job_requirements()
    {
        await using var dbContext = CreateDbContext();

        var jobApplication = await CreateJobApplicationAsync(dbContext);

        var jobApplicationId = jobApplication.Id;

        var requirements = new JobRequirements
        {
            Requirements =
            [
                new JobRequirement
                {
                    Requirement = "C# experience",
                    Category = RequirementCategory.TechnicalSkill,
                    Priority = RequirementPriority.Required
                },
                new JobRequirement
                {
                    Requirement = "Experience working with PostgreSQL",
                    Category = RequirementCategory.TechnicalSkill,
                    Priority = RequirementPriority.Preferred
                }
            ]
        };

        var extractedRequirements = Domain.Models.ExtractedJobRequirements.Create(jobApplicationId, requirements);

        var repository = new ExtractedJobRequirementsRepository(dbContext);

        await repository.AddOrReplaceAsync(extractedRequirements, TestContext.Current.CancellationToken);

        var retrievedRequirements = await repository.GetAsync(jobApplicationId, TestContext.Current.CancellationToken);

        Assert.NotNull(retrievedRequirements);

        Assert.Equal(extractedRequirements.JobApplicationId, retrievedRequirements.JobApplicationId);

        AssertEqualToPostgresPrecision(extractedRequirements.CreatedAt, retrievedRequirements.CreatedAt);

        Assert.Equal(extractedRequirements.Requirements.Requirements.Count, retrievedRequirements.Requirements.Requirements.Count);

        Assert.Equal("C# experience", retrievedRequirements.Requirements.Requirements[0].Requirement);

        Assert.Equal(RequirementCategory.TechnicalSkill, retrievedRequirements.Requirements.Requirements[0].Category);

        Assert.Equal(RequirementPriority.Required, retrievedRequirements.Requirements.Requirements[0].Priority);

        Assert.Equal("Experience working with PostgreSQL", retrievedRequirements.Requirements.Requirements[1].Requirement);

        Assert.Equal(RequirementPriority.Preferred, retrievedRequirements.Requirements.Requirements[1].Priority);
    }

    [Fact]
    public async Task Returns_null_when_extracted_job_requirements_do_not_exist()
    {
        await using var dbContext = CreateDbContext();

        var repository = new ExtractedJobRequirementsRepository(dbContext);

        var requirements = await repository.GetAsync(new JobApplicationId(Guid.NewGuid()), TestContext.Current.CancellationToken);

        Assert.Null(requirements);
    }

    [Fact]
    public async Task Replaces_existing_extracted_job_requirements()
    {
        var jobApplicationId = new JobApplicationId(Guid.NewGuid());

        await using (var dbContext = CreateDbContext())
        {
            var jobApplication = await CreateJobApplicationAsync(dbContext, jobApplicationId);
        }


        var firstRequirements = new JobRequirements
        {
            Requirements =
            [
                new JobRequirement
                {
                    Requirement = "C# experience",
                    Category = RequirementCategory.TechnicalSkill,
                    Priority = RequirementPriority.Required
                }
            ]
        };

        var firstExtraction = Domain.Models.ExtractedJobRequirements.Create(jobApplicationId, firstRequirements);

        await using (var dbContext = CreateDbContext())
        {
            var repository = new ExtractedJobRequirementsRepository(dbContext);

            await repository.AddOrReplaceAsync(firstExtraction, TestContext.Current.CancellationToken);
        }

        // Ensure the second extraction has a different CreatedAt.
        await Task.Delay(10, TestContext.Current.CancellationToken);

        var secondRequirements = new JobRequirements
        {
            Requirements =
            [
                new JobRequirement
            {
                Requirement = "PostgreSQL experience",
                Category = RequirementCategory.TechnicalSkill,
                Priority = RequirementPriority.Preferred
            }
            ]
        };

        var secondExtraction = Domain.Models.ExtractedJobRequirements.Create(jobApplicationId, secondRequirements);

        await using (var dbContext = CreateDbContext())
        {
            var repository = new ExtractedJobRequirementsRepository(dbContext);

            await repository.AddOrReplaceAsync(secondExtraction, TestContext.Current.CancellationToken);
        }

        await using var verificationContext = CreateDbContext();

        var verificationRepository =  new ExtractedJobRequirementsRepository(verificationContext);

        var retrieved = await verificationRepository.GetAsync(jobApplicationId, TestContext.Current.CancellationToken);

        Assert.NotNull(retrieved);

        var rowCount = await verificationContext.ExtractedJobRequirements
            .CountAsync(x => x.JobApplicationId == jobApplicationId.Value, TestContext.Current.CancellationToken);

        Assert.Equal(1, rowCount);

        Assert.Equal(jobApplicationId, retrieved.JobApplicationId);

        Assert.Equal("PostgreSQL experience", retrieved.Requirements.Requirements[0].Requirement);

        Assert.Equal(RequirementPriority.Preferred, retrieved.Requirements.Requirements[0].Priority);

        AssertEqualToPostgresPrecision(secondExtraction.CreatedAt, retrieved.CreatedAt);

        AssertNotEqualToPostgresPrecision(firstExtraction.CreatedAt, retrieved.CreatedAt);
    }

    [Fact]
    public async Task Deletes_extracted_job_requirements()
    {
        var jobApplicationId = new JobApplicationId(Guid.NewGuid());

        await using (var dbContext = CreateDbContext())
        {
            var jobApplication = await CreateJobApplicationAsync(dbContext, jobApplicationId);
        }

        var extraction = Domain.Models.ExtractedJobRequirements.Create(
            jobApplicationId,
            new JobRequirements
            {
                Requirements =
                [
                    new JobRequirement
                {
                    Requirement = "C# experience",
                    Category = RequirementCategory.TechnicalSkill,
                    Priority = RequirementPriority.Required
                }
                ]
            });

        await using (var dbContext = CreateDbContext())
        {
            var repository = new ExtractedJobRequirementsRepository(dbContext);

            await repository.AddOrReplaceAsync(extraction, TestContext.Current.CancellationToken);
        }

        await using (var dbContext = CreateDbContext())
        {
            var repository = new ExtractedJobRequirementsRepository(dbContext);

            await repository.DeleteAsync(jobApplicationId, TestContext.Current.CancellationToken);
        }

        await using var verificationContext = CreateDbContext();

        var verificationRepository = new ExtractedJobRequirementsRepository(verificationContext);

        var retrieved = await verificationRepository.GetAsync(jobApplicationId, TestContext.Current.CancellationToken);

        Assert.Null(retrieved);
    }

    private static void AssertEqualToPostgresPrecision(DateTime expected, DateTime actual) => Assert.Equal(expected.Ticks / 10, actual.Ticks / 10);
    private static void AssertNotEqualToPostgresPrecision(DateTime expected, DateTime actual) => Assert.NotEqual(expected.Ticks / 10, actual.Ticks / 10);

    private async Task<JobApplication> CreateJobApplicationAsync(JobApplicationHelperDbContext dbContext,
        JobApplicationId? id = null)
    {
        var jobApplication = new JobApplication
        {
            Id = id ?? new JobApplicationId(Guid.NewGuid()),
            CountryCode = "US",
            IncludeCoverLetter = true,
            CompanyName = "Test Company",
            PositionTitle = "Software Engineer",
            URL = "https://example.com/job",
            City = "Chicago",
            JobPosting = "Test job posting.",
            CreatedAt = DateTime.UtcNow,
            ApplicationFolder = "test-folder"
        };

        dbContext.JobApplications.Add(jobApplication.ToEntity());
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        return jobApplication;
    }
}