using JobApplicationHelper.Domain.Models;

namespace JobApplicationHelper.Application.Repositories;

public interface IJobApplicationRepository
{
    Task AddAsync(JobApplication jobApplication, CancellationToken cancellationToken = default);

    //Task UpdateAsync(JobApplication jobApplication, CancellationToken cancellationToken = default);

    Task<JobApplication?> GetAsync(JobApplicationId id, CancellationToken cancellationToken = default);


    Task DeleteAsync(JobApplicationId id, CancellationToken cancellationToken = default);
}