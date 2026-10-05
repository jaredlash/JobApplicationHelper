using JobApplicationHelper.Domain.Models;
using JobApplicationHelper.Infrastructure.Persistence;
using JobApplicationHelper.Infrastructure.Persistence.Repositories;
using JobApplicationHelper.Infrastructure.Persistence.Mapping;
using Microsoft.EntityFrameworkCore;

namespace JobApplicationHelper.IntegrationTests.ExperienceBank;

public sealed class ExperienceRepositoryTests
    : IClassFixture<PostgreSqlFixture>
{
    private readonly PostgreSqlFixture database;

    public ExperienceRepositoryTests(PostgreSqlFixture database)
    {
        this.database = database;
    }

    private JobApplicationHelperDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<JobApplicationHelperDbContext>().UseNpgsql(database.ConnectionString).Options;

        return new JobApplicationHelperDbContext(options);
    }

    [Fact]
    public async Task Adds_and_retrieves_experience()
    {
        var experience = CreateExperience();

        await using (var dbContext = CreateDbContext())
        {
            var repository = new ExperienceRepository(dbContext);

            await repository.AddOrReplaceAsync(experience, TestContext.Current.CancellationToken);
        }

        await using var verificationContext = CreateDbContext();

        var verificationRepository = new ExperienceRepository(verificationContext);

        var retrieved = await verificationRepository.GetAsync(experience.Id, TestContext.Current.CancellationToken);

        Assert.NotNull(retrieved);

        Assert.Equal(experience.Id, retrieved.Id);
        Assert.Equal(experience.Title, retrieved.Title);
        Assert.Equal(experience.Type, retrieved.Type);
        Assert.Equal(experience.Organization, retrieved.Organization);
        Assert.Equal(experience.Summary, retrieved.Summary);
        Assert.Equal(experience.Notes, retrieved.Notes);

        AssertDateRangeEqual(experience.DateRange, retrieved.DateRange);

        Assert.Equal(experience.Evidence, retrieved.Evidence);

        Assert.Equal(experience.Skills, retrieved.Skills);

        Assert.Equal(experience.Contexts, retrieved.Contexts);

        Assert.Equal(experience.Links, retrieved.Links);
    }

    [Fact]
    public async Task Returns_null_when_experience_does_not_exist()
    {
        await using var dbContext = CreateDbContext();

        var repository = new ExperienceRepository(dbContext);

        var experience = await repository.GetAsync(new ExperienceId(Guid.NewGuid()), TestContext.Current.CancellationToken);

        Assert.Null(experience);
    }

    [Fact]
    public async Task Gets_all_experiences()
    {
        var first = CreateExperience(title: $"First Experience {Guid.NewGuid()}");

        var second = CreateExperience(title: $"Second Experience {Guid.NewGuid()}");

        var third = CreateExperience(title: $"Third Experience {Guid.NewGuid()}");

        await using (var dbContext = CreateDbContext())
        {
            var repository = new ExperienceRepository(dbContext);

            await repository.AddOrReplaceAsync(third, TestContext.Current.CancellationToken);

            await repository.AddOrReplaceAsync(first, TestContext.Current.CancellationToken);

            await repository.AddOrReplaceAsync(second, TestContext.Current.CancellationToken);
        }

        await using var verificationContext = CreateDbContext();

        var verificationRepository = new ExperienceRepository(verificationContext);

        var experiences = await verificationRepository.GetAllAsync(TestContext.Current.CancellationToken);

        Assert.Contains(experiences, experience => experience.Id == first.Id);

        Assert.Contains(experiences, experience => experience.Id == second.Id);

        Assert.Contains(experiences, experience => experience.Id == third.Id);

        var retrievedTitles = experiences
            .Where(e =>
                e.Id == first.Id ||
                e.Id == second.Id ||
                e.Id == third.Id)
            .Select(e => e.Title)
            .ToList();

        Assert.Equal([first.Title, second.Title, third.Title], retrievedTitles);
    }

    [Fact]
    public async Task Replaces_existing_experience()
    {
        var experienceId = Guid.NewGuid();

        var first = CreateExperience(
            id: experienceId,
            title: "Original Experience",
            organization: "Original Organization",
            summary: "Original summary.",
            notes: "Original notes.",
            evidence:
            [
                "Original evidence"
            ],
            skills:
            [
                "C#",
                "PostgreSQL"
            ],
            contexts:
            [
                "Backend",
                "Database"
            ],
            links:
            [
                "https://example.com/original"
            ]);

        await using (var dbContext = CreateDbContext())
        {
            var repository = new ExperienceRepository(dbContext);

            await repository.AddOrReplaceAsync(first, TestContext.Current.CancellationToken);
        }

        var replacement = CreateExperience(
            id: experienceId,
            title: "Updated Experience",
            organization: "Updated Organization",
            summary: "Updated summary.",
            notes: "Updated notes.",
            evidence:
            [
                "Updated evidence one",
                "Updated evidence two"
            ],
            skills:
            [
                "C#",
                "ASP.NET Core"
            ],
            contexts:
            [
                "Web Development"
            ],
            links:
            [
                "https://example.com/updated"
            ]);

        await using (var dbContext = CreateDbContext())
        {
            var repository = new ExperienceRepository(dbContext);

            await repository.AddOrReplaceAsync(replacement, TestContext.Current.CancellationToken);
        }

        await using var verificationContext = CreateDbContext();

        var verificationRepository = new ExperienceRepository(verificationContext);

        var retrieved = await verificationRepository.GetAsync(new ExperienceId(experienceId), TestContext.Current.CancellationToken);

        Assert.NotNull(retrieved);

        Assert.Equal(replacement.Id, retrieved.Id);
        Assert.Equal("Updated Experience", retrieved.Title);
        Assert.Equal("Updated Organization", retrieved.Organization);
        Assert.Equal("Updated summary.", retrieved.Summary);
        Assert.Equal("Updated notes.", retrieved.Notes);

        Assert.Equal(
            [
                "Updated evidence one",
                "Updated evidence two"
            ],
            retrieved.Evidence);

        Assert.Equal(
            [
                "ASP.NET Core",
                "C#"
            ],
            retrieved.Skills.OrderBy(s => s).ToList());

        Assert.Equal(
            [
                "Web Development"
            ],
            retrieved.Contexts);

        Assert.Equal(
            [
                "https://example.com/updated"
            ],
            retrieved.Links);

        var experienceCount = await verificationContext.Experiences.CountAsync(e => e.Id == experienceId, TestContext.Current.CancellationToken);

        Assert.Equal(1, experienceCount);
    }

    [Fact]
    public async Task Replacing_experience_removes_old_dependent_rows()
    {
        var experienceId = Guid.NewGuid();

        var first = CreateExperience(
            id: experienceId,
            evidence:
            [
                "Old evidence one",
                "Old evidence two"
            ],
            skills:
            [
                "C#",
                "PostgreSQL"
            ],
            contexts:
            [
                "Backend",
                "Database"
            ],
            links:
            [
                "https://example.com/one",
                "https://example.com/two"
            ]);

        await using (var dbContext = CreateDbContext())
        {
            var repository = new ExperienceRepository(dbContext);

            await repository.AddOrReplaceAsync(
                first,
                TestContext.Current.CancellationToken);
        }

        var replacement = CreateExperience(
            id: experienceId,
            evidence:
            [
                "New evidence"
            ],
            skills:
            [
                "Python"
            ],
            contexts:
            [
                "Automation"
            ],
            links:
            [
                "https://example.com/new"
            ]);

        await using (var dbContext = CreateDbContext())
        {
            var repository = new ExperienceRepository(dbContext);

            await repository.AddOrReplaceAsync(replacement, TestContext.Current.CancellationToken);
        }

        await using var verificationContext = CreateDbContext();

        var evidenceCount = await verificationContext.ExperienceEvidence
            .CountAsync(e => e.ExperienceId == experienceId, TestContext.Current.CancellationToken);

        var linkCount = await verificationContext.ExperienceLinks
            .CountAsync(e => e.ExperienceId == experienceId, TestContext.Current.CancellationToken);

        var skillCount = await verificationContext.ExperienceSkills
            .CountAsync(e => e.ExperienceId == experienceId, TestContext.Current.CancellationToken);

        var contextCount = await verificationContext.ExperienceContexts
            .CountAsync(e => e.ExperienceId == experienceId, TestContext.Current.CancellationToken);

        Assert.Equal(1, evidenceCount);
        Assert.Equal(1, linkCount);
        Assert.Equal(1, skillCount);
        Assert.Equal(1, contextCount);

        Assert.DoesNotContain(
            await verificationContext.ExperienceEvidence
                .Where(e => e.ExperienceId == experienceId)
                .Select(e => e.Text)
                .ToListAsync(TestContext.Current.CancellationToken),
            text => text.StartsWith("Old"));

        Assert.DoesNotContain(
            await verificationContext.ExperienceLinks
                .Where(e => e.ExperienceId == experienceId)
                .Select(e => e.Url)
                .ToListAsync(TestContext.Current.CancellationToken),
            url => url.Contains("/one") || url.Contains("/two"));
    }

    [Fact]
    public async Task Reuses_existing_skill_when_name_differs_only_by_case()
    {
        var first = CreateExperience(skills: ["C#"]);

        await using (var dbContext = CreateDbContext())
        {
            var repository = new ExperienceRepository(dbContext);

            await repository.AddOrReplaceAsync(first, TestContext.Current.CancellationToken);
        }

        var second = CreateExperience(skills: ["c#"]);

        await using (var dbContext = CreateDbContext())
        {
            var repository = new ExperienceRepository(dbContext);

            await repository.AddOrReplaceAsync(second, TestContext.Current.CancellationToken);
        }

        await using var verificationContext = CreateDbContext();

        var skills = await verificationContext.Skills
            .Where(s => s.Name == "C#")
            .ToListAsync(TestContext.Current.CancellationToken);

        Assert.Single(skills);

        var skillUsageCount = await verificationContext.ExperienceSkills
            .CountAsync(es => es.SkillId == skills[0].Id, TestContext.Current.CancellationToken);

        Assert.Equal(2, skillUsageCount);
    }

    [Fact]
    public async Task Reuses_existing_context_when_name_differs_only_by_case()
    {
        var first = CreateExperience(contexts: ["Professional"]);

        await using (var dbContext = CreateDbContext())
        {
            var repository = new ExperienceRepository(dbContext);

            await repository.AddOrReplaceAsync(first, TestContext.Current.CancellationToken);
        }

        var second = CreateExperience(contexts: ["professional"]);

        await using (var dbContext = CreateDbContext())
        {
            var repository = new ExperienceRepository(dbContext);

            await repository.AddOrReplaceAsync(second, TestContext.Current.CancellationToken);
        }

        await using var verificationContext = CreateDbContext();

        var contexts = await verificationContext.Contexts
            .Where(c => c.Name == "Professional")
            .ToListAsync(TestContext.Current.CancellationToken);

        Assert.Single(contexts);

        var contextUsageCount = await verificationContext.ExperienceContexts
            .CountAsync(ec => ec.ContextId == contexts[0].Id, TestContext.Current.CancellationToken);

        Assert.Equal(2, contextUsageCount);
    }

    [Fact]
    public async Task Does_not_create_duplicate_skills_within_an_experience()
    {
        var experience = CreateExperience(
            skills:
            [
                "C#",
                "c#",
                " C# ",
                "PostgreSQL"
            ]);

        await using (var dbContext = CreateDbContext())
        {
            var repository = new ExperienceRepository(dbContext);

            await repository.AddOrReplaceAsync(experience, TestContext.Current.CancellationToken);
        }

        await using var verificationContext = CreateDbContext();

        var relationships = await verificationContext.ExperienceSkills
            .Where(es => es.ExperienceId == experience.Id.Value)
            .ToListAsync(TestContext.Current.CancellationToken);

        Assert.Equal(2, relationships.Count);

        var skillNames = await verificationContext.ExperienceSkills
            .Where(es => es.ExperienceId == experience.Id.Value)
            .Join(
                verificationContext.Skills,
                es => es.SkillId,
                skill => skill.Id,
                (_, skill) => skill.Name)
            .OrderBy(name => name)
            .ToListAsync(TestContext.Current.CancellationToken);

        Assert.Equal(
            [
                "C#",
                "PostgreSQL"
            ],
            skillNames);
    }

    [Fact]
    public async Task Does_not_create_duplicate_contexts_within_an_experience()
    {
        var experience = CreateExperience(
            contexts:
            [
                "Backend",
                "backend",
                " Backend ",
                "Database"
            ]);

        await using (var dbContext = CreateDbContext())
        {
            var repository = new ExperienceRepository(dbContext);

            await repository.AddOrReplaceAsync(experience, TestContext.Current.CancellationToken);
        }

        await using var verificationContext = CreateDbContext();

        var relationships = await verificationContext.ExperienceContexts
            .Where(ec => ec.ExperienceId == experience.Id.Value)
            .ToListAsync(TestContext.Current.CancellationToken);

        Assert.Equal(2, relationships.Count);

        var contextNames = await verificationContext.ExperienceContexts
            .Where(ec => ec.ExperienceId == experience.Id.Value)
            .Join(
                verificationContext.Contexts,
                ec => ec.ContextId,
                context => context.Id,
                (_, context) => context.Name)
            .OrderBy(name => name)
            .ToListAsync(TestContext.Current.CancellationToken);

        Assert.Equal(
            [
                "Backend",
                "Database"
            ],
            contextNames);
    }

    [Fact]
    public async Task Preserves_evidence_order()
    {
        var experience = CreateExperience(
            evidence:
            [
                "First evidence",
                "Second evidence",
                "Third evidence"
            ]);

        await using (var dbContext = CreateDbContext())
        {
            var repository = new ExperienceRepository(dbContext);

            await repository.AddOrReplaceAsync(experience, TestContext.Current.CancellationToken);
        }

        await using var verificationContext = CreateDbContext();

        var verificationRepository = new ExperienceRepository(verificationContext);

        var retrieved = await verificationRepository.GetAsync(new ExperienceId(experience.Id.Value), TestContext.Current.CancellationToken);

        Assert.NotNull(retrieved);

        Assert.Equal(
            [
                "First evidence",
                "Second evidence",
                "Third evidence"
            ],
            retrieved.Evidence);
    }

    [Fact]
    public async Task Preserves_link_order()
    {
        var experience = CreateExperience(
            links:
            [
                "https://example.com/first",
                "https://example.com/second",
                "https://example.com/third"
            ]);

        await using (var dbContext = CreateDbContext())
        {
            var repository = new ExperienceRepository(dbContext);

            await repository.AddOrReplaceAsync(experience, TestContext.Current.CancellationToken);
        }

        await using var verificationContext = CreateDbContext();

        var verificationRepository = new ExperienceRepository(verificationContext);

        var retrieved = await verificationRepository.GetAsync(new ExperienceId(experience.Id.Value), TestContext.Current.CancellationToken);

        Assert.NotNull(retrieved);

        Assert.Equal(
            [
                "https://example.com/first",
                "https://example.com/second",
                "https://example.com/third"
            ],
            retrieved.Links);
    }

    [Fact]
    public async Task Supports_experience_with_empty_collections()
    {
        var experience = CreateExperience(evidence: [], skills: [], contexts: [], links: []);

        await using (var dbContext = CreateDbContext())
        {
            var repository = new ExperienceRepository(dbContext);

            await repository.AddOrReplaceAsync(experience, TestContext.Current.CancellationToken);
        }

        await using var verificationContext = CreateDbContext();

        var verificationRepository = new ExperienceRepository(verificationContext);

        var retrieved = await verificationRepository.GetAsync(new ExperienceId(experience.Id.Value), TestContext.Current.CancellationToken);

        Assert.NotNull(retrieved);

        Assert.Empty(retrieved.Evidence);
        Assert.Empty(retrieved.Skills);
        Assert.Empty(retrieved.Contexts);
        Assert.Empty(retrieved.Links);
    }

    [Fact]
    public async Task Round_trips_complete_date_range()
    {
        var experience = CreateExperience(
            dateRange: new DateRange
            {
                Start = new PartialDate
                {
                    Year = 2020,
                    Month = 3,
                    Day = 15
                },
                End = new PartialDate
                {
                    Year = 2024,
                    Month = 11,
                    Day = 20
                }
            });

        await using (var dbContext = CreateDbContext())
        {
            var repository = new ExperienceRepository(dbContext);

            await repository.AddOrReplaceAsync(experience, TestContext.Current.CancellationToken);
        }

        await using var verificationContext = CreateDbContext();

        var verificationRepository = new ExperienceRepository(verificationContext);

        var retrieved = await verificationRepository.GetAsync(new ExperienceId(experience.Id.Value), TestContext.Current.CancellationToken);

        Assert.NotNull(retrieved);

        AssertDateRangeEqual(experience.DateRange, retrieved.DateRange);
    }

    [Fact]
    public async Task Round_trips_start_only_date_range()
    {
        var experience = CreateExperience(
            dateRange: new DateRange
            {
                Start = new PartialDate
                {
                    Year = 2020,
                    Month = 3,
                    Day = 15
                }
            });

        await using (var dbContext = CreateDbContext())
        {
            var repository = new ExperienceRepository(dbContext);

            await repository.AddOrReplaceAsync(experience, TestContext.Current.CancellationToken);
        }

        await using var verificationContext = CreateDbContext();

        var verificationRepository = new ExperienceRepository(verificationContext);

        var retrieved = await verificationRepository.GetAsync(new ExperienceId(experience.Id.Value), TestContext.Current.CancellationToken);

        Assert.NotNull(retrieved);

        AssertDateRangeEqual(experience.DateRange, retrieved.DateRange);
    }

    [Fact]
    public async Task Round_trips_end_only_date_range()
    {
        var experience = CreateExperience(
            dateRange: new DateRange
            {
                End = new PartialDate
                {
                    Year = 2024,
                    Month = 11,
                    Day = 20
                }
            });

        await using (var dbContext = CreateDbContext())
        {
            var repository = new ExperienceRepository(dbContext);

            await repository.AddOrReplaceAsync(experience, TestContext.Current.CancellationToken);
        }

        await using var verificationContext = CreateDbContext();

        var verificationRepository = new ExperienceRepository(verificationContext);

        var retrieved = await verificationRepository.GetAsync(new ExperienceId(experience.Id.Value), TestContext.Current.CancellationToken);

        Assert.NotNull(retrieved);

        AssertDateRangeEqual(experience.DateRange, retrieved.DateRange);
    }

    [Fact]
    public async Task Round_trips_year_only_partial_dates()
    {
        var experience = CreateExperience(
            dateRange: new DateRange
            {
                Start = new PartialDate
                {
                    Year = 2020
                },
                End = new PartialDate
                {
                    Year = 2024
                }
            });

        await using (var dbContext = CreateDbContext())
        {
            var repository = new ExperienceRepository(dbContext);

            await repository.AddOrReplaceAsync(experience, TestContext.Current.CancellationToken);
        }

        await using var verificationContext = CreateDbContext();

        var verificationRepository = new ExperienceRepository(verificationContext);

        var retrieved = await verificationRepository.GetAsync(new ExperienceId(experience.Id.Value), TestContext.Current.CancellationToken);

        Assert.NotNull(retrieved);

        AssertDateRangeEqual(experience.DateRange, retrieved.DateRange);
    }

    [Fact]
    public async Task Deletes_experience()
    {
        var experience = CreateExperience();

        await using (var dbContext = CreateDbContext())
        {
            var repository = new ExperienceRepository(dbContext);

            await repository.AddOrReplaceAsync(experience, TestContext.Current.CancellationToken);
        }

        await using (var dbContext = CreateDbContext())
        {
            var repository = new ExperienceRepository(dbContext);

            await repository.DeleteAsync(experience.Id, TestContext.Current.CancellationToken);
        }

        await using var verificationContext = CreateDbContext();

        var verificationRepository = new ExperienceRepository(verificationContext);

        var retrieved = await verificationRepository.GetAsync(new ExperienceId(experience.Id.Value), TestContext.Current.CancellationToken);

        Assert.Null(retrieved);

        var evidenceCount = await verificationContext.ExperienceEvidence
            .CountAsync(e => e.ExperienceId == experience.Id.Value, TestContext.Current.CancellationToken);

        var linkCount = await verificationContext.ExperienceLinks
            .CountAsync(e => e.ExperienceId == experience.Id.Value, TestContext.Current.CancellationToken);

        var skillRelationshipCount = await verificationContext.ExperienceSkills
            .CountAsync(e => e.ExperienceId == experience.Id.Value, TestContext.Current.CancellationToken);

        var contextRelationshipCount = await verificationContext.ExperienceContexts
            .CountAsync(e => e.ExperienceId == experience.Id.Value, TestContext.Current.CancellationToken);

        Assert.Equal(0, evidenceCount);
        Assert.Equal(0, linkCount);
        Assert.Equal(0, skillRelationshipCount);
        Assert.Equal(0, contextRelationshipCount);
    }

    [Fact]
    public async Task Does_not_create_duplicate_experiences_when_add_or_replace_is_called_with_same_id()
    {
        var experienceId = Guid.NewGuid();

        var first = CreateExperience(id: experienceId, title: "First version");

        var second = CreateExperience(id: experienceId, title: "Second version");

        await using (var dbContext = CreateDbContext())
        {
            var repository = new ExperienceRepository(dbContext);

            await repository.AddOrReplaceAsync(first, TestContext.Current.CancellationToken);
        }

        await using (var dbContext = CreateDbContext())
        {
            var repository = new ExperienceRepository(dbContext);

            await repository.AddOrReplaceAsync(second, TestContext.Current.CancellationToken);
        }

        await using var verificationContext = CreateDbContext();

        var experienceCount = await verificationContext.Experiences
            .CountAsync(e => e.Id == experienceId, TestContext.Current.CancellationToken);

        Assert.Equal(1, experienceCount);

        var verificationRepository = new ExperienceRepository(verificationContext);

        var retrieved = await verificationRepository.GetAsync(new ExperienceId(experienceId), TestContext.Current.CancellationToken);

        Assert.NotNull(retrieved);
        Assert.Equal("Second version", retrieved.Title);
    }

    private static Experience CreateExperience(
        Guid? id = null,
        string title = "Test Experience",
        ExperienceType type = ExperienceType.Professional,
        string organization = "Test Organization",
        DateRange? dateRange = null,
        string summary = "Test experience summary.",
        List<string>? skills = null,
        List<string>? evidence = null,
        List<string>? contexts = null,
        List<string>? links = null,
        string notes = "Test notes.")
    {
        return new Experience
        {
            Id = new ExperienceId(id ?? Guid.NewGuid()),
            Title = title,
            Type = type,
            Organization = organization,
            DateRange = dateRange ?? new DateRange
            {
                Start = new PartialDate
                {
                    Year = 2020,
                    Month = 1,
                    Day = 15
                },
                End = new PartialDate
                {
                    Year = 2024,
                    Month = 6,
                    Day = 30
                }
            },
            Summary = summary,
            Skills = skills ?? ["C#", "PostgreSQL"],
            Evidence = evidence ?? ["Built a test application."],
            Contexts = contexts ?? ["Backend Development"],
            Links = links ?? ["https://example.com/project"],
            Notes = notes
        };
    }

    private static void AssertDateRangeEqual(DateRange? expected, DateRange? actual)
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

    private static void AssertPartialDateEqual(PartialDate? expected, PartialDate? actual)
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