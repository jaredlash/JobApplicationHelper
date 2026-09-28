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

    [Fact]
    public async Task Processes_high_priority_job_before_normal_priority_job()
    {
        var repository = new FakeBackgroundJobRepository();
        var queue = new BackgroundJobQueue();
        var executor = new RecordingBackgroundJobExecutor();

        var service = new BackgroundJobService(repository, queue, executor);

        var worker = new BackgroundJobWorker(
            service,
            queue,
            Options.Create(new BackgroundJobOptions { MaxConcurrency = 1 }),
            NullLogger<BackgroundJobWorker>.Instance);

        var normalJobId = await service.CreateAsync(BackgroundJobType.Llm, BackgroundJobPriority.Normal, TestContext.Current.CancellationToken);

        var highJobId = await service.CreateAsync(BackgroundJobType.Llm, BackgroundJobPriority.High, TestContext.Current.CancellationToken);

        using var cancellationSource = new CancellationTokenSource();

        await worker.StartAsync(cancellationSource.Token);

        await WaitForJobStatusAsync(service, highJobId, BackgroundJobStatus.Completed);

        await WaitForJobStatusAsync(service, normalJobId, BackgroundJobStatus.Completed);

        Assert.Equal([highJobId, normalJobId], executor.ExecutedJobs);

        await worker.StopAsync(CancellationToken.None);
    }

    [Fact]
    public async Task Recovers_running_job_and_processes_it()
    {
        var repository = new FakeBackgroundJobRepository();
        var queue = new BackgroundJobQueue();
        var executor = new TestBackgroundJobExecutor();

        var service = new BackgroundJobService(repository, queue, executor);

        var job = BackgroundJob.Create(BackgroundJobType.Llm, BackgroundJobPriority.Normal);

        job.Start();

        await repository.AddAsync(job, TestContext.Current.CancellationToken);

        var worker = new BackgroundJobWorker(
            service,
            queue,
            Options.Create(new BackgroundJobOptions { MaxConcurrency = 1 }),
            NullLogger<BackgroundJobWorker>.Instance);

        using var cancellationSource = new CancellationTokenSource();

        await worker.StartAsync(cancellationSource.Token);

        var executedJob = await executor.Completion
            .WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        Assert.Equal(job.Id, executedJob.Id);

        var completedJob = await WaitForJobStatusAsync(
            service,
            job.Id,
            BackgroundJobStatus.Completed);

        Assert.NotNull(completedJob);
        Assert.Equal(BackgroundJobStatus.Completed, completedJob.Status);

        await worker.StopAsync(CancellationToken.None);
    }

    [Fact]
    public async Task Does_not_execute_more_jobs_than_max_concurrency()
    {
        var repository = new FakeBackgroundJobRepository();
        var queue = new BackgroundJobQueue();
        var executor = new BlockingBackgroundJobExecutor();

        var service = new BackgroundJobService(
            repository,
            queue,
            executor);

        var worker = new BackgroundJobWorker(
            service,
            queue,
            Options.Create(new BackgroundJobOptions { MaxConcurrency = 1 }),
            NullLogger<BackgroundJobWorker>.Instance);

        var firstJobId = await service.CreateAsync(BackgroundJobType.Llm, BackgroundJobPriority.Normal, TestContext.Current.CancellationToken);

        var secondJobId = await service.CreateAsync(BackgroundJobType.Llm, BackgroundJobPriority.Normal, TestContext.Current.CancellationToken);

        using var cancellationSource = new CancellationTokenSource();

        await worker.StartAsync(cancellationSource.Token);

        await executor.FirstJobStarted.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        // Give the worker a little time to demonstrate that the
        // second job is not started concurrently.
        await Task.Delay(100, TestContext.Current.CancellationToken);

        Assert.Equal(1, executor.MaximumActiveJobs);

        executor.Release();

        await WaitForJobStatusAsync(service, firstJobId, BackgroundJobStatus.Completed);

        await WaitForJobStatusAsync(service, secondJobId, BackgroundJobStatus.Completed);

        Assert.Equal(1, executor.MaximumActiveJobs);

        await worker.StopAsync(CancellationToken.None);
    }

    [Fact]
    public async Task Continues_processing_after_job_failure()
    {
        var repository = new FakeBackgroundJobRepository();
        var queue = new BackgroundJobQueue();
        var executor = new FailOnceBackgroundJobExecutor();

        var service = new BackgroundJobService(repository, queue, executor);

        var worker = new BackgroundJobWorker(
            service,
            queue,
            Options.Create(new BackgroundJobOptions { MaxConcurrency = 1 }),
            NullLogger<BackgroundJobWorker>.Instance);

        var failedJobId = await service.CreateAsync(BackgroundJobType.Llm, BackgroundJobPriority.Normal, TestContext.Current.CancellationToken);

        var successfulJobId = await service.CreateAsync(BackgroundJobType.Llm, BackgroundJobPriority.Normal, TestContext.Current.CancellationToken);

        using var cancellationSource = new CancellationTokenSource();

        await worker.StartAsync(cancellationSource.Token);

        var failedJob = await WaitForJobStatusAsync(service, failedJobId, BackgroundJobStatus.Failed);

        Assert.Equal(BackgroundJobStatus.Failed, failedJob.Status);

        var successfulJob = await WaitForJobStatusAsync(service, successfulJobId, BackgroundJobStatus.Completed);

        Assert.Equal(BackgroundJobStatus.Completed, successfulJob.Status);

        Assert.Equal(2, executor.ExecutionCount);

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