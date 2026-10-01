namespace JobApplicationHelper.Services.BackgroundJobs;

public sealed class BackgroundJobPollingOptions
{
    public TimeSpan PollingInterval { get; set; } = TimeSpan.FromSeconds(1);
}