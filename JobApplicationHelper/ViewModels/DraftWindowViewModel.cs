using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JobApplicationHelper.Models;
using JobApplicationHelper.Services;
using JobApplicationHelper.WindowService;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace JobApplicationHelper.ViewModels;

public partial class DraftWindowViewModel : ViewModelBase
{
    private readonly FileService fileService;
    private readonly JobRequirementService jobRequirementService;
    private readonly CoverLetterService coverLetterService;
    private readonly IWindowService windowService;
    private readonly ILogger<DraftWindowViewModel> logger;

    public DraftWindowViewModel(
        FileService fileService,
        JobRequirementService jobRequirementService,
        CoverLetterService coverLetterService,
        IWindowService windowService,
        ILogger<DraftWindowViewModel> logger)
    {
        this.fileService = fileService;
        this.jobRequirementService = jobRequirementService;
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
            StatusMessage = "Analyzing job requirements...";

            var jobRequirements = await jobRequirementService.ExtractRequirementsAsync(JobPosting);
            StatusMessage = "Requirements extracted.";
            
            Draft = jobRequirements.ToString();

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
