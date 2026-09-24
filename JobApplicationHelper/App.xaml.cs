using JobApplicationHelper.Application;
using JobApplicationHelper.Domain.Models;
using JobApplicationHelper.Infrastructure;
using JobApplicationHelper.Services.Api;
using JobApplicationHelper.ViewModels;
using JobApplicationHelper.Views;
using JobApplicationHelper.WindowService;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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

        builder.Configuration.Sources.Clear();
        builder.Configuration
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false);

        builder.Services.AddSingleton<IConfiguration>(builder.Configuration);

        builder.Services.AddHttpClient<JobRequirementsApiClient>(client =>
        {
            client.BaseAddress = new Uri(
                builder.Configuration["Api:BaseUrl"]
                ?? throw new InvalidOperationException("API base URL is not configured."));

            client.Timeout = TimeSpan.FromMinutes(10);
        });



        builder.Services.AddSingleton<MainWindow>();
        builder.Services.AddTransient<MainWindowViewModel>();
        builder.Services.AddTransient<JobRequirementsViewModel>();
        builder.Services.AddTransient<CoverLetterViewModel>();
        builder.Services.AddTransient<DraftWindowViewModel>(sp =>
        {
            var navigation = new DraftNavigation();
            var draftParameters = new CoverLetterDraftParameters();

            var jobRequirements =
                ActivatorUtilities.CreateInstance<JobRequirementsViewModel>(
                    sp,
                    [navigation, draftParameters]);

            var coverLetter =
                ActivatorUtilities.CreateInstance<CoverLetterViewModel>(
                    sp,
                    [navigation, draftParameters]);

            var viewModel = new DraftWindowViewModel(
                jobRequirements,
                coverLetter);
            navigation.Initialize(viewModel);

            return viewModel;
        });

        builder.Services.AddKeyedTransient<Window, DraftWindow>(typeof(DraftWindowViewModel));
        builder.Services.AddKeyedTransient<Window, VerificationResultDialog>(typeof(VerificationResultDialogViewModel));
        builder.Services.AddTransient<IFolderLauncher, FolderLauncher>();
        builder.Services.AddTransient<DraftWindow>();
        builder.Services.AddSingleton<IWindowService, JobApplicationHelper.WindowService.WindowService>();



        // Temporary refactoring, these will be moved to the API DI
        builder.Services.AddApplication(builder.Configuration);
        builder.Services.AddInfrastructure(builder.Configuration);

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