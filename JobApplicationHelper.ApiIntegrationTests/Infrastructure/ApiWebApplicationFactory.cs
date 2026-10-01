using JobApplicationHelper.Infrastructure.Persistence;
using JobApplicationHelper.Infrastructure.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace JobApplicationHelper.ApiIntegrationTests.Infrastructure;

public sealed class ApiWebApplicationFactory
    : WebApplicationFactory<Program>
{
    private readonly string connectionString;

    public ApiWebApplicationFactory(string connectionString)
    {
        this.connectionString = connectionString;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            ReplaceDbContext(services);
            RemoveBackgroundJobWorker(services);
        });
    }

    private void ReplaceDbContext(IServiceCollection services)
    {
        var descriptor = services.Single(service => service.ServiceType == typeof(DbContextOptions<JobApplicationHelperDbContext>));

        services.Remove(descriptor);

        services.AddDbContext<JobApplicationHelperDbContext>(options =>
        {
            options.UseNpgsql(connectionString);
        });
    }

    private static void RemoveBackgroundJobWorker(IServiceCollection services)
    {
        var descriptor = services.Single(service => service.ServiceType == typeof(IHostedService) && service.ImplementationType == typeof(BackgroundJobWorker));

        services.Remove(descriptor);
    }
}