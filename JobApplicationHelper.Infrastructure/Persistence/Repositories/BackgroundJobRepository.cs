using JobApplicationHelper.Application.Repositories;
using JobApplicationHelper.Domain.Models;
using JobApplicationHelper.Infrastructure.Persistence.Mapping;
using Microsoft.EntityFrameworkCore;

namespace JobApplicationHelper.Infrastructure.Persistence.Repositories;

public sealed class BackgroundJobRepository(JobApplicationHelperDbContext dbContext) : IBackgroundJobRepository
{
    public async Task AddAsync(
        BackgroundJob job,
        CancellationToken cancellationToken = default)
    {
        dbContext.BackgroundJobs.Add(job.ToEntity());

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<BackgroundJob?> GetAsync(
        BackgroundJobId id,
        CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.BackgroundJobs
            .AsNoTracking()
            .SingleOrDefaultAsync(
                job => job.Id == id.Value,
                cancellationToken);

        return entity?.ToDomain();
    }

    public async Task<IReadOnlyList<BackgroundJob>> GetPendingAsync(
        CancellationToken cancellationToken = default)
    {
        var entities = await dbContext.BackgroundJobs
            .AsNoTracking()
            .Where(job => job.Status == BackgroundJobStatus.Pending)
            .OrderBy(job => job.CreatedAt)
            .ToListAsync(cancellationToken);

        return entities
            .Select(job => job.ToDomain())
            .ToList();
    }

    public async Task UpdateAsync(
        BackgroundJob job,
        CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.BackgroundJobs
            .SingleOrDefaultAsync(
                existingJob => existingJob.Id == job.Id.Value,
                cancellationToken);

        if (entity is null)
        {
            throw new KeyNotFoundException($"Background job '{job.Id.Value}' was not found.");
        }

        entity.Type = job.Type;
        entity.Status = job.Status;
        entity.CreatedAt = job.CreatedAt;
        entity.StartedAt = job.StartedAt;
        entity.CompletedAt = job.CompletedAt;
        entity.Error = job.Error;

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}