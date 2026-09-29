using JobApplicationHelper.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace JobApplicationHelper.IntegrationTests;

public sealed class PostgreSqlFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer container = new PostgreSqlBuilder("postgres:18").Build();

    public string ConnectionString => container.GetConnectionString();

    public async Task InitializeAsync()
    {
        await container.StartAsync();

        var optionsBuilder = new DbContextOptionsBuilder<JobApplicationHelperDbContext>();
        optionsBuilder.UseNpgsql(ConnectionString);

        using var dbContext = new JobApplicationHelperDbContext(optionsBuilder.Options);
        await dbContext.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await container.StopAsync();
        await container.DisposeAsync();
    }
}