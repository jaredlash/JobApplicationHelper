using JobApplicationHelper.Contracts.BackgroundJobs;
using JobApplicationHelper.Domain.Models;
using JobApplicationHelper.Exceptions;

namespace JobApplicationHelper.Extensions;

public static class BackgroundJobExtensions
{
    public static void EnsureSucceeded(this GetBackgroundJobResponse response)
    {
        if (response.Status == BackgroundJobStatus.Failed.ToString())
        {
            throw new BackgroundJobFailedException(response.Error);
        }

        if (response.Status == BackgroundJobStatus.Cancelled.ToString())
        {
            throw new BackgroundJobCancelledException();
        }
    }
}