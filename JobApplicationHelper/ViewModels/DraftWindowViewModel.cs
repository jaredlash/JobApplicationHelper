using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JobApplicationHelper.Models;
using JobApplicationHelper.Services;
using JobApplicationHelper.WindowService;
using Microsoft.Extensions.Logging;

namespace JobApplicationHelper.ViewModels;

public partial class DraftWindowViewModel : ViewModelBase
{
    private readonly FileService fileService;
    private readonly CoverLetterService coverLetterService;
    private readonly IWindowService windowService;
    private readonly ILogger<DraftWindowViewModel> logger;

    public DraftWindowViewModel(
        FileService fileService,
        CoverLetterService coverLetterService,
        IWindowService windowService,
        ILogger<DraftWindowViewModel> logger)
    {
        this.fileService = fileService;
        this.coverLetterService = coverLetterService;
        this.windowService = windowService;
        this.logger = logger;
    }

    [ObservableProperty]
    private string cvText = String.Empty;

    [ObservableProperty]
    private string outputFolder = String.Empty;

    [ObservableProperty]
    private string jobPosting = String.Empty;

    [ObservableProperty]
    private string additionalPromptInstructions = String.Empty;

    [ObservableProperty]
    private int selectedTabIndex = 0;

    [ObservableProperty]
    private string draft = String.Empty;

    [ObservableProperty]
    private string statusMessage = String.Empty;

    [RelayCommand]
    private async Task GenerateCoverLetter()
    {
        SelectedTabIndex = 1;
        try
        {
            var experienceBank = await this.fileService.LoadExperienceBank();
            StatusMessage = "Analyzing application materials...";
            var request = new Models.CoverLetterRequest(
                CvText,
                JobPosting,
                AdditionalPromptInstructions,
                "Professional",
                "Formal, concise",
                "Dutch professional audience",
                /* Other potential audiences:
                 * German professional audience
                 * International/neutral professional audience
                 */
                experienceBank,
                350);
            var applicationAnalysis = await coverLetterService.AnalyzeApplicationAsync(request);

            StatusMessage = "Generating cover letter draft...";
            Draft = await coverLetterService.GenerateCoverLetterAsync(request, applicationAnalysis);

            StatusMessage = "Verifying cover letter draft...";

            var verificationResult = await coverLetterService.VerifyDraftAsync(request, Draft);

            if (!verificationResult.IsValid)
            {
                StatusMessage = "Verification failed. Please review the issues.";
                DisplayVerificationResult(verificationResult);
            }
            else
            {
                StatusMessage = "Done. Verification passed.";
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error generating cover letter");
            StatusMessage = $"Error generating cover letter: {ex.Message}";
        }
    }

    [RelayCommand]
    private void BackToEdit()
    {
        SelectedTabIndex = 0;
    }

    [RelayCommand]
    private void SaveCoverLetter()
    {
        try {
            fileService.SaveDraftToNotes(Draft, OutputFolder);
            StatusMessage = "Cover letter saved successfully.";
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error saving cover letter");
            StatusMessage = $"Error saving cover letter: {ex.Message}";
        }
    }

    private void DisplayVerificationResult(VerificationResult verificationResult)
    {
        var verificationResultDialogViewModel = new VerificationResultDialogViewModel(verificationResult);
        windowService.ShowDialog(verificationResultDialogViewModel);
    }
}
