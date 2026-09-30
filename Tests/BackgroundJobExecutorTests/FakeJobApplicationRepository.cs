using JobApplicationHelper.Application.Repositories;
using JobApplicationHelper.Domain.Models;

namespace JobApplicationHelper.Tests.BackgroundJobExecutorTests;

public sealed class FakeJobApplicationRepository(JobApplication? jobApplication = null)
    : IJobApplicationRepository
{
    private readonly JobApplication? jobApplication = jobApplication;

    public JobApplicationId? RequestedId { get; private set; }

    public Task AddAsync(JobApplication jobApplication, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }

    public Task<JobApplication?> GetAsync(JobApplicationId id, CancellationToken cancellationToken = default)
    {
        RequestedId = id;

        var result = jobApplication?.Id == id
            ? jobApplication
            : null;

        return Task.FromResult(result);
    }

    public Task DeleteAsync(JobApplicationId id, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }
}