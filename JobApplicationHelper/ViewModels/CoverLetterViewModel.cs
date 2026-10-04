using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JobApplicationHelper.Domain.Models;
using JobApplicationHelper.Exceptions;
using JobApplicationHelper.Extensions;
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
    [NotifyPropertyChangedFor(nameof(CanVerifyCoverLetter))]
    [NotifyCanExecuteChangedFor(nameof(VerifyCoverLetterCommand))]
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

    public string StatusMessage
    {
        get
        {
            var primaryStatus = string.IsNullOrEmpty(CoverLetterError)
                ? CoverLetterStatus
                : CoverLetterError;

            if (IsGeneratingCoverLetter)
                return primaryStatus;

            return string.IsNullOrEmpty(VerificationStatus)
                ? primaryStatus
                : $"{primaryStatus}  {VerificationStatus}";
        }
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StatusMessage))]
    [NotifyPropertyChangedFor(nameof(CanVerifyCoverLetter))]
    [NotifyCanExecuteChangedFor(nameof(VerifyCoverLetterCommand))]
    private bool isGeneratingCoverLetter = false;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanGenerateCoverLetter))]
    [NotifyCanExecuteChangedFor(nameof(GenerateCoverLetterCommand))]
    private bool isVerifyingCoverLetter = false;

    [RelayCommand(CanExecute = nameof(CanGenerateCoverLetter), IncludeCancelCommand = true)]
    private async Task GenerateCoverLetter(CancellationToken cancellationToken = default)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(ApplicationId);
            var jobApplicationId = ApplicationId.Value;
            IsGeneratingCoverLetter = true;

            CoverLetterStatus = "Generating cover letter draft...";
            CoverLetterError = "";
            VerificationStatus = string.Empty;
            draftParameters.CountryCode = CountryCode;

            var backgroundJobId = await coverLettersApiClient.GenerateCoverLetterAsync(jobApplicationId, draftParameters, cancellationToken);
            await WaitForBackgroundJobAsync(backgroundJobId, cancellationToken);

            Draft = await coverLettersApiClient.GetCoverLetterDraftAsync(jobApplicationId, cancellationToken);
            CoverLetterStatus = "Done.";
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("Cover letter generation was cancelled.");
            CoverLetterStatus = "Cover letter generation was cancelled.";
        }
        catch (BackgroundJobCancelledException)
        {
            logger.LogInformation("The background job was cancelled.");
            CoverLetterStatus = "Cover letter generation was cancelled.";
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error generating cover letter");
            CoverLetterError = $"Error generating cover letter: {ex.Message}";
            CoverLetterStatus = "";
        }
        finally
        {
            IsGeneratingCoverLetter = false;
        }
    }
    private bool CanGenerateCoverLetter => !IsVerifyingCoverLetter;

    [RelayCommand(CanExecute = nameof(CanVerifyCoverLetter), IncludeCancelCommand = true)]
    private async Task VerifyCoverLetter(CancellationToken cancellationToken = default)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(ApplicationId);
            var jobApplicationId = ApplicationId.Value;
            IsVerifyingCoverLetter = true;

            draftParameters.CountryCode = CountryCode;

            VerificationStatus = "Verifying cover letter draft...";
            var verificationBackgroundJobId = await coverLettersApiClient.VerifyDraftAsync(jobApplicationId, draftParameters, Draft, cancellationToken);
            await WaitForBackgroundJobAsync(verificationBackgroundJobId, cancellationToken);

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
        catch (OperationCanceledException)
        {
            logger.LogInformation("Verifying cover letter was cancelled.");
            VerificationStatus = "Verifying cover letter was cancelled.";
        }
        catch (BackgroundJobCancelledException)
        {
            logger.LogInformation("The background job was cancelled.");
            VerificationStatus = "Verifying cover letter was cancelled.";
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error verifying cover letter");
            VerificationStatus = $"Error verifying cover letter: {ex.Message}";
        }
        finally
        {
            IsVerifyingCoverLetter = false;
        }
    }
    private bool CanVerifyCoverLetter => !IsGeneratingCoverLetter && !string.IsNullOrWhiteSpace(Draft);

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

    private async Task WaitForBackgroundJobAsync(BackgroundJobId backgroundJobId, CancellationToken cancellationToken)
    {
        var response = await backgroundJobPollingService.WaitForCompletionAsync(backgroundJobId, cancellationToken);

        response.EnsureSucceeded();
    }
}
