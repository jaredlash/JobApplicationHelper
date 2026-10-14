using JobApplicationHelper.Application.Repositories;
using JobApplicationHelper.Domain.Models;

namespace JobApplicationHelper.Application.Services;

public class PendingExperienceImportService : IPendingExperienceImportService
{
    private readonly IPendingExperienceImportRepository pendingRepository;
    private readonly IExperienceRepository experienceRepository;

    public PendingExperienceImportService(IPendingExperienceImportRepository pendingExperienceImportRepository, IExperienceRepository experienceRepository)
    {
        this.pendingRepository = pendingExperienceImportRepository;
        this.experienceRepository = experienceRepository;
    }

    public async Task<int> GetCountAsync(CancellationToken cancellationToken = default)
    {
        return await pendingRepository.GetCountAsync(cancellationToken);
    }

    public async Task<PendingExperienceImport?> GetNextAsync(CancellationToken cancellationToken = default)
    {
        while (true)
        {
            var id = await pendingRepository.GetNextPendingImportAsync(cancellationToken);

            if (id is null)
            {
                return null;
            }

            var imported = await pendingRepository.GetAsync(id.Value, cancellationToken);

            var current = await experienceRepository.GetAsync(id.Value, cancellationToken);

            if (imported is null || current is null)
                continue;

            if (ExperienceComparer.AreEquivalent(current, imported))
            {
                await pendingRepository.DeleteAsync(id.Value, cancellationToken);

                continue;
            }

            return new PendingExperienceImport(current, imported);
        }
    }

    public async Task ResolveAsync(ExperienceId id, PendingExperienceImportResolution resolution, CancellationToken cancellationToken = default)
    {
        var importedExperience = await pendingRepository.GetAsync(id, cancellationToken);

        if (importedExperience is null)
        {
            return;
        }

        if (resolution == PendingExperienceImportResolution.Replace)
        {
            await experienceRepository.AddOrReplaceAsync(importedExperience, cancellationToken: cancellationToken);
        }

        await pendingRepository.DeleteAsync(id, cancellationToken);
    }
}
