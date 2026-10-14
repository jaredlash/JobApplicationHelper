using JobApplicationHelper.WindowService;
using Microsoft.Extensions.DependencyInjection;

namespace JobApplicationHelper.ViewModels;



public sealed class ViewModelFactory(IServiceProvider serviceProvider, IOpenWindowRegistry openWindowRegistry)
    : IViewModelFactory
{
    public TViewModel Create<TViewModel>()
        where TViewModel : ViewModelBase
    {
        Type viewModelType = typeof(TViewModel);

        if (typeof(ISingleWindowViewModel).IsAssignableFrom(viewModelType)
            && openWindowRegistry.TryGetSingle(
                viewModelType,
                out var existingViewModel,
                out _))
        {
            return (TViewModel)existingViewModel!;
        }

        return serviceProvider.GetService<TViewModel>()
            ?? throw new InvalidOperationException($"ViewModel of type {nameof(TViewModel)} is not registered in service provider");
    }
}