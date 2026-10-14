namespace JobApplicationHelper.ViewModels;

public interface IViewModelFactory
{
    TViewModel Create<TViewModel>() where TViewModel : ViewModelBase;
}