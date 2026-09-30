using JobApplicationHelper.Domain.Models;
using JobApplicationHelper.Infrastructure.Persistence;
using JobApplicationHelper.Infrastructure.Persistence.Mapping;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace JobApplicationHelper.ApiIntegrationTests.Infrastructure;

public sealed class ApiIntegrationTestFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer postgres = new PostgreSqlBuilder("postgres:18").Build();

    public ApiWebApplicationFactory Factory { get; private set; } = null!;

    public HttpClient CreateClient()
    {
        return Factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        await postgres.StartAsync();

        Factory = new ApiWebApplicationFactory(postgres.GetConnectionString());

        await using var scope = Factory.Services.CreateAsyncScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<JobApplicationHelperDbContext>();

        await dbContext.Database.MigrateAsync();
    }

    public async Task AddJobApplicationAsync(JobApplication jobApplication)
    {
        await using var scope = Factory.Services.CreateAsyncScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<JobApplicationHelperDbContext>();

        await dbContext.JobApplications.AddAsync(jobApplication.ToEntity());
        await dbContext.SaveChangesAsync();
    }

    public async Task AddExtractedJobRequirementsAsync(ExtractedJobRequirements requirements)
    {
        await using var scope = Factory.Services.CreateAsyncScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<JobApplicationHelperDbContext>();

        await dbContext.ExtractedJobRequirements.AddAsync(requirements.ToEntity());

        await dbContext.SaveChangesAsync();
    }

    public async Task DisposeAsync()
    {
        await Factory.DisposeAsync();
        await postgres.DisposeAsync();
    }
}