using JobApplicationHelper.Application.Repositories;
using JobApplicationHelper.Domain.Models;

namespace JobApplicationHelper.Application.Services;

public sealed class ExperienceImportService : IExperienceImportService
{
    private readonly IExperienceRepository experienceRepository;
    private readonly IPendingExperienceImportRepository pendingImportRepository;

    public ExperienceImportService(IExperienceRepository experienceRepository, IPendingExperienceImportRepository pendingImportRepository)
    {
        this.experienceRepository = experienceRepository;
        this.pendingImportRepository = pendingImportRepository;
    }

    public async Task<ExperienceImportResult> ImportAsync(ExperienceBankImportModel import, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(import);

        var addedCount = 0;
        var skippedCount = 0;
        var pendingCount = 0;

        foreach (var importedExperience in import.Experiences)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var (experienceId, legacyId, existing) = await ResolveIdentityAsync(importedExperience.Id, cancellationToken);

            var experience = CreateExperience(importedExperience, experienceId);

            if (existing is null)
            {
                await experienceRepository.AddOrReplaceAsync(experience, legacyId, cancellationToken);

                addedCount++;
                continue;
            }

            if (AreEquivalent(existing, experience))
            {
                skippedCount++;
                continue;
            }

            await pendingImportRepository.AddOrReplaceAsync(experience, cancellationToken);

            pendingCount++;
        }

        return new ExperienceImportResult(addedCount, skippedCount, pendingCount);
    }

    private async Task<(ExperienceId Id, string? LegacyId, Experience? Existing)> ResolveIdentityAsync(string importedId, CancellationToken cancellationToken)
    {
        if (Guid.TryParse(importedId, out var guid))
        {
            var id = new ExperienceId(guid);

            return (id, null, await experienceRepository.GetAsync(id, cancellationToken));
        }

        var existing = await experienceRepository.GetByLegacyIdAsync(importedId, cancellationToken);

        return existing is not null
            ? (existing.Id, importedId, existing)
            : (new ExperienceId(Guid.NewGuid()), importedId, null);
    }

    private static Experience CreateExperience(ExperienceImportModel importedExperience, ExperienceId id)
    {
        return new Experience
        {
            Id = id,
            Title = importedExperience.Title,
            Type = importedExperience.Type,
            Organization = importedExperience.Organization,
            DateRange = importedExperience.DateRange,
            Summary = importedExperience.Summary,
            Skills = [.. importedExperience.Skills],
            Evidence = [.. importedExperience.Evidence],
            Contexts = [.. importedExperience.Contexts],
            Links = [.. importedExperience.Links],
            Notes = importedExperience.Notes
        };
    }

    private static bool AreEquivalent(Experience existing, Experience imported)
    {
        return existing.Title == imported.Title
            && existing.Type == imported.Type
            && existing.Organization == imported.Organization
            && AreEquivalent(existing.DateRange, imported.DateRange)
            && existing.Summary == imported.Summary
            && AreEquivalentUnordered(existing.Skills, imported.Skills)
            && existing.Evidence.SequenceEqual(imported.Evidence)
            && AreEquivalentUnordered(existing.Contexts, imported.Contexts)
            && existing.Links.SequenceEqual(imported.Links)
            && existing.Notes == imported.Notes;
    }

    private static bool AreEquivalentUnordered(IReadOnlyList<string> existing, IReadOnlyList<string> imported)
    {
        return existing.Count == imported.Count
            && existing.ToHashSet(StringComparer.OrdinalIgnoreCase).SetEquals(imported);
    }

    private static bool AreEquivalent(DateRange? existing, DateRange? imported)
    {
        if (existing is null || imported is null)
        {
            return existing is null && imported is null;
        }

        return AreEquivalent(existing.Start, imported.Start)
            && AreEquivalent(existing.End, imported.End);
    }

    private static bool AreEquivalent(PartialDate? existing, PartialDate? imported)
    {
        if (existing is null || imported is null)
        {
            return existing is null && imported is null;
        }

        return existing.Year == imported.Year
            && existing.Month == imported.Month
            && existing.Day == imported.Day;
    }
}