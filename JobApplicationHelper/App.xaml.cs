using JobApplicationHelper.Configuration;
using JobApplicationHelper.Data;
using JobApplicationHelper.Models;
using JobApplicationHelper.Services;
using JobApplicationHelper.ViewModels;
using JobApplicationHelper.Views;
using JobApplicationHelper.WindowService;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using OllamaSharp;
using System.IO;
using System.Net.Http;
using System.Windows;

namespace JobApplicationHelper;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public static IHost? AppHost { get; private set; }

    public App()
    {
        var builder = Host.CreateApplicationBuilder();

        builder.Configuration.Sources.Clear();
        builder.Configuration
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false);

        builder.Services.AddSingleton<IConfiguration>(builder.Configuration);

        builder.Services.AddOptions<LocationsOptions>()
            .Configure(options =>
            {
                var locs = builder.Configuration.GetSection("Locations").Get<List<Location>>();
                options.Locations = locs ?? [];
            })
            .ValidateDataAnnotations()
            .ValidateOnStart();

        builder.Services.AddOptions<FileServiceOptions>()
            .Bind(builder.Configuration.GetSection("FileService"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        builder.Services.AddOptions<CandidateOptions>()
            .Bind(builder.Configuration.GetSection("Candidate"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        builder.Services.AddOptions<ExperienceBankOptions>()
            .Bind(builder.Configuration.GetSection("ExperienceBank"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        builder.Services.AddDbContext<ExperienceBankDbContext>((serviceProvider, options) =>
        {
            var configuration =
                serviceProvider.GetRequiredService<IConfiguration>();

            var experienceBankOptions = serviceProvider.GetRequiredService<IOptions<ExperienceBankOptions>>().Value;

            options.UseSqlite($"Data Source={experienceBankOptions.DatabaseFileName}");
        });

        //builder.Services.AddScoped<IExperienceBankService, EfExperienceBankService>();
        builder.Services.AddScoped<IExperienceBankService, TempYamlExperienceBankService>();

        // TODO: Evaluate if this should actually be transient
        builder.Services.AddSingleton<IChatClient>(sp =>
        {
            var httpClient = new HttpClient
            {
                BaseAddress = new Uri("http://localhost:11434"),
                Timeout = TimeSpan.FromMinutes(10)
            };

            return new OllamaApiClient(httpClient, "qwen3:14b");
        });

        builder.Services.AddSingleton<MainWindow>();
        builder.Services.AddSingleton<IExperienceBankImportService, YamlExperienceBankImportService>();
        builder.Services.AddTransient<MainWindowViewModel>();
        builder.Services.AddTransient<DraftWindowViewModel>();
        builder.Services.AddKeyedTransient<Window, DraftWindow>(typeof(DraftWindowViewModel));
        builder.Services.AddKeyedTransient<Window, VerificationResultDialog>(typeof(VerificationResultDialogViewModel));
        builder.Services.AddTransient<LocationService>();
        builder.Services.AddTransient<FileService>();
        builder.Services.AddTransient<DraftWindow>();
        builder.Services.AddSingleton<IWindowService, JobApplicationHelper.WindowService.WindowService>();
        builder.Services.AddTransient<CoverLetterService>();
        builder.Services.AddTransient<JobRequirementService>();

        AppHost = builder.Build();

    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        await AppHost!.StartAsync();

        var mainWindow = AppHost.Services.GetRequiredService<MainWindow>();
        mainWindow.Show();

        base.OnStartup(e);
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        await AppHost!.StopAsync();
        base.OnExit(e);
    }

}