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
    private readonly IDraftNavigation navigation;
    private readonly IWindowService windowService;
    private readonly CoverLetterDraftParameters draftParameters;
    private readonly ILogger<JobRequirementsViewModel> logger;


    public JobRequirementsViewModel(
        JobRequirementService jobRequirementService,
        IExperienceBankService experienceBankService,
        IDraftNavigation navigation,
        IWindowService windowService,
        CoverLetterDraftParameters draftParameters,
        ILogger<JobRequirementsViewModel> logger)
    {
        this.jobRequirementService = jobRequirementService;
        this.experienceBankService = experienceBankService;
        this.navigation = navigation;
        this.windowService = windowService;
        this.draftParameters = draftParameters;
        this.logger = logger;
    }

    public JobRequirements Requirements
    {
        get => draftParameters.Requirements;
        set
        {
            if (ReferenceEquals(draftParameters.Requirements, value))
                return;

            draftParameters.Requirements = value;

            OnPropertyChanged(nameof(Requirements));
            OnPropertyChanged(nameof(RequirementCount));

            NextRequirementCommand.NotifyCanExecuteChanged();
            PreviousRequirementCommand.NotifyCanExecuteChanged();
        }
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectedRequirementDisplay))]
    [NotifyPropertyChangedFor(nameof(SelectedRequirement))]
    [NotifyPropertyChangedFor(nameof(StatusMessage))]
    [NotifyPropertyChangedFor(nameof(NoSupportingEvidence))]
    [NotifyPropertyChangedFor(nameof(RequirementEvidenceExperiences))]
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
        var filteredExperiences = _allExperiences.Except(SelectedRequirement?.Evidence.Evidences.Select(ev => ev.Experience).ToList() ?? []);
        if (string.IsNullOrWhiteSpace(filter))
            return filteredExperiences.ToList();

        return filteredExperiences
            .Where(e => e.MatchFilter(filter))
            .ToList();
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectedExperience))]
    [NotifyCanExecuteChangedFor(nameof(AddExperienceAsEvidenceCommand))]
    [NotifyCanExecuteChangedFor(nameof(RemoveEvidenceFromExperienceCommand))]
    private int selectedExperienceIndex = -1;
    partial void OnSelectedExperienceIndexChanged(int value)
    {
        if (value < 0) return;

        SelectedEvidenceIndex = -1;
    }

    public Experience? SelectedExperience => Experiences.Count > 0 && SelectedExperienceIndex >= 0
        ? Experiences[SelectedExperienceIndex]
        : null;

    // This always returns a new collection/reference which will refresh the listbox to which this is bound
    // The evidence lists should be of negligible length so creating a new one on every update should be fine
    public IReadOnlyList<Experience> RequirementEvidenceExperiences => SelectedRequirement?.Evidence.Evidences.Select(ev => ev.Experience).ToList() ?? [];

    private IReadOnlyList<Evidence> RequirementEvidences => SelectedRequirement?.Evidence.Evidences ?? [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectedEvidence))]
    [NotifyPropertyChangedFor(nameof(CanEditEvidenceNote))]
    [NotifyCanExecuteChangedFor(nameof(AddExperienceAsEvidenceCommand))]
    [NotifyCanExecuteChangedFor(nameof(RemoveEvidenceFromExperienceCommand))]
    private int selectedEvidenceIndex = -1;
    partial void OnSelectedEvidenceIndexChanged(int value)
    {
        if (value  < 0) return;

        SelectedExperienceIndex = -1;
    }

    public Evidence? SelectedEvidence => SelectedEvidenceIndex >= 0 ? RequirementEvidences[SelectedEvidenceIndex] : null;
    public bool CanEditEvidenceNote => SelectedEvidenceIndex >= 0;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StatusMessage))]
    [NotifyCanExecuteChangedFor(nameof(AddExperienceAsEvidenceCommand))]
    [NotifyCanExecuteChangedFor(nameof(RemoveEvidenceFromExperienceCommand))]
    [NotifyCanExecuteChangedFor(nameof(NextRequirementCommand))]
    [NotifyCanExecuteChangedFor(nameof(PreviousRequirementCommand))]
    [NotifyCanExecuteChangedFor(nameof(LoadJobRequirementsCommand))]
    [NotifyCanExecuteChangedFor(nameof(GenerateCoverLetterCommand))]
    private bool isFinishedLoadingJobRequirements = false;

    [ObservableProperty]
    private string jobPosting = String.Empty;

    private string jobRequirementsError = String.Empty;
    private string jobRequirementsStatus => !IsFinishedLoadingJobRequirements
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
            IsFinishedLoadingJobRequirements = false;
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
            IsFinishedLoadingJobRequirements = true;
        }
    }
    public bool CanExecuteLoadJobRequirements => IsFinishedLoadingJobRequirements;

    [RelayCommand(CanExecute = nameof(CanGoToNextRequirement))]
    private void NextRequirement()
    {
        if (SelectedRequirementIndex < Requirements.Requirements.Count - 1)
        {
            SelectedRequirementIndex++;

            OnExperienceFilterChanged(ExperienceFilter);
        }
    }
    public bool CanGoToNextRequirement => IsFinishedLoadingJobRequirements && SelectedRequirementIndex < Requirements.Requirements.Count - 1;

    [RelayCommand(CanExecute = nameof(CanGoToPreviousRequirement))]
    private void PreviousRequirement()
    {
        if (SelectedRequirementIndex > 0)
        {
            SelectedRequirementIndex--;

            OnExperienceFilterChanged(ExperienceFilter);
        }
    }
    public bool CanGoToPreviousRequirement => IsFinishedLoadingJobRequirements && SelectedRequirementIndex > 0;

    [RelayCommand(CanExecute = nameof(CanAddExperienceAsEvidence))]
    private void AddExperienceAsEvidence()
    {
        if (SelectedRequirement is null || SelectedExperience is null) return;

        var evidence = new Evidence { Experience = SelectedExperience };
        var result = SelectedRequirement.Evidence.AddEvidence(evidence);
        if (result) SelectLastEvidence();
        OnPropertyChanged(nameof(RequirementEvidenceExperiences));
        OnPropertyChanged(nameof(StatusMessage));
        OnExperienceFilterChanged(ExperienceFilter);
        GenerateCoverLetterCommand.NotifyCanExecuteChanged();
    }
    public bool CanAddExperienceAsEvidence => IsFinishedLoadingJobRequirements && SelectedExperienceIndex >= 0;


    private void SelectLastEvidence()
    {
        var lastIndex = RequirementEvidences.Count - 1;
        if (lastIndex >= 0)
            SelectedEvidenceIndex = lastIndex;
    }

    [RelayCommand(CanExecute = nameof(CanRemoveEvidenceFromExperience))]
    private void RemoveEvidenceFromExperience()
    {
        if (SelectedRequirement is null || SelectedEvidence is null) return;

        SelectedRequirement.Evidence.RemoveEvidence(SelectedEvidence);
        OnPropertyChanged(nameof(RequirementEvidenceExperiences));
        OnPropertyChanged(nameof(StatusMessage));
        OnExperienceFilterChanged(ExperienceFilter);
        GenerateCoverLetterCommand.NotifyCanExecuteChanged();
    }
    public bool CanRemoveEvidenceFromExperience => IsFinishedLoadingJobRequirements && SelectedEvidenceIndex >= 0;


    [RelayCommand(CanExecute = nameof(CanGenerateCoverLetter))]
    private async Task GenerateCoverLetter()
    {
        navigation.GoToTab(DraftTab.CoverLetterTab);
    }

    public bool CanGenerateCoverLetter => IsFinishedLoadingJobRequirements && FulfilledRequirementCount == RequirementCount;
}
