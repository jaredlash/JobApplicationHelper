using JobApplicationHelper.Application.Services;
using JobApplicationHelper.Infrastructure.Configuration;
using JobApplicationHelper.Infrastructure.Data;
using JobApplicationHelper.Infrastructure.Services;
using JobApplicationHelper.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Chat;

namespace JobApplicationHelper.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<ExperienceBankOptions>()
            .Bind(configuration.GetSection("ExperienceBank"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<FileServiceOptions>()
            .Bind(configuration.GetSection("FileService"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        // TODO: Remove this when the CandidateOptions are no longer needed in the infrastructure layer.
        services.AddOptions<CandidateOptions>()
            .Bind(configuration.GetSection("Candidate"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddTransient<FileService>();

        services.AddSingleton<IExperienceBankImportService, YamlExperienceBankImportService>();
        services.AddSingleton<IChatClient>(sp =>
        {
            var configuration = sp.GetRequiredService<IConfiguration>();

            var endpoint = configuration["LLM:Endpoint"]
                ?? throw new InvalidOperationException("LLM endpoint is not configured.");

            var model = configuration["LLM:Model"]
                ?? throw new InvalidOperationException("LLM model is not configured.");

            var chatClient = new ChatClient(
                model: model,
                credential: new System.ClientModel.ApiKeyCredential("not-needed"),
                options: new OpenAIClientOptions
                {
                    Endpoint = new Uri(endpoint),
                    NetworkTimeout = TimeSpan.FromMinutes(10)
                });

            return chatClient.AsIChatClient();
        });
        services.AddDbContext<ExperienceBankDbContext>((serviceProvider, options) =>
        {
            var configuration =
                serviceProvider.GetRequiredService<IConfiguration>();

            var experienceBankOptions = serviceProvider.GetRequiredService<IOptions<ExperienceBankOptions>>().Value;

            options.UseSqlite($"Data Source={experienceBankOptions.DatabaseFileName}");
        });

        //ervices.AddScoped<IExperienceBankService, EfExperienceBankService>();
        services.AddScoped<IExperienceBankService, TempYamlExperienceBankService>();

        return services;
    }
}
