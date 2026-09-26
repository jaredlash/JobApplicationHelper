using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JobApplicationHelper.Application.Services;
using JobApplicationHelper.Domain.Models;
using JobApplicationHelper.Services.Api;
using JobApplicationHelper.WindowService;
using Microsoft.Extensions.Logging;

namespace JobApplicationHelper.ViewModels;

public partial class CoverLetterViewModel : ViewModelBase
{
    private readonly CoverLettersApiClient coverLettersApiClient;
    private readonly JobApplicationsApiClient jobApplicationsApiClient;
    private readonly IDraftNavigation navigation;
    private readonly IWindowService windowService;
    private readonly CoverLetterDraftParameters draftParameters;
    private readonly ILogger<CoverLetterViewModel> logger;


    public CoverLetterViewModel(
        CoverLettersApiClient coverLettersApiClient,
        JobApplicationsApiClient jobApplicationsApiClient,
        IDraftNavigation navigation,
        IWindowService windowService,
        CoverLetterDraftParameters draftParameters,
        ILogger<CoverLetterViewModel> logger)
    {
        this.coverLettersApiClient = coverLettersApiClient;
        this.jobApplicationsApiClient = jobApplicationsApiClient;
        this.navigation = navigation;
        this.windowService = windowService;
        this.draftParameters = draftParameters;
        this.logger = logger;
    }

    public string CountryCode { get; set; } = string.Empty;

    [ObservableProperty]
    private JobApplicationId? applicationId;

    [ObservableProperty]
    private string draft = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StatusMessage))]
    private string coverLetterError = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StatusMessage))]
    private string coverLetterStatus = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StatusMessage))]
    private string verificationStatus = string.Empty;

    public string StatusMessage => (CoverLetterError == string.Empty ? CoverLetterStatus : CoverLetterError) + "  " + VerificationStatus;



    [RelayCommand(CanExecute = nameof(CanGenerateCoverLetter))]
    private async Task GenerateCoverLetter(CancellationToken cancellationToken = default)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(ApplicationId);

            CoverLetterStatus = "Generating cover letter draft...";
            VerificationStatus = string.Empty;
            draftParameters.CountryCode = CountryCode;
            Draft = await coverLettersApiClient.GenerateCoverLetterAsync(draftParameters, cancellationToken);
            CoverLetterStatus = "Done.";

            VerificationStatus = "Verifying cover letter draft...";

            var verificationResult = await coverLettersApiClient.VerifyDraftAsync(draftParameters, Draft, cancellationToken);

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
    private async Task SaveCoverLetter()
    {
        try
        {
            ArgumentNullException.ThrowIfNull(ApplicationId);

            await jobApplicationsApiClient.SaveCoverLetter(ApplicationId, Draft);
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
