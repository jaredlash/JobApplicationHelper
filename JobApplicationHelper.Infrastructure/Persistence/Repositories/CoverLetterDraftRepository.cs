using JobApplicationHelper.Application.Repositories;
using JobApplicationHelper.Domain.Models;
using JobApplicationHelper.Infrastructure.Persistence.Mapping;
using Microsoft.EntityFrameworkCore;

namespace JobApplicationHelper.Infrastructure.Persistence.Repositories;

public sealed class CoverLetterDraftRepository(JobApplicationHelperDbContext dbContext)
    : ICoverLetterDraftRepository
{
    public async Task AddOrReplaceAsync(CoverLetterDraft draft, CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.CoverLetterDrafts
            .SingleOrDefaultAsync(
                x => x.JobApplicationId == draft.JobApplicationId.Value,
                cancellationToken);

        if (entity is null)
        {
            await dbContext.CoverLetterDrafts.AddAsync(draft.ToEntity(), cancellationToken);
        }
        else
        {
            entity.UpdateEntity(draft);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<CoverLetterDraft?> GetAsync(
        JobApplicationId jobApplicationId,
        CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.CoverLetterDrafts
            .AsNoTracking()
            .SingleOrDefaultAsync(
                x => x.JobApplicationId == jobApplicationId.Value,
                cancellationToken);

        return entity?.ToDomain();
    }

    public async Task DeleteAsync(
        JobApplicationId jobApplicationId,
        CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.CoverLetterDrafts
            .SingleOrDefaultAsync(
                x => x.JobApplicationId == jobApplicationId.Value,
                cancellationToken);

        if (entity is null)
        {
            return;
        }

        dbContext.CoverLetterDrafts.Remove(entity);

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}