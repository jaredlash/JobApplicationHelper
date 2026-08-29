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
    private readonly ILogger<CoverLetterViewModel> logger;


    public CoverLetterViewModel(
        FileService fileService,
        CoverLetterService coverLetterService,
        IDraftNavigation navigation,
        IWindowService windowService,
        ILogger<CoverLetterViewModel> logger)
    {
        this.fileService = fileService;
        this.coverLetterService = coverLetterService;
        this.navigation = navigation;
        this.windowService = windowService;
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

    public string StatusMessage => CoverLetterError == string.Empty ? CoverLetterStatus : CoverLetterError;



    [RelayCommand(CanExecute = nameof(CanGenerateCoverLetter))]
    private async Task GenerateCoverLetter()
    {
        try
        {
            CoverLetterStatus = "Generating cover letter...";

            //var request = new Models.CoverLetterRequest(
            //    CvText,
            //    JobPosting,
            //    AdditionalPromptInstructions,
            //    "Professional",
            //    "Formal, concise",
            //    "Dutch professional audience",
            //    /* Other potential audiences:
            //     * German professional audience
            //     * International/neutral professional audience
            //     */
            //    experienceBank,
            //    350);
            //var applicationAnalysis = await coverLetterService.AnalyzeApplicationAsync(request);
            //StatusMessage = "Analysis complete.";

            //var options = new JsonSerializerOptions { WriteIndented = true };
            //options.Converters.Add(new JsonStringEnumConverter());

            //// Serialize and print
            //string formattedJson = JsonSerializer.Serialize(applicationAnalysis, options);

            //// Verify experience bank IDs are real
            //var experienceBankIds = new HashSet<string>(experienceBank.Experiences.Select(e => e.Id));
            //var sb = new System.Text.StringBuilder("Verifying experience bank IDs...\n");
            //int foundErrorCount = 0;
            //foreach (var req in applicationAnalysis.JobRequirements)
            //{
            //    foreach (var evidenceRef in req.Evidence)
            //    {
            //        if (evidenceRef.Source != EvidenceSource.ExperienceBank)
            //        {
            //            if (evidenceRef.ExperienceIds.Count > 0)
            //            {
            //                sb.AppendLine("Found id not from experience bank for requirement " + req.Requirement);
            //                foundErrorCount++;
            //                continue;
            //            }
            //        }

            //        foreach (var expId in evidenceRef.ExperienceIds)
            //        {
            //            if (!experienceBankIds.Contains(expId))
            //            {
            //                sb.AppendLine($"Experience ID '{expId}' in requirement '{req.Requirement}' does not exist in the experience bank.");
            //                foundErrorCount++;
            //            }
            //        }
            //    };
            //};
            //if (foundErrorCount == 0)
            //{
            //    sb.AppendLine("All experience bank IDs are valid.");
            //}
            //else
            //{
            //    sb.AppendLine($"Found {foundErrorCount} invalid experience bank IDs.");
            //}
            //Draft = sb.ToString() + formattedJson;

            //StatusMessage = "Generating cover letter draft...";
            //Draft = await coverLetterService.GenerateCoverLetterAsync(request, applicationAnalysis);

            //StatusMessage = "Verifying cover letter draft...";

            //var verificationResult = await coverLetterService.VerifyDraftAsync(request, Draft);

            //if (!verificationResult.IsValid)
            //{
            //    StatusMessage = "Verification failed. Please review the issues.";
            //    DisplayVerificationResult(verificationResult);
            //}
            //else
            //{
            //    StatusMessage = "Done. Verification passed.";
            //}
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
