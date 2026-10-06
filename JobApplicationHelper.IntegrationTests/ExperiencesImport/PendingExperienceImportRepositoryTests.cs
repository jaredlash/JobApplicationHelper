using JobApplicationHelper.Domain.Models;
using JobApplicationHelper.Infrastructure.Persistence;
using JobApplicationHelper.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace JobApplicationHelper.IntegrationTests.ExperiencesImport;

public sealed class PendingExperienceImportRepositoryTests :
    IClassFixture<PostgreSqlFixture>
{
    private readonly PostgreSqlFixture _fixture;

    public PendingExperienceImportRepositoryTests(
        PostgreSqlFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetCountAsync_WhenEmpty_ReturnsZero()
    {
        await using var dbContext = CreateDbContext();

        var repository = new PendingExperienceImportRepository(dbContext);

        var count = await repository.GetCountAsync(TestContext.Current.CancellationToken);

        Assert.Equal(0, count);
    }

    [Fact]
    public async Task GetCountAsync_WhenPendingImportsExist_ReturnsCount()
    {
        await using var dbContext = CreateDbContext();

        var experience1 = CreateExperience();
        var experience2 = CreateExperience();

        await AddExperienceAsync(dbContext, experience1);
        await AddExperienceAsync(dbContext, experience2);

        var repository = new PendingExperienceImportRepository(dbContext);

        await repository.AddOrReplaceAsync(experience1, TestContext.Current.CancellationToken);
        await repository.AddOrReplaceAsync(experience2, TestContext.Current.CancellationToken);

        var count = await repository.GetCountAsync(TestContext.Current.CancellationToken);

        Assert.Equal(2, count);
    }

    [Fact]
    public async Task GetNextPendingImportAsync_WhenEmpty_ReturnsNull()
    {
        await using var dbContext = CreateDbContext();

        var repository = new PendingExperienceImportRepository(dbContext);

        var result = await repository.GetNextPendingImportAsync(TestContext.Current.CancellationToken);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetNextPendingImportAsync_ReturnsLowestExperienceId()
    {
        await using var dbContext = CreateDbContext();

        var experience1 = CreateExperience(
            new ExperienceId(
                Guid.Parse("00000000-0000-0000-0000-000000000002")));

        var experience2 = CreateExperience(
            new ExperienceId(
                Guid.Parse("00000000-0000-0000-0000-000000000001")));

        await AddExperienceAsync(dbContext, experience1);
        await AddExperienceAsync(dbContext, experience2);

        var repository = new PendingExperienceImportRepository(dbContext);

        await repository.AddOrReplaceAsync(experience1, TestContext.Current.CancellationToken);
        await repository.AddOrReplaceAsync(experience2, TestContext.Current.CancellationToken);

        var result = await repository.GetNextPendingImportAsync(TestContext.Current.CancellationToken);

        Assert.Equal(experience2.Id, result);
    }

    [Fact]
    public async Task GetAsync_WhenMissing_ReturnsNull()
    {
        await using var dbContext = CreateDbContext();

        var repository = new PendingExperienceImportRepository(dbContext);

        var result = await repository.GetAsync(new ExperienceId(Guid.NewGuid()), TestContext.Current.CancellationToken);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetAsync_WhenPendingImportExists_ReturnsDeserializedExperience()
    {
        await using var dbContext = CreateDbContext();

        var experience = CreateCompleteExperience();

        await AddExperienceAsync(dbContext, experience);

        var repository = new PendingExperienceImportRepository(dbContext);

        await repository.AddOrReplaceAsync(experience, TestContext.Current.CancellationToken);

        var result = await repository.GetAsync(experience.Id, TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        AssertExperiencesEqual(experience, result);
    }

    [Fact]
    public async Task AddOrReplaceAsync_AddsPendingImport()
    {
        await using var dbContext = CreateDbContext();

        var experience = CreateExperience();

        await AddExperienceAsync(dbContext, experience);

        var repository = new PendingExperienceImportRepository(dbContext);

        await repository.AddOrReplaceAsync(experience, TestContext.Current.CancellationToken);

        var entity = await dbContext.PendingExperienceImports
            .AsNoTracking()
            .SingleAsync(e => e.ExperienceId == experience.Id.Value, TestContext.Current.CancellationToken);

        Assert.Equal(experience.Id.Value, entity.ExperienceId);
        Assert.False(string.IsNullOrWhiteSpace(entity.SerializedExperience));
        Assert.Equal(1, await repository.GetCountAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task AddOrReplaceAsync_WithSameId_ReplacesExistingPendingImport()
    {
        await using var dbContext = CreateDbContext();

        var experience = CreateExperience();

        await AddExperienceAsync(dbContext, experience);

        var repository = new PendingExperienceImportRepository(dbContext);

        await repository.AddOrReplaceAsync(experience, TestContext.Current.CancellationToken);

        var updatedExperience = CreateExperience(
            experience.Id,
            title: "Updated title");

        await repository.AddOrReplaceAsync(updatedExperience, TestContext.Current.CancellationToken);

        var result = await repository.GetAsync(experience.Id, TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Equal("Updated title", result.Title);
        Assert.Equal(1, await repository.GetCountAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task DeleteAsync_WhenPendingImportExists_RemovesIt()
    {
        await using var dbContext = CreateDbContext();

        var experience = CreateExperience();

        await AddExperienceAsync(dbContext, experience);

        var repository = new PendingExperienceImportRepository(dbContext);

        await repository.AddOrReplaceAsync(experience, TestContext.Current.CancellationToken);

        Assert.Equal(1, await repository.GetCountAsync(TestContext.Current.CancellationToken));

        await repository.DeleteAsync(experience.Id, TestContext.Current.CancellationToken);

        Assert.Null(await repository.GetAsync(experience.Id, TestContext.Current.CancellationToken));
        Assert.Equal(0, await repository.GetCountAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task DeleteAsync_WhenMissing_IsNoOp()
    {
        await using var dbContext = CreateDbContext();

        var repository = new PendingExperienceImportRepository(dbContext);

        var id = new ExperienceId(Guid.NewGuid());

        await repository.DeleteAsync(id, TestContext.Current.CancellationToken);

        Assert.Equal(0, await repository.GetCountAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task DeletingExperience_CascadesToPendingImport()
    {
        await using var dbContext = CreateDbContext();

        var experience = CreateExperience();

        await AddExperienceAsync(dbContext, experience);

        var repository = new PendingExperienceImportRepository(dbContext);

        await repository.AddOrReplaceAsync(experience, TestContext.Current.CancellationToken);

        Assert.Equal(1, await repository.GetCountAsync(TestContext.Current.CancellationToken));

        var experienceRepository = new ExperienceRepository(dbContext);

        await experienceRepository.DeleteAsync(experience.Id, TestContext.Current.CancellationToken);

        Assert.Equal(0, await repository.GetCountAsync(TestContext.Current.CancellationToken));
        Assert.Null(await repository.GetAsync(experience.Id, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task AddOrReplaceAsync_RoundTripsCompleteExperience()
    {
        await using var dbContext = CreateDbContext();

        var experience = CreateCompleteExperience();

        await AddExperienceAsync(dbContext, experience);

        var repository = new PendingExperienceImportRepository(dbContext);

        await repository.AddOrReplaceAsync(experience, TestContext.Current.CancellationToken);

        var result = await repository.GetAsync(experience.Id, TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        AssertExperiencesEqual(experience, result);
    }

    private JobApplicationHelperDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<JobApplicationHelperDbContext>()
            .UseNpgsql(_fixture.ConnectionString)
            .Options;

        return new JobApplicationHelperDbContext(options);
    }

    private static async Task AddExperienceAsync(
        JobApplicationHelperDbContext dbContext,
        Experience experience)
    {
        var repository = new ExperienceRepository(dbContext);

        await repository.AddOrReplaceAsync(experience);
    }

    private static Experience CreateExperience(
        ExperienceId? id = null,
        string title = "Test experience")
    {
        return new Experience
        {
            Id = id ?? new ExperienceId(Guid.NewGuid()),
            Title = title,
            Type = ExperienceType.Professional,
            Organization = "Test Organization",
            Summary = "Test summary"
        };
    }

    private static Experience CreateCompleteExperience()
    {
        return new Experience
        {
            Id = new ExperienceId(Guid.NewGuid()),
            Title = "Complete experience",
            Type = ExperienceType.PersonalProject,
            Organization = "Test Organization",
            DateRange = new DateRange
            {
                Start = new PartialDate
                {
                    Year = 2024,
                    Month = 3,
                    Day = 15
                },
                End = new PartialDate
                {
                    Year = 2025,
                    Month = 8,
                    Day = 20
                }
            },
            Summary = "A complete test experience.",
            Skills =
            [
                "C#",
                "PostgreSQL",
                "WPF"
            ],
            Evidence =
            [
                "Implemented the feature.",
                "Added integration tests."
            ],
            Contexts =
            [
                "Software Development",
                "Database"
            ],
            Links =
            [
                "https://example.com/project",
                "https://github.com/example/project"
            ],
            Notes = "Additional notes."
        };
    }

    private static void AssertExperiencesEqual(
        Experience expected,
        Experience actual)
    {
        Assert.Equal(expected.Id, actual.Id);
        Assert.Equal(expected.Title, actual.Title);
        Assert.Equal(expected.Type, actual.Type);
        Assert.Equal(expected.Organization, actual.Organization);
        Assert.Equal(expected.Summary, actual.Summary);
        Assert.Equal(expected.Notes, actual.Notes);

        Assert.Equal(
            expected.DateRange?.Start?.Year,
            actual.DateRange?.Start?.Year);
        Assert.Equal(
            expected.DateRange?.Start?.Month,
            actual.DateRange?.Start?.Month);
        Assert.Equal(
            expected.DateRange?.Start?.Day,
            actual.DateRange?.Start?.Day);

        Assert.Equal(
            expected.DateRange?.End?.Year,
            actual.DateRange?.End?.Year);
        Assert.Equal(
            expected.DateRange?.End?.Month,
            actual.DateRange?.End?.Month);
        Assert.Equal(
            expected.DateRange?.End?.Day,
            actual.DateRange?.End?.Day);

        Assert.Equal(expected.Skills, actual.Skills);
        Assert.Equal(expected.Evidence, actual.Evidence);
        Assert.Equal(expected.Contexts, actual.Contexts);
        Assert.Equal(expected.Links, actual.Links);
    }
}