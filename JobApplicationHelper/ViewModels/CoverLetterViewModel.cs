using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JobApplicationHelper.Contracts.BackgroundJobs;
using JobApplicationHelper.Domain.Models;
using JobApplicationHelper.Exceptions;
using JobApplicationHelper.Services.Api;
using JobApplicationHelper.Services.BackgroundJobs;
using JobApplicationHelper.WindowService;
using Microsoft.Extensions.Logging;

namespace JobApplicationHelper.ViewModels;

public partial class CoverLetterViewModel : ViewModelBase
{
    private readonly CoverLettersApiClient coverLettersApiClient;
    private readonly IBackgroundJobPollingService backgroundJobPollingService;
    private readonly JobApplicationsApiClient jobApplicationsApiClient;
    private readonly IDraftNavigation navigation;
    private readonly IWindowService windowService;
    private readonly CoverLetterDraftParameters draftParameters;
    private readonly ILogger<CoverLetterViewModel> logger;


    public CoverLetterViewModel(
        CoverLettersApiClient coverLettersApiClient,
        IBackgroundJobPollingService backgroundJobPollingService,
        JobApplicationsApiClient jobApplicationsApiClient,
        IDraftNavigation navigation,
        IWindowService windowService,
        CoverLetterDraftParameters draftParameters,
        ILogger<CoverLetterViewModel> logger)
    {
        this.coverLettersApiClient = coverLettersApiClient;
        this.backgroundJobPollingService = backgroundJobPollingService;
        this.jobApplicationsApiClient = jobApplicationsApiClient;
        this.navigation = navigation;
        this.windowService = windowService;
        this.draftParameters = draftParameters;
        this.logger = logger;
    }

    public string CountryCode { get; set; } = string.Empty;


    public JobApplicationId? ApplicationId { get; set; }

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
            var jobApplicationId = ApplicationId.Value;

            CoverLetterStatus = "Generating cover letter draft...";
            VerificationStatus = string.Empty;
            draftParameters.CountryCode = CountryCode;
            var backgroundJobId = await coverLettersApiClient.GenerateCoverLetterAsync(jobApplicationId, draftParameters, cancellationToken);
            var backgroundJobResponse = await backgroundJobPollingService.WaitForCompletionAsync(backgroundJobId, cancellationToken);
            EnsureBackgroundJobSucceeded(backgroundJobResponse);

            Draft = await coverLettersApiClient.GetCoverLetterDraftAsync(jobApplicationId, cancellationToken);
            CoverLetterStatus = "Done.";

            VerificationStatus = "Verifying cover letter draft...";
            var verificationBackgroundJobId = await coverLettersApiClient.VerifyDraftAsync(jobApplicationId, draftParameters, Draft, cancellationToken);
            var verificationJobResponse = await backgroundJobPollingService.WaitForCompletionAsync(verificationBackgroundJobId, cancellationToken);
            EnsureBackgroundJobSucceeded(verificationJobResponse);

            var verificationResult = await coverLettersApiClient.GetVerifyCoverLetterResultAsync(jobApplicationId, cancellationToken);

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
            if (ApplicationId is null)
            {
                throw new InvalidOperationException("ApplicationId has not been set.");
            }

            await jobApplicationsApiClient.SaveCoverLetter(ApplicationId.Value, Draft);

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

    private static void EnsureBackgroundJobSucceeded(GetBackgroundJobResponse response)
    {
        if (response.Status == BackgroundJobStatus.Failed.ToString())
        {
            throw new BackgroundJobFailedException(response.Error);
        }

        if (response.Status == BackgroundJobStatus.Cancelled.ToString())
        {
            throw new OperationCanceledException("The background job was cancelled.");
        }
    }
}
