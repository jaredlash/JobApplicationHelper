using JobApplicationHelper.Contracts.BackgroundJobs;
using JobApplicationHelper.Domain.Models;

namespace JobApplicationHelper.Services.BackgroundJobs;

public interface IBackgroundJobPollingService
{
    Task<GetBackgroundJobResponse> WaitForCompletionAsync(BackgroundJobId backgroundJobId, CancellationToken cancellationToken = default);
}
