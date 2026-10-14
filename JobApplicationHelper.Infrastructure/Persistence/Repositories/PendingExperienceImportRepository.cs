using System.Text.Json;
using JobApplicationHelper.Application.Repositories;
using JobApplicationHelper.Application.Services;
using JobApplicationHelper.Domain.Models;
using JobApplicationHelper.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace JobApplicationHelper.Infrastructure.Persistence.Repositories;

public sealed class PendingExperienceImportRepository : IPendingExperienceImportRepository
{
    private readonly JobApplicationHelperDbContext dbContext;

    public PendingExperienceImportRepository(JobApplicationHelperDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task<int> GetCountAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.PendingExperienceImports.CountAsync(cancellationToken);
    }

    public async Task<ExperienceId?> GetNextPendingImportAsync(CancellationToken cancellationToken = default)
    {
        var id = await dbContext.PendingExperienceImports
            .OrderBy(e => e.ExperienceId)
            .Select(e => (Guid?)e.ExperienceId)
            .FirstOrDefaultAsync(cancellationToken);

        return id is null
            ? null
            : new ExperienceId(id.Value);
    }

    public async Task<Experience?> GetAsync(ExperienceId id, CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.PendingExperienceImports
            .SingleOrDefaultAsync(e => e.ExperienceId == id.Value, cancellationToken);

        if (entity is null)
        {
            return null;
        }

        return JsonSerializer.Deserialize<Experience>(entity.SerializedExperience, ApplicationJson.Options)
            ?? throw new InvalidDataException($"Pending experience import '{id.Value}' could not be deserialized.");
    }

    public async Task AddOrReplaceAsync(Experience experience, CancellationToken cancellationToken = default)
    {
        var serializedExperience = JsonSerializer.Serialize(experience, ApplicationJson.Options);

        var entity = await dbContext.PendingExperienceImports
            .SingleOrDefaultAsync(e => e.ExperienceId == experience.Id.Value, cancellationToken);

        if (entity is null)
        {
            dbContext.PendingExperienceImports.Add(new PendingExperienceImportEntity { ExperienceId = experience.Id.Value, SerializedExperience = serializedExperience });
        }
        else
        {
            entity.SerializedExperience = serializedExperience;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(ExperienceId id, CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.PendingExperienceImports
            .SingleOrDefaultAsync(e => e.ExperienceId == id.Value, cancellationToken);

        if (entity is null)
        {
            return;
        }

        dbContext.PendingExperienceImports.Remove(entity);

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}