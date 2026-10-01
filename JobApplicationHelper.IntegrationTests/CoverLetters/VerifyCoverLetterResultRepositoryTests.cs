using JobApplicationHelper.Domain.Models;
using JobApplicationHelper.Infrastructure.Persistence;
using JobApplicationHelper.Infrastructure.Persistence.Repositories;
using JobApplicationHelper.Infrastructure.Persistence.Mapping;
using Microsoft.EntityFrameworkCore;

namespace JobApplicationHelper.IntegrationTests.CoverLetters;

public sealed class VerifyCoverLetterResultRepositoryTests : IClassFixture<PostgreSqlFixture>
{
    private readonly PostgreSqlFixture database;

    public VerifyCoverLetterResultRepositoryTests(PostgreSqlFixture database)
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
    public async Task Adds_and_retrieves_valid_cover_letter_verification()
    {
        await using var dbContext = CreateDbContext();

        var jobApplication = await CreateJobApplicationAsync(dbContext);

        var jobApplicationId = jobApplication.Id;

        var verificationResult = CreateValidVerificationResult();

        var verifyCoverLetterResult = VerifyCoverLetterResult.Create(jobApplicationId, verificationResult);

        var repository = new VerifyCoverLetterResultRepository(dbContext);

        await repository.AddOrReplaceAsync(verifyCoverLetterResult);

        var retrievedResult = await repository.GetAsync(jobApplicationId);

        Assert.NotNull(retrievedResult);

        Assert.Equal(verifyCoverLetterResult.JobApplicationId, retrievedResult.JobApplicationId);

        AssertEqualToPostgresPrecision(verifyCoverLetterResult.CreatedAt, retrievedResult.CreatedAt);

        Assert.True(retrievedResult.VerificationResult.IsValid);
        Assert.Equal(verifyCoverLetterResult.VerificationResult.IsValid, retrievedResult.VerificationResult.IsValid);
    }

    [Fact]
    public async Task Adds_and_retrieves_cover_letter_verification_with_errors()
    {
        await using var dbContext = CreateDbContext();

        var jobApplication = await CreateJobApplicationAsync(dbContext);

        var jobApplicationId = jobApplication.Id;

        var verificationResult = CreateVerificationResultWithErrors();

        var verifyCoverLetterResult = VerifyCoverLetterResult.Create(jobApplicationId, verificationResult);

        var repository = new VerifyCoverLetterResultRepository(dbContext);

        await repository.AddOrReplaceAsync(verifyCoverLetterResult);

        var retrievedResult = await repository.GetAsync(jobApplicationId);

        Assert.NotNull(retrievedResult);

        Assert.Equal(verifyCoverLetterResult.JobApplicationId, retrievedResult.JobApplicationId);

        AssertEqualToPostgresPrecision(verifyCoverLetterResult.CreatedAt, retrievedResult.CreatedAt);

        Assert.False(retrievedResult.VerificationResult.IsValid);
        Assert.Equal(verifyCoverLetterResult.VerificationResult.IsValid, retrievedResult.VerificationResult.IsValid);
        Assert.Single(retrievedResult.VerificationResult.Errors);
        Assert.Equal("Error 1", retrievedResult.VerificationResult.Errors.First());
        Assert.Single(retrievedResult.VerificationResult.UnsupportedClaims);
        Assert.Equal("Claim 1", retrievedResult.VerificationResult.UnsupportedClaims.First());
        Assert.Single(retrievedResult.VerificationResult.StyleViolations);
        Assert.Equal("Violation 1", retrievedResult.VerificationResult.StyleViolations.First());
        Assert.Single(retrievedResult.VerificationResult.RequiredCorrections);
        Assert.Equal("Correction 1", retrievedResult.VerificationResult.RequiredCorrections.First());
    }

    [Fact]
    public async Task Returns_null_when_verification_result_does_not_exist()
    {
        await using var dbContext = CreateDbContext();

        var repository = new VerifyCoverLetterResultRepository(dbContext);

        var draft = await repository.GetAsync(new JobApplicationId(Guid.NewGuid()));

        Assert.Null(draft);
    }

    [Fact]
    public async Task Replaces_existing_verification_results()
    {
        var jobApplicationId = new JobApplicationId(Guid.NewGuid());

        await using (var dbContext = CreateDbContext())
        {
            var jobApplication = await CreateJobApplicationAsync(dbContext, jobApplicationId);
        }


        var firstVerificationResult = VerifyCoverLetterResult.Create(jobApplicationId, CreateVerificationResultWithErrors());

        await using (var dbContext = CreateDbContext())
        {
            var repository = new VerifyCoverLetterResultRepository(dbContext);

            await repository.AddOrReplaceAsync(firstVerificationResult);
        }

        // Ensure the second verification result has a different CreatedAt.
        await Task.Delay(10);

        var secondVerificationResult = VerifyCoverLetterResult.Create(jobApplicationId, CreateValidVerificationResult());

        await using (var dbContext = CreateDbContext())
        {
            var repository = new VerifyCoverLetterResultRepository(dbContext);

            await repository.AddOrReplaceAsync(secondVerificationResult);
        }

        await using var verificationContext = CreateDbContext();

        var verificationRepository = new VerifyCoverLetterResultRepository(verificationContext);

        var retrieved = await verificationRepository.GetAsync(jobApplicationId);

        Assert.NotNull(retrieved);

        var rowCount = await verificationContext.VerifyCoverLetterResults
            .CountAsync(x => x.JobApplicationId == jobApplicationId.Value);

        Assert.Equal(1, rowCount);

        Assert.Equal(jobApplicationId, retrieved.JobApplicationId);

        // Sufficient to ensure the verification result has changed, since the first one had errors and the second one is valid.
        Assert.NotEqual(firstVerificationResult.VerificationResult.IsValid, retrieved.VerificationResult.IsValid);

        AssertEqualToPostgresPrecision(secondVerificationResult.CreatedAt, retrieved.CreatedAt);

        AssertNotEqualToPostgresPrecision(firstVerificationResult.CreatedAt, retrieved.CreatedAt);
    }

    [Fact]
    public async Task Deletes_verification_results()
    {
        var jobApplicationId = new JobApplicationId(Guid.NewGuid());

        await using (var dbContext = CreateDbContext())
        {
            var jobApplication = await CreateJobApplicationAsync(dbContext, jobApplicationId);
        }

        var verificationResult = CreateVerificationResultWithErrors();

        var verifyCoverLetterResult = VerifyCoverLetterResult.Create(jobApplicationId, verificationResult);

        await using (var dbContext = CreateDbContext())
        {
            var repository = new VerifyCoverLetterResultRepository(dbContext);

            await repository.AddOrReplaceAsync(verifyCoverLetterResult);
        }

        await using (var dbContext = CreateDbContext())
        {
            var repository = new VerifyCoverLetterResultRepository(dbContext);

            await repository.DeleteAsync(jobApplicationId);
        }

        await using var verificationContext = CreateDbContext();

        var verificationRepository = new VerifyCoverLetterResultRepository(verificationContext);

        var retrieved = await verificationRepository.GetAsync(jobApplicationId);

        Assert.Null(retrieved);
    }

    private static void AssertEqualToPostgresPrecision(DateTime expected, DateTime actual) => Assert.Equal(expected.Ticks / 10, actual.Ticks / 10);

    private static void AssertNotEqualToPostgresPrecision(DateTime expected, DateTime actual) => Assert.NotEqual(expected.Ticks / 10, actual.Ticks / 10);

    private VerificationResult CreateValidVerificationResult()
    {
        return new VerificationResult();
    }

    private VerificationResult CreateVerificationResultWithErrors()
    {
        var result = new VerificationResult();

        result.Errors.Add("Error 1");
        result.UnsupportedClaims.Add("Claim 1");
        result.StyleViolations.Add("Violation 1");
        result.RequiredCorrections.Add("Correction 1");

        return result;
    }

    private async Task<JobApplication> CreateJobApplicationAsync(JobApplicationHelperDbContext dbContext, JobApplicationId? id = null)
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
        await dbContext.SaveChangesAsync();

        return jobApplication;
    }
}