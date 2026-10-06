using System.Net;
using System.Net.Http.Json;
using JobApplicationHelper.ApiIntegrationTests.Infrastructure;
using JobApplicationHelper.Contracts.Experiences;
using JobApplicationHelper.Domain.Models;

namespace JobApplicationHelper.ApiIntegrationTests.Endpoints;

public sealed class ExperienceEndpointTests
    : IClassFixture<ApiIntegrationTestFixture>
{
    private readonly ApiIntegrationTestFixture fixture;
    private readonly HttpClient client;

    public ExperienceEndpointTests(ApiIntegrationTestFixture fixture)
    {
        this.fixture = fixture;
        client = fixture.CreateClient();
    }

    [Fact]
    public async Task GetAll_returns_persisted_experiences()
    {
        // Arrange
        var firstExperience = CreateExperience();
        var secondExperience = CreateExperience();

        await fixture.AddExperienceAsync(firstExperience);
        await fixture.AddExperienceAsync(secondExperience);

        // Act
        var response = await client.GetAsync("/api/experiences/", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<List<ExperienceDto>>(TestContext.Current.CancellationToken);

        Assert.NotNull(result);

        var first = result.Where(e => e.Id == firstExperience.Id.Value);
        Assert.Single(first);

        var second = result.Where(e => e.Id == secondExperience.Id.Value);
        Assert.Single(second);

        AssertExperienceDto(first.First(), firstExperience);
        AssertExperienceDto(second.First(), secondExperience);
    }

    [Fact]
    public async Task GetById_existing_experience_returns_experience()
    {
        // Arrange
        var experience = CreateExperience();

        await fixture.AddExperienceAsync(experience);

        // Act
        var response = await client.GetAsync($"/api/experiences/{experience.Id.Value}", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<ExperienceDto>(TestContext.Current.CancellationToken);

        Assert.NotNull(result);

        AssertExperienceDto(result, experience);
    }

    [Fact]
    public async Task GetById_nonexistent_experience_returns_not_found()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var response = await client.GetAsync($"/api/experiences/{id}", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Put_nonexistent_experience_creates_experience()
    {
        // Arrange
        var id = Guid.NewGuid();

        var request = CreateRequest();

        // Act
        var response = await client.PutAsJsonAsync($"/api/experiences/{id}", request, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var getResponse = await client.GetAsync($"/api/experiences/{id}", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var result = await getResponse.Content.ReadFromJsonAsync<ExperienceDto>(TestContext.Current.CancellationToken);

        Assert.NotNull(result);

        Assert.Equal(id, result.Id);
        Assert.Equal(request.Title, result.Title);
        Assert.Equal(request.Type, result.Type);
        Assert.Equal(request.Organization, result.Organization);
        AssertDateRangeEqual(request.DateRange, result.DateRange);
        Assert.Equal(request.Summary, result.Summary);
        Assert.Equal(request.Skills.OrderBy(x => x), result.Skills.OrderBy(x => x));
        Assert.Equal(request.Evidence, result.Evidence);
        Assert.Equal(request.Contexts.OrderBy(x => x), result.Contexts.OrderBy(x => x));
        Assert.Equal(request.Links, result.Links);
        Assert.Equal(request.Notes, result.Notes);
    }

    [Fact]
    public async Task Put_existing_experience_replaces_experience()
    {
        // Arrange
        var experience = CreateExperience();

        await fixture.AddExperienceAsync(experience);

        var request = new UpsertExperienceRequest(
            Title: "Updated Experience",
            Type: "PersonalProject",
            Organization: "Updated Organization",
            DateRange: new DateRangeDto(
                new PartialDateDto(2024, 3, 15),
                new PartialDateDto(2025, 9, 30)),
            Summary: "Updated summary.",
            Skills:
            [
                "C#",
                "ASP.NET Core",
                "PostgreSQL"
            ],
            Evidence:
            [
                "Updated evidence one.",
                "Updated evidence two."
            ],
            Contexts:
            [
                "Backend",
                "Web Development"
            ],
            Links:
            [
                "https://example.com/updated"
            ],
            Notes: "Updated notes.");

        // Act
        var response = await client.PutAsJsonAsync($"/api/experiences/{experience.Id.Value}", request, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var getResponse = await client.GetAsync($"/api/experiences/{experience.Id.Value}", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var result = await getResponse.Content.ReadFromJsonAsync<ExperienceDto>(TestContext.Current.CancellationToken);

        Assert.NotNull(result);

        Assert.Equal(experience.Id.Value, result.Id);
        Assert.Equal(request.Title, result.Title);
        Assert.Equal(request.Type, result.Type);
        Assert.Equal(request.Organization, result.Organization);
        AssertDateRangeEqual(request.DateRange, result.DateRange);
        Assert.Equal(request.Summary, result.Summary);
        Assert.Equal(request.Skills.OrderBy(x => x), result.Skills.OrderBy(x => x));
        Assert.Equal(request.Evidence, result.Evidence);
        Assert.Equal(request.Contexts.OrderBy(x => x), result.Contexts.OrderBy(x => x));
        Assert.Equal(request.Links, result.Links);
        Assert.Equal(request.Notes, result.Notes);
    }

    [Fact]
    public async Task Delete_existing_experience_returns_no_content()
    {
        // Arrange
        var experience = CreateExperience();

        await fixture.AddExperienceAsync(experience);

        // Act
        var response = await client.DeleteAsync($"/api/experiences/{experience.Id.Value}", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var getResponse = await client.GetAsync($"/api/experiences/{experience.Id.Value}", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    private static Experience CreateExperience()
    {
        return new Experience
        {
            Id = new ExperienceId(Guid.NewGuid()),
            Title = "Senior Software Engineer",
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

    private static UpsertExperienceRequest CreateRequest()
    {
        return new UpsertExperienceRequest(
            Title: "Software Engineer",
            Type: "Professional",
            Organization: "Test Organization",
            DateRange: new DateRangeDto(
                new PartialDateDto(2020, 6, 1),
                new PartialDateDto(2023, 12, 15)),
            Summary: "Built software for a test organization.",
            Skills:
            [
                "C#",
                "PostgreSQL"
            ],
            Evidence:
            [
                "Implemented backend services.",
                "Created automated tests."
            ],
            Contexts:
            [
                "Backend Development",
                "Testing"
            ],
            Links:
            [
                "https://example.com/experience"
            ],
            Notes: "Test request.");
    }

    private static void AssertExperienceDto(
        ExperienceDto actual,
        Experience expected)
    {
        Assert.Equal(expected.Id.Value, actual.Id);
        Assert.Equal(expected.Title, actual.Title);
        Assert.Equal(expected.Type.ToString(), actual.Type);
        Assert.Equal(expected.Organization, actual.Organization);

        AssertDateRangeEqual(
            expected.DateRange,
            actual.DateRange);

        Assert.Equal(expected.Summary, actual.Summary);

        Assert.Equal(
            expected.Skills.OrderBy(x => x),
            actual.Skills.OrderBy(x => x));

        Assert.Equal(expected.Evidence, actual.Evidence);

        Assert.Equal(
            expected.Contexts.OrderBy(x => x),
            actual.Contexts.OrderBy(x => x));

        Assert.Equal(expected.Links, actual.Links);
        Assert.Equal(expected.Notes, actual.Notes);
    }

    private static void AssertDateRangeEqual(
        DateRangeDto? expected,
        DateRangeDto? actual)
    {
        if (expected is null)
        {
            Assert.Null(actual);
            return;
        }

        Assert.NotNull(actual);

        AssertPartialDateEqual(expected.Start, actual.Start);
        AssertPartialDateEqual(expected.End, actual.End);
    }

    private static void AssertDateRangeEqual(
        DateRange? expected,
        DateRangeDto? actual)
    {
        if (expected is null)
        {
            Assert.Null(actual);
            return;
        }

        Assert.NotNull(actual);

        AssertPartialDateEqual(expected.Start, actual.Start);
        AssertPartialDateEqual(expected.End, actual.End);
    }

    private static void AssertPartialDateEqual(
        PartialDateDto? expected,
        PartialDateDto? actual)
    {
        if (expected is null)
        {
            Assert.Null(actual);
            return;
        }

        Assert.NotNull(actual);

        Assert.Equal(expected.Year, actual.Year);
        Assert.Equal(expected.Month, actual.Month);
        Assert.Equal(expected.Day, actual.Day);
    }

    private static void AssertPartialDateEqual(
        PartialDate? expected,
        PartialDateDto? actual)
    {
        if (expected is null)
        {
            Assert.Null(actual);
            return;
        }

        Assert.NotNull(actual);

        Assert.Equal(expected.Year, actual.Year);
        Assert.Equal(expected.Month, actual.Month);
        Assert.Equal(expected.Day, actual.Day);
    }
}