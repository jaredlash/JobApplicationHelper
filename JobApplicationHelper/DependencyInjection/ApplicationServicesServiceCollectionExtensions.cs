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
        services.AddSingleton<IBackgroundJobNotificationService>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<ApiOptions>>().Value;
            var hubUri = new Uri(new Uri(options.BaseUrl), "hubs/background-jobs");

            return ActivatorUtilities.CreateInstance<SignalRBackgroundJobNotificationService>(sp, hubUri);
        });

        services.AddTransient<IBackgroundJobPollingService, BackgroundJobPollingService>();
        services.AddTransient<ExperiencesImportService>();

        return services;
    }
}
