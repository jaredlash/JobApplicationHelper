using JobApplicationHelper.ViewModels;

namespace JobApplicationHelper.WindowService;

public interface IWindowService
{
    bool? ShowDialog<TViewModel>(TViewModel viewModel) where TViewModel : ViewModelBase;
    void ShowWindow<TViewModel>(TViewModel viewModel) where TViewModel : ViewModelBase;

    void ShowAlertDialog(string title, string message);

    bool? ShowConfirmDialog(string title, string message, string yesText = "Yes", string noText = "No");
}
