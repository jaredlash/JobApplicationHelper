using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JobApplicationHelper.Models;

namespace JobApplicationHelper.ViewModels;

public partial class VerificationResultDialogViewModel : ViewModelBase
{
    public VerificationResultDialogViewModel(VerificationResult verificationResult)
    {
        this.verificationResult = verificationResult;
    }

    [ObservableProperty]
    private VerificationResult verificationResult;

    [RelayCommand]
    private void Okay()
    {
        TryClose(true);
    }
}
