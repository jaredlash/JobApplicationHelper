using JobApplicationHelper.Application.Repositories;
using JobApplicationHelper.Application.Services;
using JobApplicationHelper.Domain.Models;

namespace JobApplicationHelper.Infrastructure.Services;

public sealed class BackgroundJobService(
    IBackgroundJobRepository backgroundJobRepository)
    : IBackgroundJobService
{
    public async Task<BackgroundJobId> CreateAsync(
        BackgroundJobType type,
        CancellationToken cancellationToken = default)
    {
        var job = BackgroundJob.Create(type);

        await backgroundJobRepository.AddAsync(
            job,
            cancellationToken);

        return job.Id;
    }

    public Task<BackgroundJob?> GetAsync(
        BackgroundJobId id,
        CancellationToken cancellationToken = default)
    {
        return backgroundJobRepository.GetAsync(
            id,
            cancellationToken);
    }
}