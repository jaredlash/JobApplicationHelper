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
        var response = await client.PostAsJsonAsync("/api/job-requirements/extract", request, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<ExtractJobRequirementsResponse>(TestContext.Current.CancellationToken);

        Assert.NotNull(result);

        await using var scope = fixture.Factory.Services.CreateAsyncScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<JobApplicationHelperDbContext>();

        var backgroundJob = await dbContext.BackgroundJobs.SingleAsync(x => x.Id == result.BackgroundJobId, TestContext.Current.CancellationToken);

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
        var response = await client.PostAsJsonAsync("/api/job-requirements/extract", request, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<ExtractJobRequirementsResponse>(TestContext.Current.CancellationToken);

        Assert.NotNull(result);

        await using var scope = fixture.Factory.Services.CreateAsyncScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<JobApplicationHelperDbContext>();

        var backgroundJob = await dbContext.BackgroundJobs.SingleAsync(x => x.Id == result.BackgroundJobId, TestContext.Current.CancellationToken);

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
        var response = await client.PostAsJsonAsync("/api/job-requirements/extract", request, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Get_existing_extracted_job_requirements_returns_requirements()
    {
        // Arrange
        var jobApplicationId = new JobApplicationId(Guid.NewGuid());

        await fixture.AddJobApplicationAsync(CreateJobApplication(jobApplicationId));

        var extractedRequirements =
            ExtractedJobRequirements.Create(
                jobApplicationId,
                new JobRequirements
                {
                    Requirements =
                    [
                        new JobRequirement
                    {
                        Requirement = "5+ years of C# experience",
                        Category = RequirementCategory.TechnicalSkill,
                        Priority = RequirementPriority.Required
                    },
                    new JobRequirement
                    {
                        Requirement = "Experience with PostgreSQL",
                        Category = RequirementCategory.TechnicalSkill,
                        Priority = RequirementPriority.Preferred
                    }
                    ]
                });

        await fixture.AddExtractedJobRequirementsAsync(extractedRequirements);

        // Act
        var response = await client.GetAsync($"/api/job-requirements/{jobApplicationId.Value}", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<GetExtractedJobRequirementsResponse>(TestContext.Current.CancellationToken);

        Assert.NotNull(result);

        Assert.Equal(jobApplicationId.Value, result.JobApplicationId);

        Assert.Equal(2, result.Requirements.Count);

        Assert.Equal("5+ years of C# experience", result.Requirements[0].Requirement);

        Assert.Equal("TechnicalSkill",
            result.Requirements[0].Category);

        Assert.Equal("Required", result.Requirements[0].Priority);
    }

    [Fact]
    public async Task Get_nonexistent_extracted_job_requirements_returns_not_found()
    {
        // Arrange
        var jobApplicationId = Guid.NewGuid();

        // Act
        var response = await client.GetAsync($"/api/job-requirements/{jobApplicationId}", TestContext.Current.CancellationToken);

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
}