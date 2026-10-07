using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using JobApplicationHelper.ApiIntegrationTests.Infrastructure;
using JobApplicationHelper.Contracts.Experiences;
using JobApplicationHelper.Domain.Models;
using JobApplicationHelper.Infrastructure.Persistence;
using JobApplicationHelper.Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace JobApplicationHelper.ApiIntegrationTests.Endpoints;

public sealed class ExperienceImportEndpointTests
    : IClassFixture<ApiIntegrationTestFixture>
{
    private readonly ApiIntegrationTestFixture fixture;
    private readonly HttpClient client;

    public ExperienceImportEndpointTests(
        ApiIntegrationTestFixture fixture)
    {
        this.fixture = fixture;
        client = fixture.CreateClient();
    }

    [Fact]
    public async Task Import_new_experience_returns_added_count()
    {
        // Arrange
        var id = Guid.NewGuid();

        var yaml = CreateYaml(
            $"""
              - id: "{id}"
                title: "Software Engineer"
                type: professional
                organization: "Test Company"
                dateRange:
                  start:
                    year: 2021
                    month: 4
                    day: 12
                  end:
                    year: 2024
                    month: 8
                    day: 23
                summary: "Developed software for a test organization."
                skills:
                  - C#
                  - ASP.NET Core
                evidence:
                  - "Designed and implemented backend services."
                contexts:
                  - Backend Development
                links:
                  - "https://example.com/project"
                notes: "Test experience."
            """);

        // Act
        var response = await PostImportAsync(
            yaml,
            TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result =
            await response.Content.ReadFromJsonAsync<ImportExperiencesResponse>(
                TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Equal(1, result.AddedCount);
        Assert.Equal(0, result.SkippedCount);
        Assert.Equal(0, result.PendingCount);

        var persisted = await GetExperienceAsync(
            new ExperienceId(id));

        Assert.NotNull(persisted);
        Assert.Equal("Software Engineer", persisted.Title);
    }

    [Fact]
    public async Task Import_identical_existing_experience_returns_skipped_count()
    {
        // Arrange
        var experience = CreateExperience();

        await fixture.AddExperienceAsync(experience);

        var yaml = CreateYaml(
            CreateExperienceYaml(experience));

        // Act
        var response = await PostImportAsync(
            yaml,
            TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result =
            await response.Content.ReadFromJsonAsync<ImportExperiencesResponse>(
                TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Equal(0, result.AddedCount);
        Assert.Equal(1, result.SkippedCount);
        Assert.Equal(0, result.PendingCount);

        var pending = await GetPendingImportAsync(experience.Id);

        Assert.Null(pending);
    }

    [Fact]
    public async Task Import_conflicting_existing_experience_returns_pending_count()
    {
        // Arrange
        var experience = CreateExperience();

        await fixture.AddExperienceAsync(experience);

        var yaml = CreateYaml(
            CreateExperienceYaml(
                experience,
                title: "Updated Experience"));

        // Act
        var response = await PostImportAsync(yaml, TestContext.Current.CancellationToken);
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result =
            await response.Content.ReadFromJsonAsync<ImportExperiencesResponse>(
                TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Equal(0, result.AddedCount);
        Assert.Equal(0, result.SkippedCount);
        Assert.Equal(1, result.PendingCount);

        var persisted = await GetExperienceAsync(experience.Id);

        Assert.NotNull(persisted);
        Assert.Equal("Senior Software Engineer", persisted.Title);

        var pending = await GetPendingImportAsync(experience.Id);

        Assert.NotNull(pending);
        Assert.Equal("Updated Experience", pending.Title);
    }

    [Fact]
    public async Task Import_multiple_experiences_returns_correct_counts()
    {
        var firstId = Guid.NewGuid().ToString();
        var secondId = Guid.NewGuid().ToString();

        var yaml = CreateYaml(
            $"""
            - id: {firstId}
              title: First Experience
              type: professional
              organization: First Organization
              summary: First experience summary
              skills:
                - C#
              evidence:
                - First evidence
              contexts:
                - Software Development
              links: []
              notes: First notes
            - id: {secondId}
              title: Second Experience
              type: professional
              organization: Second Organization
              summary: Second experience summary
              skills:
                - SQL
              evidence:
                - Second evidence
              contexts:
                - Database Development
              links: []
              notes: Second notes
            """);

        var response = await PostImportAsync(
            yaml,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<ImportExperiencesResponse>(
            TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Equal(2, result.AddedCount);
        Assert.Equal(0, result.SkippedCount);
        Assert.Equal(0, result.PendingCount);
    }

    [Fact]
    public async Task Import_new_experience_with_legacy_id_persists_legacy_id()
    {
        // Arrange
        const string legacyId = "legacy-experience-1";

        var yaml = CreateYaml(
            CreateExperienceYaml(
                legacyId,
                "Imported Experience"));

        // Act
        var response = await PostImportAsync(
            yaml,
            TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result =
            await response.Content.ReadFromJsonAsync<ImportExperiencesResponse>(
                TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Equal(1, result.AddedCount);
        Assert.Equal(0, result.SkippedCount);
        Assert.Equal(0, result.PendingCount);

        var experience = await GetExperienceByLegacyIdAsync(legacyId);

        Assert.NotNull(experience);
        Assert.NotEqual(Guid.Empty, experience.Id.Value);
        Assert.Equal("Imported Experience", experience.Title);
    }

    [Fact]
    public async Task Import_existing_experience_by_legacy_id_returns_skipped_count()
    {
        // Arrange
        const string legacyId = "legacy-experience-1";

        var experience = CreateExperience();

        await fixture.AddExperienceAsync(
            experience,
            legacyId);

        var yaml = CreateYaml(
            CreateExperienceYaml(
                legacyId,
                experience.Title));

        // Act
        var response = await PostImportAsync(
            yaml,
            TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result =
            await response.Content.ReadFromJsonAsync<ImportExperiencesResponse>(
                TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Equal(0, result.AddedCount);
        Assert.Equal(1, result.SkippedCount);
        Assert.Equal(0, result.PendingCount);

        var persisted = await GetExperienceByLegacyIdAsync(legacyId);

        Assert.NotNull(persisted);
        Assert.Equal(experience.Id, persisted.Id);
    }

    [Fact]
    public async Task Import_empty_file_returns_bad_request()
    {
        // Arrange
        var yaml = string.Empty;

        // Act
        var response = await PostImportAsync(
            yaml,
            TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Import_invalid_yaml_returns_bad_request()
    {
        // Arrange
        var yaml =
            """
            version: 1
            experiences:
              - id: "not valid yaml
                title: "Broken
            """;

        // Act
        var response = await PostImportAsync(
            yaml,
            TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private async Task<HttpResponseMessage> PostImportAsync(
        string yaml,
        CancellationToken cancellationToken)
    {
        using var content = new MultipartFormDataContent();

        using var fileContent = new ByteArrayContent(
            System.Text.Encoding.UTF8.GetBytes(yaml));

        fileContent.Headers.ContentType =
            new MediaTypeHeaderValue("application/x-yaml");

        content.Add(
            fileContent,
            "file",
            "experience-bank.yaml");

        return await client.PostAsync(
            "/api/experiences/import",
            content,
            cancellationToken);
    }

    private async Task<Experience?> GetExperienceAsync(
        ExperienceId id)
    {
        await using var scope =
            fixture.Factory.Services.CreateAsyncScope();

        var dbContext =
            scope.ServiceProvider
                .GetRequiredService<JobApplicationHelperDbContext>();

        var repository = new ExperienceRepository(dbContext);

        return await repository.GetAsync(
            id,
            TestContext.Current.CancellationToken);
    }

    private async Task<Experience?> GetExperienceByLegacyIdAsync(
        string legacyId)
    {
        await using var scope =
            fixture.Factory.Services.CreateAsyncScope();

        var dbContext =
            scope.ServiceProvider
                .GetRequiredService<JobApplicationHelperDbContext>();

        var repository = new ExperienceRepository(dbContext);

        return await repository.GetByLegacyIdAsync(
            legacyId,
            TestContext.Current.CancellationToken);
    }

    private async Task<Experience?> GetPendingImportAsync(
        ExperienceId id)
    {
        await using var scope =
            fixture.Factory.Services.CreateAsyncScope();

        var dbContext =
            scope.ServiceProvider
                .GetRequiredService<JobApplicationHelperDbContext>();

        var repository =
            new PendingExperienceImportRepository(dbContext);

        return await repository.GetAsync(
            id,
            TestContext.Current.CancellationToken);
    }

    private static string CreateYaml(params string[] experiences)
    {
        var result =
            $"""
            version: 1
            experiences:
            {string.Join(
                '\n',
                experiences.Select(x => IndentYaml(x, 2)))}
            """;
        return result;
    }

    private static string IndentYaml(
        string yaml,
        int spaces)
    {
        var indentation = new string(' ', spaces);

        var tempSplit = yaml.Split('\n');
        var result = string.Join(
            Environment.NewLine,
            yaml.Split('\n')
                .Select(line => indentation + line));
        return result;
    }

    private static string CreateExperienceYaml(
        Experience experience,
        string? title = null)
    {
        var result =  CreateExperienceYaml(
            experience.Id.Value.ToString(),
            title ?? experience.Title);

        return result;
    }

    private static string CreateExperienceYaml(string id, string title)
    {
        return
            $"""
               - id: "{id}"
                 title: "{title}"
                 type: professional
                 organization: "Test Company"
                 dateRange:
                   start:
                     year: 2021
                     month: 4
                     day: 12
                   end:
                     year: 2024
                     month: 8
                     day: 23
                 summary: "Developed software for a test organization."
                 skills:
                   - C#
                   - ASP.NET Core
                   - PostgreSQL
                 evidence:
                   - "Designed and implemented backend services."
                   - "Built integration tests against PostgreSQL."
                 contexts:
                   - Backend Development
                   - Software Engineering
                 links:
                   - "https://example.com/project"
                   - "https://github.com/example/project"
                 notes: "Test experience."
             """;
    }

    private static Experience CreateExperience()
    {
        return CreateExperience(
            Guid.NewGuid(),
            "Senior Software Engineer");
    }

    private static Experience CreateExperience(
        Guid id,
        string title)
    {
        return new Experience
        {
            Id = new ExperienceId(id),
            Title = title,
            Type = ExperienceType.Professional,
            Organization = "Test Company",
            DateRange = new DateRange
            {
                Start = new PartialDate
                {
                    Year = 2021,
                    Month = 4,
                    Day = 12
                },
                End = new PartialDate
                {
                    Year = 2024,
                    Month = 8,
                    Day = 23
                }
            },
            Summary = "Developed software for a test organization.",
            Skills =
            [
                "C#",
                "ASP.NET Core",
                "PostgreSQL"
            ],
            Evidence =
            [
                "Designed and implemented backend services.",
                "Built integration tests against PostgreSQL."
            ],
            Contexts =
            [
                "Backend Development",
                "Software Engineering"
            ],
            Links =
            [
                "https://example.com/project",
                "https://github.com/example/project"
            ],
            Notes = "Test experience."
        };
    }
}