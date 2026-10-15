using JobApplicationHelper.DependencyInjection;
using JobApplicationHelper.Domain.Models;
using JobApplicationHelper.Services.Api;
using JobApplicationHelper.Services.BackgroundJobs;
using JobApplicationHelper.Services.Experiences;
using JobApplicationHelper.ViewModels;
using JobApplicationHelper.Views;
using JobApplicationHelper.WindowService;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.Windows;
using WpfApplication = System.Windows.Application;

namespace JobApplicationHelper;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : WpfApplication
{
    public static IHost? AppHost { get; private set; }

    public App()
    {
        var builder = Host.CreateApplicationBuilder();

        builder.Configuration
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false);

        builder.Services.AddApiOptions();

        builder.Services.AddApiHttpClient<JobRequirementsApiClient>();
        builder.Services.AddApiHttpClient<CoverLettersApiClient>();
        builder.Services.AddApiHttpClient<ExperiencesApiClient>();
        builder.Services.AddApiHttpClient<LocationsApiClient>();
        builder.Services.AddApiHttpClient<JobApplicationsApiClient>();
        builder.Services.AddApiHttpClient<BackgroundJobsApiClient>();
        builder.Services.AddApiHttpClient<IApiHealthService, ApiHealthService>();

        builder.Services.AddSingleton<IBackgroundJobNotificationService>(serviceProvider =>
        {
            var backgroundJobsApiClient = serviceProvider.GetRequiredService<BackgroundJobsApiClient>();
            var options = serviceProvider.GetRequiredService<IOptions<ApiOptions>>().Value;
            var hubUri = new Uri(new Uri(options.BaseUrl), "hubs/background-jobs");

            return new SignalRBackgroundJobNotificationService(backgroundJobsApiClient, hubUri.ToString());
        });

        builder.Services.AddTransient<IBackgroundJobPollingService, BackgroundJobPollingService>();
        builder.Services.AddTransient<ExperiencesImportService>();


        builder.Services.AddWindowServices();


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