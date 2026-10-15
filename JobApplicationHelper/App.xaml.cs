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

        builder.Services.AddApiOptions();
        builder.Services.AddApiHttpClients();
        builder.Services.AddApplicationServices();
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
        try
        {
            if (AppHost is not null)
            {
                await AppHost.StopAsync();
                AppHost.Dispose();
            }
        }
        finally
        {
            base.OnExit(e);
        }
    }
}