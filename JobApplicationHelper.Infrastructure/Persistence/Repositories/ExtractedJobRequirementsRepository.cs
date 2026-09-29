using JobApplicationHelper.Application.Repositories;
using JobApplicationHelper.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace JobApplicationHelper.Infrastructure.Persistence.Repositories;

public sealed class ExtractedJobRequirementsRepository(JobApplicationHelperDbContext dbContext)
    : IExtractedJobRequirementsRepository
{
    public async Task<ExtractedJobRequirements?> GetAsync(
        JobApplicationId jobApplicationId,
        CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.ExtractedJobRequirements
            .AsNoTracking()
            .SingleOrDefaultAsync(
                x => x.JobApplicationId == jobApplicationId.Value,
                cancellationToken);

        return entity?.ToDomain();
    }

    public async Task ReplaceAsync(
        ExtractedJobRequirements requirements,
        CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.ExtractedJobRequirements
            .SingleOrDefaultAsync(
                x => x.JobApplicationId == requirements.JobApplicationId.Value,
                cancellationToken);

        if (entity is null)
        {
            throw new InvalidOperationException(
                $"No extracted job requirements exist for job application " +
                $"'{requirements.JobApplicationId}'.");
        }

        entity.RequirementsJson =
            System.Text.Json.JsonSerializer.Serialize(requirements.Requirements);

        entity.CreatedAt = requirements.CreatedAt;

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(
        JobApplicationId jobApplicationId,
        CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.ExtractedJobRequirements
            .SingleOrDefaultAsync(
                x => x.JobApplicationId == jobApplicationId.Value,
                cancellationToken);

        if (entity is null)
        {
            return;
        }

        dbContext.ExtractedJobRequirements.Remove(entity);

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task AddOrReplaceAsync(
        ExtractedJobRequirements requirements,
        CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.ExtractedJobRequirements
            .SingleOrDefaultAsync(
                x => x.JobApplicationId == requirements.JobApplicationId.Value,
                cancellationToken);

        if (entity is null)
        {
            await dbContext.ExtractedJobRequirements.AddAsync(
                requirements.ToEntity(),
                cancellationToken);
        }
        else
        {
            entity.UpdateEntity(requirements);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}