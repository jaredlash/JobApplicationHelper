namespace JobApplicationHelper.Contracts.BackgroundJobs;

public interface IBackgroundJobClient
{
    Task BackgroundJobStatusChanged(BackgroundJobStatusChanged notification);
}