using JobApplicationHelper.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace JobApplicationHelper.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddViewWithViewModel<TViewModel, TView>(this IServiceCollection services)
        where TViewModel : ViewModelBase
        where TView : Window
    {
        services.AddTransient<TViewModel>();

        services.AddKeyedTransient<Window, TView>(typeof(TViewModel));

        return services;
    }
}