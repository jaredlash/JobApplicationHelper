using JobApplicationHelper.Domain.Models;

namespace JobApplicationHelper.Application.Services;

public interface IBackgroundJobQueue
{
    ValueTask EnqueueAsync(
        BackgroundJobId jobId,
        BackgroundJobPriority priority,
        DateTime createdAt,
        CancellationToken cancellationToken = default);

    ValueTask<BackgroundJobId> DequeueAsync(
        CancellationToken cancellationToken = default);
}
