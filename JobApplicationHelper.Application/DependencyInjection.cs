using JobApplicationHelper.Application.Configuration;
using JobApplicationHelper.Application.Services;
using JobApplicationHelper.Domain.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace JobApplicationHelper.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {

        services.AddOptions<LocationsOptions>()
            .Configure(options =>
            {
                var locs = configuration.GetSection("Locations").Get<List<Location>>();
                options.Locations = locs ?? [];
            })
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddScoped<LocationService>();
        services.AddScoped<CoverLetterService>();
        services.AddScoped<IJobRequirementService, JobRequirementService>();
        services.AddScoped<IBackgroundJobService, BackgroundJobService>();

        //services.AddScoped<IBackgroundJobExecutor, NoOpBackgroundJobExecutor>();
        services.AddScoped<IBackgroundJobExecutor, BackgroundJobExecutor>();

        return services;
    }
}
