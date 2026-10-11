using CommunityToolkit.Mvvm.Input;

namespace JobApplicationHelper.ViewModels;


public partial class ConfirmDialogViewModel : ViewModelBase
{
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string YesText { get; set; } = "Yes";
    public string NoText { get; set; } = "No";

    [RelayCommand]
    public void Yes()
    {
        TryClose(true);
    }

    [RelayCommand]
    public void No()
    {
        TryClose(false);
    }
}