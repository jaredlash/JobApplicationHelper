using JobApplicationHelper.Domain.Models;
using JobApplicationHelper.Infrastructure.Persistence;
using JobApplicationHelper.Infrastructure.Persistence.Repositories;
using JobApplicationHelper.Infrastructure.Persistence.Mapping;
using Microsoft.EntityFrameworkCore;

namespace JobApplicationHelper.IntegrationTests.CoverLetters;

public sealed class CoverLetterDraftRepositoryTests
    : IClassFixture<PostgreSqlFixture>
{
    private readonly PostgreSqlFixture database;

    public CoverLetterDraftRepositoryTests(PostgreSqlFixture database)
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
    public async Task Adds_and_retrieves_cover_letter_drafts()
    {
        await using var dbContext = CreateDbContext();

        var jobApplication = await CreateJobApplicationAsync(dbContext);

        var jobApplicationId = jobApplication.Id;

        var draft = CoverLetterDraft.Create(jobApplicationId, "This is a test cover letter draft.");

        var repository = new CoverLetterDraftRepository(dbContext);

        await repository.AddOrReplaceAsync(draft, TestContext.Current.CancellationToken);

        var retrievedDraft = await repository.GetAsync(jobApplicationId, TestContext.Current.CancellationToken);

        Assert.NotNull(retrievedDraft);

        Assert.Equal(draft.JobApplicationId, retrievedDraft.JobApplicationId);

        AssertEqualToPostgresPrecision(draft.CreatedAt, retrievedDraft.CreatedAt);

        Assert.Equal(draft.Draft, retrievedDraft.Draft);
    }

    [Fact]
    public async Task Returns_null_when_cover_letter_does_not_exist()
    {
        await using var dbContext = CreateDbContext();

        var repository = new CoverLetterDraftRepository(dbContext);

        var draft = await repository.GetAsync(new JobApplicationId(Guid.NewGuid()), TestContext.Current.CancellationToken);

        Assert.Null(draft);
    }

    [Fact]
    public async Task Replaces_existing_cover_letter_drafts()
    {
        var jobApplicationId = new JobApplicationId(Guid.NewGuid());

        await using (var dbContext = CreateDbContext())
        {
            var jobApplication = await CreateJobApplicationAsync(dbContext, jobApplicationId);
        }


        var firstDraft = CoverLetterDraft.Create(jobApplicationId, "This is the first cover letter draft.");

        await using (var dbContext = CreateDbContext())
        {
            var repository = new CoverLetterDraftRepository(dbContext);

            await repository.AddOrReplaceAsync(firstDraft, TestContext.Current.CancellationToken);
        }

        // Ensure the second draft has a different CreatedAt.
        await Task.Delay(10, TestContext.Current.CancellationToken);

        var secondDraft = CoverLetterDraft.Create(jobApplicationId, "This is the second cover letter draft.");

        await using (var dbContext = CreateDbContext())
        {
            var repository = new CoverLetterDraftRepository(dbContext);

            await repository.AddOrReplaceAsync(secondDraft, TestContext.Current.CancellationToken);
        }

        await using var verificationContext = CreateDbContext();

        var verificationRepository = new CoverLetterDraftRepository(verificationContext);

        var retrieved = await verificationRepository.GetAsync(jobApplicationId, TestContext.Current.CancellationToken);

        Assert.NotNull(retrieved);

        var rowCount = await verificationContext.CoverLetterDrafts
            .CountAsync(x => x.JobApplicationId == jobApplicationId.Value, TestContext.Current.CancellationToken);

        Assert.Equal(1, rowCount);

        Assert.Equal(jobApplicationId, retrieved.JobApplicationId);

        Assert.Equal("This is the second cover letter draft.", retrieved.Draft);

        AssertEqualToPostgresPrecision(secondDraft.CreatedAt, retrieved.CreatedAt);

        AssertNotEqualToPostgresPrecision(firstDraft.CreatedAt, retrieved.CreatedAt);
    }

    [Fact]
    public async Task Deletes_cover_letter_drafts()
    {
        var jobApplicationId = new JobApplicationId(Guid.NewGuid());

        await using (var dbContext = CreateDbContext())
        {
            var jobApplication = await CreateJobApplicationAsync(dbContext, jobApplicationId);
        }


        var draft = CoverLetterDraft.Create(jobApplicationId, "This is a test cover letter draft.");

        await using (var dbContext = CreateDbContext())
        {
            var repository = new CoverLetterDraftRepository(dbContext);

            await repository.AddOrReplaceAsync(draft, TestContext.Current.CancellationToken);
        }

        await using (var dbContext = CreateDbContext())
        {
            var repository = new CoverLetterDraftRepository(dbContext);

            await repository.DeleteAsync(jobApplicationId, TestContext.Current.CancellationToken);
        }

        await using var verificationContext = CreateDbContext();

        var verificationRepository = new CoverLetterDraftRepository(verificationContext);

        var retrieved = await verificationRepository.GetAsync(jobApplicationId, TestContext.Current.CancellationToken);

        Assert.Null(retrieved);
    }

    private static void AssertEqualToPostgresPrecision(DateTime expected, DateTime actual) => Assert.Equal(expected.Ticks / 10, actual.Ticks / 10);
    private static void AssertNotEqualToPostgresPrecision(DateTime expected, DateTime actual) => Assert.NotEqual(expected.Ticks / 10, actual.Ticks / 10);

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