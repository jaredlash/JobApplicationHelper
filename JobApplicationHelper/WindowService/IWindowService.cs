using JobApplicationHelper.ViewModels;

namespace JobApplicationHelper.WindowService;

public interface IWindowService
{
    bool? ShowDialog<TViewModel>(TViewModel viewModel) where TViewModel : ViewModelBase;
    void ShowWindow<TViewModel>(TViewModel viewModel) where TViewModel : ViewModelBase;
}
