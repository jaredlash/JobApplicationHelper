
using JobApplicationHelper.Services.Api;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace JobApplicationHelper.DependencyInjection;

public static class ApiServiceCollectionExtensions
{
    public static IServiceCollection AddApiOptions(this IServiceCollection services)
    {
        services.AddOptions<ApiOptions>()
            .BindConfiguration("Api")
            .ValidateDataAnnotations()
            .Validate(
                options =>
                {
                    if (!Uri.TryCreate(
                            options.BaseUrl,
                            UriKind.Absolute,
                            out var uri))
                    {
                        return false;
                    }

                    return uri.Scheme == Uri.UriSchemeHttp
                        || uri.Scheme == Uri.UriSchemeHttps;
                },
                "Api:BaseUrl must be a valid HTTP or HTTPS URL.")
            .ValidateOnStart();

        return services;
    }

    public static IServiceCollection AddApiHttpClients(this IServiceCollection services)
    {

        services.AddApiHttpClient<JobRequirementsApiClient>();
        services.AddApiHttpClient<CoverLettersApiClient>();
        services.AddApiHttpClient<ExperiencesApiClient>();
        services.AddApiHttpClient<LocationsApiClient>();
        services.AddApiHttpClient<JobApplicationsApiClient>();
        services.AddApiHttpClient<BackgroundJobsApiClient>();
        services.AddApiHttpClient<IApiHealthService, ApiHealthService>();

        return services;
    }


    public static IServiceCollection AddApiHttpClient<TClient>(this IServiceCollection services)
        where TClient : class
    {
        services.AddHttpClient<TClient>((serviceProvider, client) =>
        {
            var options = serviceProvider
                .GetRequiredService<IOptions<ApiOptions>>()
                .Value;

            client.BaseAddress = new Uri(options.BaseUrl);
        });

        return services;
    }

    public static IServiceCollection AddApiHttpClient<TClient, TImplementation>(this IServiceCollection services)
        where TClient : class
        where TImplementation : class, TClient
    {
        services.AddHttpClient<TClient, TImplementation>(
            (serviceProvider, client) =>
            {
                var options = serviceProvider
                    .GetRequiredService<IOptions<ApiOptions>>()
                    .Value;

                client.BaseAddress = new Uri(options.BaseUrl);
            });

        return services;
    }
}
