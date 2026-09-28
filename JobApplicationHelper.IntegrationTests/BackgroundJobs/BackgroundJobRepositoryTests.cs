using JobApplicationHelper.Domain.Models;
using JobApplicationHelper.Infrastructure.Persistence;
using JobApplicationHelper.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace JobApplicationHelper.IntegrationTests.BackgroundJobs;

public sealed class BackgroundJobRepositoryTests
{
    [Fact]
    public async Task Adds_and_retrieves_background_job()
    {
        var options = new DbContextOptionsBuilder<JobApplicationHelperDbContext>()
            .UseNpgsql(TestDatabase.ConnectionString)
            .Options;

        await using var dbContext =
            new JobApplicationHelperDbContext(options);

        var repository = new BackgroundJobRepository(dbContext);

        var job = BackgroundJob.Create(BackgroundJobType.Llm, BackgroundJobPriority.High);

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
    }

    private static void AssertEqualToPostgresPrecision(DateTime expected, DateTime actual)
    {
        Assert.Equal(
            expected.Ticks / 10,
            actual.Ticks / 10);
    }
}