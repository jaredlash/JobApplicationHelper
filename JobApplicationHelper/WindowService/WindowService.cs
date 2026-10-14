using JobApplicationHelper.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace JobApplicationHelper.WindowService;

public class WindowService : IWindowService
{
    private readonly IServiceProvider serviceProvider;
    private readonly IOpenWindowRegistry openWindowRegistry;

    public WindowService(IServiceProvider serviceProvider, IOpenWindowRegistry openWindowRegistry)
    {
        this.serviceProvider = serviceProvider;
        this.openWindowRegistry = openWindowRegistry;
    }

    public void ShowAlertDialog(string title, string message)
    {
        var alertDialogViewModel = serviceProvider.GetRequiredService<AlertDialogViewModel>();
        alertDialogViewModel.Title = title;
        alertDialogViewModel.Message = message;

        ShowDialog(alertDialogViewModel);
    }

    public bool? ShowConfirmDialog(string title, string message, string yesText = "Yes", string noText = "No")
    {

        var confirmDialogViewModel = serviceProvider.GetRequiredService<ConfirmDialogViewModel>();
        confirmDialogViewModel.Title = title;
        confirmDialogViewModel.Message = message;
        confirmDialogViewModel.YesText = yesText;
        confirmDialogViewModel.NoText = noText;

        return ShowDialog(confirmDialogViewModel);
    }

    public bool? ShowDialog<TViewModel>(TViewModel viewModel)
        where TViewModel : ViewModelBase
    {
        ArgumentNullException.ThrowIfNull(viewModel);

        Type viewModelType = viewModel.GetType();
        Window view = CreateWindow(viewModelType, viewModel);

        viewModel.CloseAction = result =>
        {
            view.DialogResult = result;
        };

        view.Closed += (_, _) =>
        {
            try
            {
                viewModel.OnClose();
            }
            finally
            {
                viewModel.CloseAction = null;
            }
        };

        return view.ShowDialog();
    }

    public void ShowWindow<TViewModel>(TViewModel viewModel) where TViewModel : ViewModelBase
    {
        ArgumentNullException.ThrowIfNull(viewModel);

        Type viewModelType = viewModel.GetType();

        if (viewModel is ISingleWindowViewModel)
        {
            if (openWindowRegistry.TryGetSingle(viewModelType, out _, out Window? existingWindow))
            {
                Activate(existingWindow!);
                return;
            }

            Window view = CreateWindow(viewModelType, viewModel);

            openWindowRegistry.RegisterSingle(viewModelType, viewModel, view);

            viewModel.CloseAction = _ => view.Close();

            view.Closed += (_, _) =>
            {
                try
                {
                    openWindowRegistry.RemoveSingle(viewModelType, viewModel, view);
                }
                finally
                {
                    try
                    {
                        viewModel.OnClose();
                    }
                    finally
                    {
                        viewModel.CloseAction = null;
                    }
                }
            };

            Activate(view);
            return;
        }

        if (openWindowRegistry.TryGetWindow(viewModel, out Window? existingInstanceWindow))
        {
            Activate(existingInstanceWindow!);
            return;
        }

        Window instanceView = CreateWindow(viewModelType, viewModel);

        openWindowRegistry.RegisterInstance(viewModel, instanceView);

        viewModel.CloseAction = _ => instanceView.Close();

        instanceView.Closed += (_, _) =>
        {
            try
            {
                openWindowRegistry.RemoveInstance(viewModel, instanceView);
            }
            finally
            {
                try
                {
                    viewModel.OnClose();
                }
                finally
                {
                    viewModel.CloseAction = null;
                }
            }
        };

        Activate(instanceView);
    }

    private Window CreateWindow(Type viewModelType, ViewModelBase viewModel)
    {
        Window view = serviceProvider.GetKeyedService<Window>(viewModelType)
            ?? throw new InvalidOperationException($"No window registered for view model type {viewModelType.FullName}");

        view.DataContext = viewModel;

        return view;
    }

    private static void Activate(Window view)
    {
        if (!view.IsVisible)
        {
            view.Show();
        }

        if (view.WindowState == WindowState.Minimized)
        {
            view.WindowState = WindowState.Normal;
        }

        view.Activate();
    }
}
