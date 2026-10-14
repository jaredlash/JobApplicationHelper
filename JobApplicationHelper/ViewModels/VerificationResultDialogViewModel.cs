using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JobApplicationHelper.Domain.Models;

namespace JobApplicationHelper.ViewModels;

public partial class VerificationResultDialogViewModel : ViewModelBase
{
    public void Initialize(VerificationResult verificationResult)
    {
        VerificationResult = verificationResult;
    }

    [ObservableProperty]
    private VerificationResult verificationResult = new();

    [RelayCommand]
    private void Okay()
    {
        TryClose(true);
    }
}
