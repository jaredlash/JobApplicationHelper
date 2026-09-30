using System.Net;
using System.Net.Http.Json;
using JobApplicationHelper.ApiIntegrationTests.Infrastructure;
using JobApplicationHelper.Contracts.JobRequirements;
using JobApplicationHelper.Domain.Models;
using JobApplicationHelper.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace JobApplicationHelper.ApiIntegrationTests.Endpoints;

public sealed class JobRequirementsEndpointTests
    : IClassFixture<ApiIntegrationTestFixture>
{
    private readonly ApiIntegrationTestFixture fixture;
    private readonly HttpClient client;

    public JobRequirementsEndpointTests(ApiIntegrationTestFixture fixture)
    {
        this.fixture = fixture;
        client = fixture.CreateClient();
    }

    [Fact]
    public async Task Extract_creates_high_priority_background_job()
    {
        // Arrange
        var jobApplicationId = new JobApplicationId(Guid.NewGuid());

        await fixture.AddJobApplicationAsync(CreateJobApplication(jobApplicationId));

        var request = new ExtractJobRequirementsRequest(jobApplicationId.Value, "High");

        // Act
        var response = await client.PostAsJsonAsync("/api/job-requirements/extract", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<ExtractJobRequirementsResponse>();

        Assert.NotNull(result);

        await using var scope = fixture.Factory.Services.CreateAsyncScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<JobApplicationHelperDbContext>();

        var backgroundJob = await dbContext.BackgroundJobs.SingleAsync(x => x.Id == result.BackgroundJobId);

        Assert.Equal(BackgroundJobType.ExtractJobRequirements, backgroundJob.Type);

        Assert.Equal(BackgroundJobPriority.High, backgroundJob.Priority);

        Assert.Equal(BackgroundJobStatus.Pending, backgroundJob.Status);
    }

    [Fact]
    public async Task Extract_creates_normal_priority_background_job()
    {
        // Arrange
        var jobApplicationId = new JobApplicationId(Guid.NewGuid());

        await fixture.AddJobApplicationAsync(CreateJobApplication(jobApplicationId));

        var request = new ExtractJobRequirementsRequest(jobApplicationId.Value, "Normal");

        // Act
        var response = await client.PostAsJsonAsync("/api/job-requirements/extract", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<ExtractJobRequirementsResponse>();

        Assert.NotNull(result);

        await using var scope = fixture.Factory.Services.CreateAsyncScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<JobApplicationHelperDbContext>();

        var backgroundJob = await dbContext.BackgroundJobs.SingleAsync(x => x.Id == result.BackgroundJobId);

        Assert.Equal(BackgroundJobType.ExtractJobRequirements, backgroundJob.Type);

        Assert.Equal(BackgroundJobPriority.Normal, backgroundJob.Priority);

        Assert.Equal(BackgroundJobStatus.Pending, backgroundJob.Status);
    }

    [Fact]
    public async Task Extract_with_invalid_priority_returns_bad_request()
    {
        // Arrange
        var request = new ExtractJobRequirementsRequest(Guid.NewGuid(), "Urgent");

        // Act
        var response = await client.PostAsJsonAsync("/api/job-requirements/extract", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
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
}