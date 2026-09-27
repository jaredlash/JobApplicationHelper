using JobApplicationHelper.Application.Repositories;
using JobApplicationHelper.Domain.Models;
using JobApplicationHelper.Infrastructure.Persistence;
using JobApplicationHelper.Infrastructure.Persistence.Mapping;
using Microsoft.EntityFrameworkCore;

namespace JobApplicationHelper.Infrastructure.Persistence.Repositories;

public sealed class JobApplicationRepository(JobApplicationHelperDbContext dbContext) : IJobApplicationRepository
{
    public async Task AddAsync(JobApplication application, CancellationToken cancellationToken = default)
    {
        dbContext.JobApplications.Add(application.ToEntity());

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<JobApplication?> GetAsync(JobApplicationId id, CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.JobApplications.AsNoTracking()
            .SingleOrDefaultAsync(application => application.Id == id.Value, cancellationToken);

        return entity?.ToDomain();
    }

    public async Task DeleteAsync(JobApplicationId id, CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.JobApplications.SingleOrDefaultAsync(application => application.Id == id.Value, cancellationToken);

        if (entity is null)
        {
            return;
        }

        dbContext.JobApplications.Remove(entity);

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}