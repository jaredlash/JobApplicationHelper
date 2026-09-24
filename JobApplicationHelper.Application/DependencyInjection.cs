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

        services.AddTransient<LocationService>();
        services.AddTransient<CoverLetterService>();
        services.AddTransient<JobRequirementService>();

        return services;
    }
}
