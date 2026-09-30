using System.Net;
using System.Net.Http.Json;
using JobApplicationHelper.ApiIntegrationTests.Infrastructure;
using JobApplicationHelper.Contracts.BackgroundJobs;
using JobApplicationHelper.Domain.Models;
using JobApplicationHelper.Infrastructure.Persistence;
using JobApplicationHelper.Infrastructure.Persistence.Mapping;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace JobApplicationHelper.ApiIntegrationTests.Endpoints;

public sealed class BackgroundJobEndpointTests
    : IClassFixture<ApiIntegrationTestFixture>
{
    private readonly ApiIntegrationTestFixture fixture;
    private readonly HttpClient client;

    public BackgroundJobEndpointTests(ApiIntegrationTestFixture fixture)
    {
        this.fixture = fixture;
        client = fixture.CreateClient();
    }

    [Fact]
    public async Task Get_existing_background_job_returns_job()
    {
        // Arrange
        var jobApplicationId = new JobApplicationId(Guid.NewGuid());

        await fixture.AddJobApplicationAsync(CreateJobApplication(jobApplicationId));

        var backgroundJob = BackgroundJob.Create(BackgroundJobType.ExtractJobRequirements, BackgroundJobPriority.High, jobApplicationId);

        await using (var scope = fixture.Factory.Services.CreateAsyncScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<JobApplicationHelperDbContext>();

            await dbContext.BackgroundJobs.AddAsync(backgroundJob.ToEntity());

            await dbContext.SaveChangesAsync();
        }

        // Act
        var response = await client.GetAsync($"/api/background-jobs/{backgroundJob.Id.Value}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<GetBackgroundJobResponse>();

        Assert.NotNull(result);
        Assert.Equal(backgroundJob.Id.Value, result.Id);
        Assert.Equal(BackgroundJobType.ExtractJobRequirements.ToString(), result.Type);
        Assert.Equal(BackgroundJobPriority.High.ToString(), result.Priority);
        Assert.Equal(BackgroundJobStatus.Pending.ToString(), result.Status);
        AssertEqualToPostgresPrecision(backgroundJob.CreatedAt, result.CreatedAt);
        Assert.Null(result.StartedAt);
        Assert.Null(result.CompletedAt);
        Assert.Null(result.Error);
    }

    [Fact]
    public async Task Get_nonexistent_background_job_returns_not_found()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var response = await client.GetAsync($"/api/background-jobs/{id}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private static JobApplication CreateJobApplication(JobApplicationId id)
    {
        return new JobApplication
        {
            Id = id,
            CountryCode = "US",
            IncludeCoverLetter = true,
            CompanyName = "Test Company",
            PositionTitle = "Test Position",
            URL = "https://example.com/job",
            City = "Chicago",
            JobPosting = "Test job posting.",
            CreatedAt = DateTime.UtcNow,
            ApplicationFolder = "test-folder"
        };
    }

    private static void AssertEqualToPostgresPrecision(DateTime expected, DateTime actual)
    {
        Assert.Equal(
            expected.Ticks / 10,
            actual.Ticks / 10);
    }
}