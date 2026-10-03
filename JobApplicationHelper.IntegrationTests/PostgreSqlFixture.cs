using JobApplicationHelper.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Respawn;
using Testcontainers.PostgreSql;
using Xunit.v3;

namespace JobApplicationHelper.IntegrationTests;

public sealed class PostgreSqlFixture :
    IAsyncLifetime,
    INotifyTestLifecycleAsync
{
    private readonly PostgreSqlContainer container =
        new PostgreSqlBuilder("postgres:18").Build();

    private Respawner respawner = null!;

    public string ConnectionString => container.GetConnectionString();

    public async ValueTask InitializeAsync()
    {
        await container.StartAsync();

        var optionsBuilder = new DbContextOptionsBuilder<JobApplicationHelperDbContext>();

        optionsBuilder.UseNpgsql(ConnectionString);

        await using var dbContext = new JobApplicationHelperDbContext(optionsBuilder.Options);

        await dbContext.Database.MigrateAsync();

        await using var connection = new NpgsqlConnection(ConnectionString);

        await connection.OpenAsync();

        respawner = await Respawner.CreateAsync(connection, new RespawnerOptions { DbAdapter = DbAdapter.Postgres });
    }

    public async ValueTask DisposeAsync()
    {
        await container.StopAsync();
        await container.DisposeAsync();
    }

    public async ValueTask OnTestStartingAsync(IXunitTest test)
    {
        await using var connection = new NpgsqlConnection(ConnectionString);

        await connection.OpenAsync();

        await respawner.ResetAsync(connection);
    }

    public ValueTask OnTestFinishedAsync(IXunitTest test)
    {
        return ValueTask.CompletedTask;
    }
}