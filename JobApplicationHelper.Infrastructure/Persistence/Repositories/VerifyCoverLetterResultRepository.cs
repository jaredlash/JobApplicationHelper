using JobApplicationHelper.Application.Repositories;
using JobApplicationHelper.Domain.Models;
using JobApplicationHelper.Infrastructure.Persistence.Mapping;
using Microsoft.EntityFrameworkCore;

namespace JobApplicationHelper.Infrastructure.Persistence.Repositories;

public class VerifyCoverLetterResultRepository(
    JobApplicationHelperDbContext dbContext)
    : IVerifyCoverLetterResultRepository
{
    public async Task AddOrReplaceAsync(VerifyCoverLetterResult result, CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.VerifyCoverLetterResults
            .SingleOrDefaultAsync(x => x.JobApplicationId == result.JobApplicationId.Value, cancellationToken);

        if (entity is null)
        {
            await dbContext.VerifyCoverLetterResults.AddAsync(result.ToEntity(), cancellationToken);
        }
        else
        {
            entity.UpdateEntity(result);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }


    public async Task<VerifyCoverLetterResult?> GetAsync(JobApplicationId jobApplicationId, CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.VerifyCoverLetterResults
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.JobApplicationId == jobApplicationId.Value, cancellationToken);

        return entity?.ToDomain();
    }


    public async Task DeleteAsync(JobApplicationId jobApplicationId, CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.VerifyCoverLetterResults
            .SingleOrDefaultAsync(x => x.JobApplicationId == jobApplicationId.Value, cancellationToken);

        if (entity is null)
        {
            return;
        }

        dbContext.VerifyCoverLetterResults.Remove(entity);

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
