using JobApplicationHelper.DependencyInjection;
using JobApplicationHelper.Views;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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

        builder.Services.AddApiOptions();
        builder.Services.AddApiHttpClients();
        builder.Services.AddApplicationServices();
        builder.Services.AddWindowServices();


        AppHost = builder.Build();

    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        await AppHost!.StartAsync();
        base.OnStartup(e);

        var mainWindow = AppHost.Services.GetRequiredService<MainWindow>();
        mainWindow.Show();
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