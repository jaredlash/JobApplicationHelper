using JobApplicationHelper.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace JobApplicationHelper.WindowService;

public class WindowService : IWindowService
{
    private readonly Dictionary<Type, Window> currentWindows;
    private readonly IServiceProvider serviceProvider;

    public WindowService(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
        currentWindows = [];
    }

    public bool? ShowDialog<TViewModel>(TViewModel viewModel) where TViewModel : ViewModelBase
    {
        Window view;
        Type vmType = typeof(TViewModel);

        view = serviceProvider.GetKeyedService<Window>(vmType) ?? throw new InvalidOperationException($"No window registered for view model type {vmType.FullName}");

        view.DataContext = viewModel;

        currentWindows.Add(vmType, view);

        viewModel.CloseAction = (r) => {
            view.DialogResult = r;
        };


        view.Closed += (o, args) =>
        {
            viewModel.OnClose();
            currentWindows.Remove(vmType);
            viewModel.CloseAction = null;
        };


        return view.ShowDialog();
    }

    public void ShowWindow<TViewModel>(TViewModel viewModel) where TViewModel : ViewModelBase
    {
        Type vmType = typeof(TViewModel);

        if (currentWindows.TryGetValue(vmType, out Window? view) == false)
        {
            view = serviceProvider.GetKeyedService<Window>(vmType) ?? throw new InvalidOperationException($"No window registered for view model type {vmType.FullName}");

            view.DataContext = viewModel;

            currentWindows.Add(vmType, view);

            viewModel.CloseAction = (r) => { view.Close(); };

            view.Closed += (o, args) =>
            {
                viewModel.OnClose();
                currentWindows.Remove(vmType);
                viewModel.CloseAction = null;
            };
        }

        view.Show();
        view.Activate();
    }
}
