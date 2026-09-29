namespace JobApplicationHelper.Domain.Models;

public enum BackgroundJobStatus
{
    Pending,
    Running,
    Completed,
    Failed,
    Cancelled
}
