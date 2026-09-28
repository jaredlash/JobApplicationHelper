using JobApplicationHelper.Application.Configuration;
using JobApplicationHelper.Application.Services;
using JobApplicationHelper.Domain.Models;
using JobApplicationHelper.Infrastructure.Configuration;
using JobApplicationHelper.Infrastructure.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace JobApplicationHelper.Tests.BackgroundJobs;

public sealed class BackgroundJobWorkerTests
{
    [Fact]
    public async Task Processes_created_background_job()
    {
        var repository = new FakeBackgroundJobRepository();
        var queue = new BackgroundJobQueue();
        var executor = new TestBackgroundJobExecutor();

        var service = new BackgroundJobService(repository, queue, executor);

        var worker = new BackgroundJobWorker(
            service,
            queue,
            Options.Create(new BackgroundJobOptions { MaxConcurrency = 1 }),
            NullLogger<BackgroundJobWorker>.Instance);

        var jobId = await service.CreateAsync(BackgroundJobType.Llm, BackgroundJobPriority.Normal, TestContext.Current.CancellationToken);

        using var cancellationSource = new CancellationTokenSource();

        await worker.StartAsync(cancellationSource.Token);

        var executedJob = await executor.Completion.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        Assert.Equal(jobId, executedJob.Id);
        Assert.Equal(BackgroundJobStatus.Running, executedJob.Status);

        var completedJob = await service.GetAsync(jobId, TestContext.Current.CancellationToken);

        Assert.NotNull(completedJob);
        Assert.Equal(BackgroundJobStatus.Completed, completedJob.Status);

        await worker.StopAsync(CancellationToken.None);
    }

    [Fact]
    public async Task Marks_job_as_failed_when_executor_throws()
    {
        var repository = new FakeBackgroundJobRepository();
        var queue = new BackgroundJobQueue();

        var expectedException = new InvalidOperationException("Test execution failure.");

        var executor = new FailingBackgroundJobExecutor(expectedException);

        var service = new BackgroundJobService(repository, queue, executor);

        var worker = new BackgroundJobWorker(
            service,
            queue,
            Options.Create(new BackgroundJobOptions { MaxConcurrency = 1 }),
            NullLogger<BackgroundJobWorker>.Instance);

        var jobId = await service.CreateAsync(BackgroundJobType.Llm, BackgroundJobPriority.Normal, TestContext.Current.CancellationToken);

        using var cancellationSource = new CancellationTokenSource();

        await worker.StartAsync(cancellationSource.Token);

        var failedJob = await WaitForJobStatusAsync(service, jobId, BackgroundJobStatus.Failed);

        Assert.NotNull(failedJob);
        Assert.Equal(BackgroundJobStatus.Failed, failedJob.Status);
        Assert.Equal(expectedException.Message, failedJob.Error);

        await worker.StopAsync(CancellationToken.None);
    }


    private static async Task<BackgroundJob> WaitForJobStatusAsync(
        IBackgroundJobService service,
        BackgroundJobId jobId,
        BackgroundJobStatus expectedStatus)
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));

        while (true)
        {
            var job = await service.GetAsync(jobId, timeout.Token);

            if (job is not null && job.Status == expectedStatus)
            {
                return job;
            }

            await Task.Delay(TimeSpan.FromMilliseconds(10), timeout.Token);
        }
    }
}