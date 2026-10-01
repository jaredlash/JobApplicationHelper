using JobApplicationHelper.Domain.Models;
using JobApplicationHelper.Infrastructure.Persistence;
using JobApplicationHelper.Infrastructure.Persistence.Mapping;
using JobApplicationHelper.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace JobApplicationHelper.IntegrationTests.BackgroundJobs;

public sealed class BackgroundJobRepositoryTests : IClassFixture<PostgreSqlFixture>
{
    private readonly PostgreSqlFixture database;

    public BackgroundJobRepositoryTests(PostgreSqlFixture database)
    {
        this.database = database;
    }

    private JobApplicationHelperDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<JobApplicationHelperDbContext>().UseNpgsql(database.ConnectionString).Options;
        return new JobApplicationHelperDbContext(options);
    }

    [Fact]
    public async Task Adds_and_retrieves_background_job()
    {
        await using var dbContext = CreateDbContext();

        var repository = new BackgroundJobRepository(dbContext);
        var jobApplication = await CreateJobApplicationAsync(dbContext);

        var jobApplicationId = jobApplication.Id;

        var job = BackgroundJob.Create(BackgroundJobType.Llm, BackgroundJobPriority.High, jobApplicationId, "testing");

        await repository.AddAsync(job);

        var retrievedJob = await repository.GetAsync(job.Id);

        Assert.NotNull(retrievedJob);
        Assert.Equal(job.Id, retrievedJob.Id);
        Assert.Equal(job.Type, retrievedJob.Type);
        Assert.Equal(job.Priority, retrievedJob.Priority);
        Assert.Equal(job.Status, retrievedJob.Status);
        AssertEqualToPostgresPrecision(job.CreatedAt, retrievedJob.CreatedAt);
        Assert.Equal(job.StartedAt, retrievedJob.StartedAt);
        Assert.Equal(job.CompletedAt, retrievedJob.CompletedAt);
        Assert.Equal(job.Error, retrievedJob.Error);
        Assert.Equal(job.JobApplicationId, retrievedJob.JobApplicationId);
        Assert.Equal(job.Payload, retrievedJob.Payload);
    }

    [Fact]
    public async Task Gets_pending_jobs_in_priority_and_creation_order()
    {
        var jobApplicationId = new JobApplicationId(Guid.NewGuid());

        await using (var dbContext = CreateDbContext())
        {
            var jobApplication = await CreateJobApplicationAsync(dbContext, jobApplicationId);
        }

        var normalJob1 = BackgroundJob.Create(BackgroundJobType.Llm, BackgroundJobPriority.Normal, jobApplicationId, "testing");

        await Task.Delay(10);

        var normalJob2 = BackgroundJob.Create(BackgroundJobType.Llm, BackgroundJobPriority.Normal, jobApplicationId, "testing");

        await Task.Delay(10);

        var highJob = BackgroundJob.Create(BackgroundJobType.Llm, BackgroundJobPriority.High, jobApplicationId, "testing");

        await using (var dbContext = CreateDbContext())
        {
            var repository = new BackgroundJobRepository(dbContext);

            await repository.AddAsync(normalJob1);
            await repository.AddAsync(normalJob2);
            await repository.AddAsync(highJob);
        }

        await using var verificationContext = CreateDbContext();
        var verificationRepository = new BackgroundJobRepository(verificationContext);

        var pendingJobs = await verificationRepository.GetPendingAsync();

        Assert.Equal(
            [highJob.Id, normalJob1.Id, normalJob2.Id],
            pendingJobs.Select(job => job.Id).ToArray());
    }


    [Fact]
    public async Task Recovers_running_background_jobs()
    {
        var jobApplicationId = new JobApplicationId(Guid.NewGuid());

        await using (var dbContext = CreateDbContext())
        {
            var jobApplication = await CreateJobApplicationAsync(dbContext, jobApplicationId);
        }

        var runningJob1 = BackgroundJob.Create(BackgroundJobType.Llm, BackgroundJobPriority.Normal, jobApplicationId, "testing");

        var runningJob2 = BackgroundJob.Create(BackgroundJobType.Llm, BackgroundJobPriority.High, jobApplicationId, "testing");

        await using (var dbContext = CreateDbContext())
        {
            var repository = new BackgroundJobRepository(dbContext);

            await repository.AddAsync(runningJob1);
            await repository.AddAsync(runningJob2);

            runningJob1.Start();
            runningJob2.Start();

            await repository.UpdateAsync(runningJob1);
            await repository.UpdateAsync(runningJob2);
        }

        await using (var dbContext = CreateDbContext())
        {
            var repository = new BackgroundJobRepository(dbContext);

            await repository.RecoverRunningAsync();
        }

        await using var verificationContext = CreateDbContext();
        var verificationRepository = new BackgroundJobRepository(verificationContext);

        var recoveredJob1 = await verificationRepository.GetAsync(runningJob1.Id);

        var recoveredJob2 = await verificationRepository.GetAsync(runningJob2.Id);

        Assert.NotNull(recoveredJob1);
        Assert.NotNull(recoveredJob2);

        Assert.Equal(BackgroundJobStatus.Pending, recoveredJob1.Status);
        Assert.Equal(BackgroundJobStatus.Pending, recoveredJob2.Status);

        Assert.Null(recoveredJob1.StartedAt);
        Assert.Null(recoveredJob1.CompletedAt);
        Assert.Null(recoveredJob1.Error);

        Assert.Null(recoveredJob2.StartedAt);
        Assert.Null(recoveredJob2.CompletedAt);
        Assert.Null(recoveredJob2.Error);
    }

    [Fact]
    public async Task Does_not_recover_jobs_that_are_not_running()
    {
        var jobApplicationId = new JobApplicationId(Guid.NewGuid());

        await using (var dbContext = CreateDbContext())
        {
            var jobApplication = await CreateJobApplicationAsync(dbContext, jobApplicationId);
        }

        var pendingJob = BackgroundJob.Create(BackgroundJobType.Llm, BackgroundJobPriority.Normal, jobApplicationId, "testing");

        var completedJob = BackgroundJob.Create(BackgroundJobType.Llm, BackgroundJobPriority.Normal, jobApplicationId, "testing");

        var failedJob = BackgroundJob.Create(BackgroundJobType.Llm, BackgroundJobPriority.Normal, jobApplicationId, "testing");

        await using (var dbContext = CreateDbContext())
        {
            var repository = new BackgroundJobRepository(dbContext);

            await repository.AddAsync(pendingJob);
            await repository.AddAsync(completedJob);
            await repository.AddAsync(failedJob);

            completedJob.Start();
            completedJob.Complete();
            await repository.UpdateAsync(completedJob);

            failedJob.Start();
            failedJob.Fail("Test failure.");
            await repository.UpdateAsync(failedJob);
        }

        await using (var dbContext = CreateDbContext())
        {
            var repository = new BackgroundJobRepository(dbContext);

            await repository.RecoverRunningAsync();
        }

        await using var verificationContext = CreateDbContext();
        var verificationRepository = new BackgroundJobRepository(verificationContext);

        var retrievedPendingJob = await verificationRepository.GetAsync(pendingJob.Id);

        var retrievedCompletedJob = await verificationRepository.GetAsync(completedJob.Id);

        var retrievedFailedJob = await verificationRepository.GetAsync(failedJob.Id);

        Assert.NotNull(retrievedPendingJob);
        Assert.NotNull(retrievedCompletedJob);
        Assert.NotNull(retrievedFailedJob);

        Assert.Equal(BackgroundJobStatus.Pending, retrievedPendingJob.Status);

        Assert.Equal(BackgroundJobStatus.Completed, retrievedCompletedJob.Status);
        Assert.NotNull(retrievedCompletedJob.CompletedAt);

        Assert.Equal(BackgroundJobStatus.Failed, retrievedFailedJob.Status);
        Assert.Equal("Test failure.", retrievedFailedJob.Error);
    }

    private static void AssertEqualToPostgresPrecision(DateTime expected, DateTime actual)
    {
        Assert.Equal(
            expected.Ticks / 10,
            actual.Ticks / 10);
    }

    private async Task<JobApplication> CreateJobApplicationAsync(JobApplicationHelperDbContext dbContext,
        JobApplicationId? id = null)
    {
        var jobApplication = new JobApplication
        {
            Id = id ?? new JobApplicationId(Guid.NewGuid()),
            CountryCode = "US",
            IncludeCoverLetter = true,
            CompanyName = "Test Company",
            PositionTitle = "Software Engineer",
            URL = "https://example.com/job",
            City = "Chicago",
            JobPosting = "Test job posting.",
            CreatedAt = DateTime.UtcNow,
            ApplicationFolder = "test-folder"
        };

        dbContext.JobApplications.Add(jobApplication.ToEntity());
        await dbContext.SaveChangesAsync();

        return jobApplication;
    }
}