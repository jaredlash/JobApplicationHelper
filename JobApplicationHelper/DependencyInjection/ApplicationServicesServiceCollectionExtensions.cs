using JobApplicationHelper.Services.Api;
using JobApplicationHelper.Services.BackgroundJobs;
using JobApplicationHelper.Services.Experiences;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace JobApplicationHelper.DependencyInjection;

public static class ApplicationServicesServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddSingleton<IBackgroundJobNotificationService>(serviceProvider =>
        {
            var backgroundJobsApiClient = serviceProvider.GetRequiredService<BackgroundJobsApiClient>();
            var options = serviceProvider.GetRequiredService<IOptions<ApiOptions>>().Value;
            var hubUri = new Uri(new Uri(options.BaseUrl), "hubs/background-jobs");

            return new SignalRBackgroundJobNotificationService(backgroundJobsApiClient, hubUri.ToString());
        });

        services.AddTransient<IBackgroundJobPollingService, BackgroundJobPollingService>();
        services.AddTransient<ExperiencesImportService>();

        return services;
    }
}
