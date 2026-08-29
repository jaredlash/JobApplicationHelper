using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JobApplicationHelper.Models;
using JobApplicationHelper.Services;
using JobApplicationHelper.WindowService;
using Microsoft.Extensions.Logging;

namespace JobApplicationHelper.ViewModels;

public partial class JobRequirementsViewModel : ViewModelBase
{
    private readonly JobRequirementService jobRequirementService;
    private readonly IExperienceBankService experienceBankService;
    private readonly IWindowService windowService;
    private readonly ILogger<JobRequirementsViewModel> logger;


    public JobRequirementsViewModel(
        JobRequirementService jobRequirementService,
        IExperienceBankService experienceBankService,
        IWindowService windowService,
        ILogger<JobRequirementsViewModel> logger)
    {
        this.jobRequirementService = jobRequirementService;
        this.experienceBankService = experienceBankService;
        this.windowService = windowService;
        this.logger = logger;
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(RequirementCount))]
    [NotifyCanExecuteChangedFor(nameof(NextRequirementCommand))]
    [NotifyCanExecuteChangedFor(nameof(PreviousRequirementCommand))]
    private JobRequirements requirements = new JobRequirements { Requirements = [] };

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectedRequirementDisplay))]
    [NotifyPropertyChangedFor(nameof(SelectedRequirement))]
    [NotifyPropertyChangedFor(nameof(StatusMessage))]
    [NotifyPropertyChangedFor(nameof(NoSupportingEvidence))]
    [NotifyCanExecuteChangedFor(nameof(NextRequirementCommand))]
    [NotifyCanExecuteChangedFor(nameof(PreviousRequirementCommand))]
    private int selectedRequirementIndex = -1;

    public string SelectedRequirementDisplay =>
        SelectedRequirementIndex >= 0 && RequirementCount > 0
            ? $"{SelectedRequirementIndex + 1} / {RequirementCount}"
            : "0 / 0";

    public int RequirementCount => Requirements.Requirements.Count;
    public int FulfilledRequirementCount => Requirements.Requirements.Count(r => r.Evidence.IsSatisfied);
    private string FulfilledRequirementsMessage => $"{FulfilledRequirementCount} of {RequirementCount} requirements addressed";

    public bool NoSupportingEvidence
    {
        get => SelectedRequirement?.Evidence.NoSupportingEvidence ?? false;
        set
        {
            if (SelectedRequirement?.Evidence is null)
                return;

            if (SelectedRequirement.Evidence.NoSupportingEvidence == value)
                return;

            SelectedRequirement.Evidence.NoSupportingEvidence = value;

            OnPropertyChanged(nameof(StatusMessage));
            GenerateCoverLetterCommand.NotifyCanExecuteChanged();
        }
    }

    public JobRequirement? SelectedRequirement => SelectedRequirementIndex >= 0 && SelectedRequirementIndex < Requirements.Requirements.Count
        ? Requirements.Requirements[SelectedRequirementIndex]
        : null;

    [ObservableProperty]
    private IReadOnlyList<Experience> experiences = [];

    private IReadOnlyList<Experience> _allExperiences = [];

    [ObservableProperty]
    private string experienceFilter = string.Empty;

    partial void OnExperienceFilterChanged(string value)
    {
        Experiences = ApplyExperienceFilter(value);
    }

    private IReadOnlyList<Experience> ApplyExperienceFilter(string filter)
    {
        if (string.IsNullOrWhiteSpace(filter))
            return _allExperiences;

        return _allExperiences
            .Where(e => e.MatchFilter(filter))
            .ToList();
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StatusMessage))]
    [NotifyCanExecuteChangedFor(nameof(LoadJobRequirementsCommand))]
    [NotifyCanExecuteChangedFor(nameof(GenerateCoverLetterCommand))]
    private bool isLoadingJobRequirements = false;

    [ObservableProperty]
    private string jobPosting = String.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StatusMessage))]
    private int selectedTabIndex = 0;

    private string jobRequirementsError = String.Empty;
    private string jobRequirementsStatus => IsLoadingJobRequirements
        ? "Analyzing job requirements..."
        : jobRequirementsError == string.Empty
        ? FulfilledRequirementsMessage
        : string.Empty;

    public string StatusMessage => jobRequirementsError == string.Empty ? jobRequirementsStatus : jobRequirementsError;


    [RelayCommand]
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await LoadExperienceBank(cancellationToken);
        await LoadJobRequirements(cancellationToken);
    }

    private async Task LoadExperienceBank(CancellationToken cancellationToken = default)
    {
        _allExperiences = await experienceBankService.GetAllAsync(cancellationToken);
        OnExperienceFilterChanged(ExperienceFilter);
    }

    [RelayCommand(CanExecute = nameof(CanExecuteLoadJobRequirements))]
    private async Task LoadJobRequirements(CancellationToken cancellationToken = default)
    {
        try
        {
            IsLoadingJobRequirements = true;
            Requirements = await jobRequirementService.ExtractRequirementsAsync(JobPosting, cancellationToken);
            // Valid job requirements have at least one requirement
            SelectedRequirementIndex = 0;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error loading job requirements");
            jobRequirementsError = $"Error loading job requirements: {ex.Message}";
        }
        finally
        {
            IsLoadingJobRequirements = false;
        }
    }
    public bool CanExecuteLoadJobRequirements => !IsLoadingJobRequirements;

    [RelayCommand(CanExecute = nameof(CanGoToNextRequirement))]
    private void NextRequirement()
    {
        if (SelectedRequirementIndex < Requirements.Requirements.Count - 1)
        {
            SelectedRequirementIndex++;
        }
    }
    public bool CanGoToNextRequirement => SelectedRequirementIndex < Requirements.Requirements.Count - 1;

    [RelayCommand(CanExecute = nameof(CanGoToPreviousRequirement))]
    private void PreviousRequirement()
    {
        if (SelectedRequirementIndex > 0)
        {
            SelectedRequirementIndex--;
        }
    }
    public bool CanGoToPreviousRequirement => SelectedRequirementIndex > 0;

    [RelayCommand(CanExecute = nameof(CanGenerateCoverLetter))]
    private async Task GenerateCoverLetter()
    {
        SelectedTabIndex = 1;
    }

    public bool CanGenerateCoverLetter => !IsLoadingJobRequirements && FulfilledRequirementCount == RequirementCount;
}
