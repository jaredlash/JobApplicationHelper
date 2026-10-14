using JobApplicationHelper.Application.Repositories;
using JobApplicationHelper.Application.Services;
using JobApplicationHelper.Domain.Models;
using JobApplicationHelper.Infrastructure.Persistence;
using JobApplicationHelper.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit.v3;

namespace JobApplicationHelper.IntegrationTests;

public sealed class ExperienceImportServiceTests :
    IClassFixture<PostgreSqlFixture>
{
    private readonly PostgreSqlFixture fixture;

    public ExperienceImportServiceTests(PostgreSqlFixture fixture)
    {
        this.fixture = fixture;
    }

    [Fact]
    public async Task ImportAsync_WithEmptyImport_ReturnsZeroCounts()
    {
        await using var dbContext = CreateDbContext();

        var service = CreateService(dbContext);

        var import = new ExperienceBankImportModel();

        var result = await service.ImportAsync(import, TestContext.Current.CancellationToken);

        Assert.Equal(0, result.AddedCount);
        Assert.Equal(0, result.SkippedCount);
        Assert.Equal(0, result.PendingCount);
    }

    [Fact]
    public async Task ImportAsync_WithNewExperienceUsingGuidId_AddsExperience()
    {
        await using var dbContext = CreateDbContext();

        var service = CreateService(dbContext);

        var id = Guid.NewGuid();

        var import = CreateImport(CreateImportedExperience(id.ToString(), "New Experience"));

        var result = await service.ImportAsync(import, TestContext.Current.CancellationToken);

        Assert.Equal(1, result.AddedCount);
        Assert.Equal(0, result.SkippedCount);
        Assert.Equal(0, result.PendingCount);

        var repository = new ExperienceRepository(dbContext);

        var experience = await repository.GetAsync(new ExperienceId(id), TestContext.Current.CancellationToken);

        Assert.NotNull(experience);
        Assert.Equal("New Experience", experience.Title);
    }

    [Fact]
    public async Task ImportAsync_WithNewExperienceUsingLegacyId_AddsExperienceWithLegacyId()
    {
        await using var dbContext = CreateDbContext();

        var service = CreateService(dbContext);

        const string legacyId = "legacy-experience-1";

        var import = CreateImport(CreateImportedExperience(legacyId, "New Experience"));

        var result = await service.ImportAsync(import, TestContext.Current.CancellationToken);

        Assert.Equal(1, result.AddedCount);
        Assert.Equal(0, result.SkippedCount);
        Assert.Equal(0, result.PendingCount);

        var repository = new ExperienceRepository(dbContext);

        var experience = await repository.GetByLegacyIdAsync(legacyId, TestContext.Current.CancellationToken);

        Assert.NotNull(experience);
        Assert.Equal("New Experience", experience.Title);
        Assert.NotEqual(Guid.Empty, experience.Id.Value);
    }

    [Fact]
    public async Task ImportAsync_WithExistingIdenticalExperience_SkipsExperience()
    {
        await using var dbContext = CreateDbContext();

        var repository = new ExperienceRepository(dbContext);

        var existing = CreateExperience(Guid.NewGuid(), "Existing Experience");

        await repository.AddOrReplaceAsync(existing, cancellationToken: TestContext.Current.CancellationToken);

        var service = CreateService(dbContext);

        var import = CreateImport(CreateImportedExperience(existing.Id.Value.ToString(), existing.Title));

        var result = await service.ImportAsync(import, TestContext.Current.CancellationToken);

        Assert.Equal(0, result.AddedCount);
        Assert.Equal(1, result.SkippedCount);
        Assert.Equal(0, result.PendingCount);

        var pendingRepository = new PendingExperienceImportRepository(dbContext);

        var pending = await pendingRepository.GetAsync(existing.Id, TestContext.Current.CancellationToken);

        Assert.Null(pending);
    }

    [Fact]
    public async Task ImportAsync_WithExistingDifferentExperience_CreatesPendingImport()
    {
        await using var dbContext = CreateDbContext();

        var repository = new ExperienceRepository(dbContext);

        var existing = CreateExperience(Guid.NewGuid(), "Existing Experience");

        await repository.AddOrReplaceAsync(existing, cancellationToken: TestContext.Current.CancellationToken);

        var service = CreateService(dbContext);

        var import = CreateImport(CreateImportedExperience(existing.Id.Value.ToString(), "Updated Experience"));

        var result = await service.ImportAsync(import, TestContext.Current.CancellationToken);

        Assert.Equal(0, result.AddedCount);
        Assert.Equal(0, result.SkippedCount);
        Assert.Equal(1, result.PendingCount);

        var pendingRepository = new PendingExperienceImportRepository(dbContext);

        var pending = await pendingRepository.GetAsync(existing.Id, TestContext.Current.CancellationToken);

        Assert.NotNull(pending);
        Assert.Equal(existing.Id, pending.Id);
        Assert.Equal("Updated Experience", pending.Title);

        var persistedExisting = await repository.GetAsync(existing.Id, TestContext.Current.CancellationToken);

        Assert.NotNull(persistedExisting);
        Assert.Equal("Existing Experience", persistedExisting.Title);
    }

    [Fact]
    public async Task ImportAsync_WithExistingLegacyId_UsesExistingExperience()
    {
        await using var dbContext = CreateDbContext();

        const string legacyId = "legacy-experience-1";

        var repository = new ExperienceRepository(dbContext);

        var existing = CreateExperience(Guid.NewGuid(), "Existing Experience");

        await repository.AddOrReplaceAsync(existing, legacyId, TestContext.Current.CancellationToken);

        var service = CreateService(dbContext);

        var import = CreateImport(CreateImportedExperience(legacyId, existing.Title));

        var result = await service.ImportAsync(import, TestContext.Current.CancellationToken);

        Assert.Equal(0, result.AddedCount);
        Assert.Equal(1, result.SkippedCount);
        Assert.Equal(0, result.PendingCount);

        var persisted = await repository.GetByLegacyIdAsync(legacyId, TestContext.Current.CancellationToken);

        Assert.NotNull(persisted);
        Assert.Equal(existing.Id, persisted.Id);
    }

    [Fact]
    public async Task ImportAsync_WithDifferentExperienceUsingExistingLegacyId_CreatesPendingImport()
    {
        await using var dbContext = CreateDbContext();

        const string legacyId = "legacy-experience-1";

        var repository = new ExperienceRepository(dbContext);

        var existing = CreateExperience(Guid.NewGuid(), "Existing Experience");

        await repository.AddOrReplaceAsync(existing, legacyId, TestContext.Current.CancellationToken);

        var service = CreateService(dbContext);

        var import = CreateImport(CreateImportedExperience(legacyId, "Updated Experience"));

        var result = await service.ImportAsync(import, TestContext.Current.CancellationToken);

        Assert.Equal(0, result.AddedCount);
        Assert.Equal(0, result.SkippedCount);
        Assert.Equal(1, result.PendingCount);

        var pendingRepository = new PendingExperienceImportRepository(dbContext);

        var pending = await pendingRepository.GetAsync(existing.Id, TestContext.Current.CancellationToken);

        Assert.NotNull(pending);
        Assert.Equal(existing.Id, pending.Id);
        Assert.Equal("Updated Experience", pending.Title);

        var persistedExisting = await repository.GetAsync(existing.Id, TestContext.Current.CancellationToken);

        Assert.NotNull(persistedExisting);
        Assert.Equal("Existing Experience", persistedExisting.Title);
    }

    [Fact]
    public async Task ImportAsync_WithEquivalentCollections_DoesNotCreatePendingImport()
    {
        await using var dbContext = CreateDbContext();

        var repository = new ExperienceRepository(dbContext);

        var id = Guid.NewGuid();

        var existing = CreateExperience(id, "Equivalent Experience");

        existing.Skills =
        [
            "C#",
            "PostgreSQL",
            "WPF"
        ];

        existing.Contexts =
        [
            "Desktop",
            "Backend"
        ];

        existing.Evidence =
        [
            "Built the application.",
            "Added integration tests."
        ];

        existing.Links =
        [
            "https://example.com/project",
            "https://example.com/source"
        ];

        await repository.AddOrReplaceAsync(existing, cancellationToken: TestContext.Current.CancellationToken);

        var imported = CreateImportedExperience(id.ToString(), existing.Title);

        // Skills and contexts are unordered.
        imported.Skills =
        [
            "wpf",
            "C#",
            "POSTGRESQL"
        ];

        imported.Contexts =
        [
            "backend",
            "desktop"
        ];

        // Evidence and links are ordered.
        imported.Evidence =
        [
            "Built the application.",
            "Added integration tests."
        ];

        imported.Links =
        [
            "https://example.com/project",
            "https://example.com/source"
        ];

        var service = CreateService(dbContext);

        var result = await service.ImportAsync(CreateImport(imported), TestContext.Current.CancellationToken);

        Assert.Equal(0, result.AddedCount);
        Assert.Equal(1, result.SkippedCount);
        Assert.Equal(0, result.PendingCount);

        var pendingRepository = new PendingExperienceImportRepository(dbContext);

        var pending = await pendingRepository.GetAsync(new ExperienceId(id), TestContext.Current.CancellationToken);

        Assert.Null(pending);
    }

    [Fact]
    public async Task ImportAsync_WithDifferentScalarProperty_CreatesPendingImport()
    {
        await using var dbContext = CreateDbContext();

        var repository = new ExperienceRepository(dbContext);

        var existing = CreateExperience(Guid.NewGuid(), "Experience");

        await repository.AddOrReplaceAsync(existing, cancellationToken: TestContext.Current.CancellationToken);

        var imported = CreateImportedExperience(existing.Id.Value.ToString(), existing.Title);

        imported.Summary = "Different summary.";

        var service = CreateService(dbContext);

        var result = await service.ImportAsync(CreateImport(imported), TestContext.Current.CancellationToken);

        Assert.Equal(0, result.AddedCount);
        Assert.Equal(0, result.SkippedCount);
        Assert.Equal(1, result.PendingCount);
    }

    [Fact]
    public async Task ImportAsync_WithMultipleExperiences_ReturnsCorrectCounts()
    {
        await using var dbContext = CreateDbContext();

        var repository = new ExperienceRepository(dbContext);

        var existingId = Guid.NewGuid();

        var existing = CreateExperience(existingId, "Existing Experience");

        await repository.AddOrReplaceAsync(existing, cancellationToken: TestContext.Current.CancellationToken);

        var newExperienceId = Guid.NewGuid();

        var import = CreateImport(
            CreateImportedExperience(newExperienceId.ToString(), "New Experience"),
            CreateImportedExperience(existingId.ToString(), existing.Title),
            CreateImportedExperience(existingId.ToString(), "Conflicting Experience"));

        var service = CreateService(dbContext);

        var result = await service.ImportAsync(import, TestContext.Current.CancellationToken);

        Assert.Equal(1, result.AddedCount);
        Assert.Equal(1, result.SkippedCount);
        Assert.Equal(1, result.PendingCount);

        var pendingRepository = new PendingExperienceImportRepository(dbContext);

        var pending = await pendingRepository.GetAsync(existing.Id, TestContext.Current.CancellationToken);

        Assert.NotNull(pending);
        Assert.Equal("Conflicting Experience", pending.Title);

        var added = await repository.GetAsync(new ExperienceId(newExperienceId), TestContext.Current.CancellationToken);

        Assert.NotNull(added);
        Assert.Equal("New Experience", added.Title);
    }

    [Fact]
    public async Task ImportAsync_WithRepeatedConflictingImport_ReplacesExistingPendingImport()
    {
        await using var dbContext = CreateDbContext();

        var experienceRepository = new ExperienceRepository(dbContext);

        var existing = CreateExperience(Guid.NewGuid(), "Existing Experience");

        await experienceRepository.AddOrReplaceAsync(existing, cancellationToken: TestContext.Current.CancellationToken);

        var service = CreateService(dbContext);

        var firstImport = CreateImport(CreateImportedExperience(existing.Id.Value.ToString(), "First Imported Version"));

        var firstResult = await service.ImportAsync(firstImport, TestContext.Current.CancellationToken);

        Assert.Equal(0, firstResult.AddedCount);
        Assert.Equal(0, firstResult.SkippedCount);
        Assert.Equal(1, firstResult.PendingCount);

        var secondImport = CreateImport(CreateImportedExperience(existing.Id.Value.ToString(), "Second Imported Version"));

        var secondResult = await service.ImportAsync(secondImport, TestContext.Current.CancellationToken);

        Assert.Equal(0, secondResult.AddedCount);
        Assert.Equal(0, secondResult.SkippedCount);
        Assert.Equal(1, secondResult.PendingCount);

        var pendingRepository = new PendingExperienceImportRepository(dbContext);

        var pending = await pendingRepository.GetAsync(existing.Id, TestContext.Current.CancellationToken);

        Assert.NotNull(pending);
        Assert.Equal(existing.Id, pending.Id);
        Assert.Equal("Second Imported Version", pending.Title);

        var pendingCount = await pendingRepository.GetCountAsync(TestContext.Current.CancellationToken);

        Assert.Equal(1, pendingCount);

        var persistedExisting = await experienceRepository.GetAsync(existing.Id, TestContext.Current.CancellationToken);

        Assert.NotNull(persistedExisting);
        Assert.Equal("Existing Experience", persistedExisting.Title);
    }

    private ExperienceImportService CreateService(JobApplicationHelperDbContext dbContext)
    {
        IExperienceRepository experienceRepository = new ExperienceRepository(dbContext);

        IPendingExperienceImportRepository pendingImportRepository = new PendingExperienceImportRepository(dbContext);

        return new ExperienceImportService(experienceRepository, pendingImportRepository);
    }

    private JobApplicationHelperDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<JobApplicationHelperDbContext>()
            .UseNpgsql(fixture.ConnectionString)
            .Options;

        return new JobApplicationHelperDbContext(options);
    }

    private static ExperienceBankImportModel CreateImport(params ExperienceImportModel[] experiences)
    {
        return new ExperienceBankImportModel
        {
            Version = 1,
            Experiences = [.. experiences]
        };
    }

    private static ExperienceImportModel CreateImportedExperience(string id, string title)
    {
        return new ExperienceImportModel
        {
            Id = id,
            Title = title,
            Type = ExperienceType.Professional,
            Organization = "Example Organization",
            DateRange = new DateRange
            {
                Start = new PartialDate
                {
                    Year = 2024,
                    Month = 1
                },
                End = new PartialDate
                {
                    Year = 2025,
                    Month = 6
                }
            },
            Summary = "Example summary.",
            Skills =
            [
                "C#",
                "PostgreSQL"
            ],
            Evidence =
            [
                "Example evidence."
            ],
            Contexts =
            [
                "Backend"
            ],
            Links =
            [
                "https://example.com"
            ],
            Notes = "Example notes."
        };
    }

    private static Experience CreateExperience(Guid id, string title)
    {
        return new Experience
        {
            Id = new ExperienceId(id),
            Title = title,
            Type = ExperienceType.Professional,
            Organization = "Example Organization",
            DateRange = new DateRange
            {
                Start = new PartialDate
                {
                    Year = 2024,
                    Month = 1
                },
                End = new PartialDate
                {
                    Year = 2025,
                    Month = 6
                }
            },
            Summary = "Example summary.",
            Skills =
            [
                "C#",
                "PostgreSQL"
            ],
            Evidence =
            [
                "Example evidence."
            ],
            Contexts =
            [
                "Backend"
            ],
            Links =
            [
                "https://example.com"
            ],
            Notes = "Example notes."
        };
    }
}