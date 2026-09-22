using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JobApplicationHelper.Models;
using JobApplicationHelper.Services;
using JobApplicationHelper.WindowService;
using Microsoft.Extensions.Logging;

namespace JobApplicationHelper.ViewModels;

public partial class CoverLetterViewModel : ViewModelBase
{
    private readonly FileService fileService;
    private readonly CoverLetterService coverLetterService;
    private readonly IDraftNavigation navigation;
    private readonly IWindowService windowService;
    private readonly CoverLetterDraftParameters draftParameters;
    private readonly ILogger<CoverLetterViewModel> logger;


    public CoverLetterViewModel(
        FileService fileService,
        CoverLetterService coverLetterService,
        IDraftNavigation navigation,
        IWindowService windowService,
        CoverLetterDraftParameters draftParameters,
        ILogger<CoverLetterViewModel> logger)
    {
        this.fileService = fileService;
        this.coverLetterService = coverLetterService;
        this.navigation = navigation;
        this.windowService = windowService;
        this.draftParameters = draftParameters;
        this.logger = logger;
    }


    [ObservableProperty]
    private string cvText = String.Empty;

    [ObservableProperty]
    private string outputFolder = String.Empty;

    [ObservableProperty]
    private string additionalPromptInstructions = String.Empty;

    [ObservableProperty]
    private string draft = String.Empty;


    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StatusMessage))]
    private string coverLetterError = String.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StatusMessage))]
    private string coverLetterStatus = String.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StatusMessage))]
    private string verificationStatus = String.Empty;

    public string StatusMessage => (CoverLetterError == string.Empty ? CoverLetterStatus : CoverLetterError) + "  " + VerificationStatus;



    [RelayCommand(CanExecute = nameof(CanGenerateCoverLetter))]
    private async Task GenerateCoverLetter(CancellationToken cancellationToken = default)
    {
        try
        {
            CoverLetterStatus = "Generating cover letter draft...";
            VerificationStatus = string.Empty;
            Draft = await coverLetterService.GenerateCoverLetterAsync(draftParameters, cancellationToken);
            CoverLetterStatus = "Done.";

            VerificationStatus = "Verifying cover letter draft...";

            var verificationResult = await coverLetterService.VerifyDraftAsync(draftParameters, Draft, cancellationToken);

            if (!verificationResult.IsValid)
            {
                VerificationStatus = "Verification failed. Please review the issues.";
                DisplayVerificationResult(verificationResult);
            }
            else
            {
                VerificationStatus = "Verification passed.";
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error generating cover letter");
            CoverLetterError = $"Error generating cover letter: {ex.Message}";
        }
    }
    public bool CanGenerateCoverLetter => true;

    [RelayCommand]
    private void BackToRequirements()
    {
        navigation.GoToTab(DraftTab.RequirementsTab);
    }

    [RelayCommand]
    private void SaveCoverLetter()
    {
        try
        {
            fileService.SaveDraftToNotes(Draft, OutputFolder);
            CoverLetterStatus = "Cover letter saved successfully.";
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error saving cover letter");
            CoverLetterError = $"Error saving cover letter: {ex.Message}";
        }
    }

    private void DisplayVerificationResult(VerificationResult verificationResult)
    {
        var verificationResultDialogViewModel = new VerificationResultDialogViewModel(verificationResult);
        windowService.ShowDialog(verificationResultDialogViewModel);
    }
}
