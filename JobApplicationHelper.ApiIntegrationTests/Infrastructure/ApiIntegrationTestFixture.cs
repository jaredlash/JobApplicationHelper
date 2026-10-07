using JobApplicationHelper.Domain.Models;
using JobApplicationHelper.Infrastructure.Persistence;
using JobApplicationHelper.Infrastructure.Persistence.Mapping;
using JobApplicationHelper.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Respawn;
using Testcontainers.PostgreSql;
using Xunit.v3;

namespace JobApplicationHelper.ApiIntegrationTests.Infrastructure;

public sealed class ApiIntegrationTestFixture :
    IAsyncLifetime,
    INotifyTestLifecycleAsync
{
    private readonly PostgreSqlContainer postgres = new PostgreSqlBuilder("postgres:18").Build();

    private Respawner respawner = null!;

    public ApiWebApplicationFactory Factory { get; private set; } = null!;

    public HttpClient CreateClient()
    {
        return Factory.CreateClient();
    }

    public async ValueTask InitializeAsync()
    {
        await postgres.StartAsync();

        Factory = new ApiWebApplicationFactory(postgres.GetConnectionString());

        await using var scope = Factory.Services.CreateAsyncScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<JobApplicationHelperDbContext>();

        await dbContext.Database.MigrateAsync();

        await using var connection = new NpgsqlConnection(postgres.GetConnectionString());

        await connection.OpenAsync();

        respawner = await Respawner.CreateAsync(connection, new RespawnerOptions { DbAdapter = DbAdapter.Postgres });
    }

    public async ValueTask DisposeAsync()
    {
        await Factory.DisposeAsync();
        await postgres.DisposeAsync();
    }

    public async ValueTask OnTestStartingAsync(IXunitTest test)
    {
        await using var connection = new NpgsqlConnection(postgres.GetConnectionString());

        await connection.OpenAsync();

        await respawner.ResetAsync(connection);
    }

    public ValueTask OnTestFinishedAsync(IXunitTest test)
    {
        return ValueTask.CompletedTask;
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

        var dbContext =
            scope.ServiceProvider.GetRequiredService<JobApplicationHelperDbContext>();

        await dbContext.ExtractedJobRequirements.AddAsync(
            requirements.ToEntity());

        await dbContext.SaveChangesAsync();
    }

    public async Task AddExperienceAsync(Experience experience, string? legacyId = null)
    {
        await using var scope = Factory.Services.CreateAsyncScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<JobApplicationHelperDbContext>();

        var repository = new ExperienceRepository(dbContext);

        await repository.AddOrReplaceAsync(experience, legacyId);
    }
}