using System.Windows;
using JobApplicationHelper.ViewModels;

namespace JobApplicationHelper.WindowService;

public interface IOpenWindowRegistry
{
    bool TryGetSingle(Type viewModelType, out ViewModelBase? viewModel, out Window? window);

    bool TryGetWindow(ViewModelBase viewModel, out Window? window);

    void RegisterSingle(Type viewModelType, ViewModelBase viewModel, Window window);

    void RegisterInstance(ViewModelBase viewModel, Window window);

    void RemoveSingle(Type viewModelType, ViewModelBase viewModel, Window window);

    void RemoveInstance(ViewModelBase viewModel, Window window);
}