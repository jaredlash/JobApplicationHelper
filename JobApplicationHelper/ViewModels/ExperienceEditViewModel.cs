using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JobApplicationHelper.Domain.Models;
using JobApplicationHelper.Services.Api;
using JobApplicationHelper.WindowService;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace JobApplicationHelper.ViewModels;

public partial class ExperienceEditViewModel : ViewModelBase
{
    private readonly ExperiencesApiClient experiencesApiClient;
    private readonly IWindowService windowService;

    public ExperienceEditViewModel(ExperiencesApiClient experiencesApiClient, IWindowService windowService)
    {
        this.experiencesApiClient = experiencesApiClient;
        this.windowService = windowService;
    }

    private Experience? originalExperience;

    public bool IsEditMode => originalExperience is not null;

    public string DialogTitle => IsEditMode ? "Edit Experience" : "Add Experience";

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Title is required")]
    private string title = string.Empty;

    [ObservableProperty]
    private ExperienceType type;

    public ExperienceType[] AvailableTypes => Enum.GetValues<ExperienceType>().OrderBy(x => x.ToString()).ToArray();

    [ObservableProperty]
    private string organization = string.Empty;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [CustomValidation(typeof(ExperienceEditViewModel), nameof(ValidateStartDateText))]
    private string startDateText = string.Empty;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [CustomValidation(typeof(ExperienceEditViewModel), nameof(ValidateEndDateText))]
    private string endDateText = string.Empty;

    partial void OnStartDateTextChanged(string value)
    {
        ValidateProperty(EndDateText, nameof(EndDateText));
    }

    partial void OnEndDateTextChanged(string value)
    {
        ValidateProperty(StartDateText, nameof(StartDateText));
    }

    [ObservableProperty]
    private string summary = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SkillsText))]
    private List<string> skills = [];

    public string SkillsText => string.Join(", ", Skills);

    [ObservableProperty]
    private ObservableCollection<EditableEvidence> evidence = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ContextsText))]
    private List<string> contexts = [];

    public string ContextsText => string.Join(", ", Contexts);

    [ObservableProperty]
    private ObservableCollection<EditableLink> links = [];

    [ObservableProperty]
    private string notes = string.Empty;

    public void InitializeForAdd()
    {
        originalExperience = null;

        Title = string.Empty;
        Type = default;
        Organization = string.Empty;
        StartDateText = string.Empty;
        EndDateText = string.Empty;
        Summary = string.Empty;
        Skills = [];
        Evidence = [];
        Contexts = [];
        Links = [];
        Notes = string.Empty;

        OnPropertyChanged(nameof(IsEditMode));
        OnPropertyChanged(nameof(DialogTitle));
    }

    public void InitializeForEdit(Experience experience)
    {
        originalExperience = experience;

        Title = experience.Title;
        Type = experience.Type;
        Organization = experience.Organization;
        StartDateText = PartialDateParser.FormatForEditing(experience.DateRange.Start);
        EndDateText = PartialDateParser.FormatForEditing(experience.DateRange.End);
        Summary = experience.Summary;
        Skills = [.. experience.Skills];
        Evidence = new ObservableCollection<EditableEvidence>(experience.Evidence.Select(e => new EditableEvidence(e)));
        Contexts = [.. experience.Contexts];
        Links = new ObservableCollection<EditableLink>(experience.Links.Select(link => new EditableLink(link)));
        Notes = experience.Notes;

        OnPropertyChanged(nameof(IsEditMode));
        OnPropertyChanged(nameof(DialogTitle));
    }

    [RelayCommand]
    private async Task EditSkills(CancellationToken cancellationToken = default)
    {
        var editSkillsViewModel = new EditSkillsViewModel(experiencesApiClient);

        await editSkillsViewModel.InitializeAsync(Skills, cancellationToken);

        if (windowService.ShowDialog(editSkillsViewModel) == true)
        {
            Skills = editSkillsViewModel.Skills.ToList();
        }
    }

    [RelayCommand]
    private async Task EditContexts(CancellationToken cancellationToken = default)
    {
        var editContextsViewModel = new EditContextsViewModel(experiencesApiClient);

        await editContextsViewModel.InitializeAsync(Contexts, cancellationToken);

        if (windowService.ShowDialog(editContextsViewModel) == true)
        {
            Contexts = editContextsViewModel.Contexts.ToList();
        }
    }

    [RelayCommand]
    private void AddEvidence()
    {
        Evidence.Add(new EditableEvidence());
    }

    [RelayCommand]
    private void RemoveEvidence(EditableEvidence evidence)
    {
        Evidence.Remove(evidence);
    }

    [RelayCommand]
    private void AddLink()
    {
        Links.Add(new EditableLink());
    }

    [RelayCommand]
    private void RemoveLink(EditableLink link)
    {
        Links.Remove(link);
    }

    [RelayCommand]
    private async Task SaveAsync(CancellationToken cancellationToken = default)
    {
        ValidateAllProperties();

        if (HasErrors)
            return;

        var experience = CreateExperience();
        if (experience is null)
            return;

        try
        {
            await experiencesApiClient.AddOrReplaceAsync(experience, cancellationToken);

            TryClose(true);
        }
        catch (Exception ex)
        {
            windowService.ShowAlertDialog("Problem saving experience", $"An error occurred when trying to save the experience: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task CancelAsync(CancellationToken cancellationToken = default)
    {
        var experience = CreateExperience();

        if ((IsEditMode == false && IsBlank == false)  // Add-mode and the form is no longer blank
            || (IsEditMode && ExperienceComparer.AreEquivalent(experience, originalExperience) == false)) // Edit mode and the form has been modified
        {
            var response = windowService.ShowConfirmDialog("Unsaved changes", "There are unsaved changes.  Are you sure?");
            if (response == true)
            {
                TryClose(false);
            }
            return;
        }

        TryClose(false);
    }

    private Experience? CreateExperience()
    {
        if (!TryCreateDateRange(out var dateRange))
            return null;

        var experience = new Experience()
        {
            Id = originalExperience?.Id ?? new ExperienceId(Guid.NewGuid()),
            Title = Title,
            Type = Type,
            Organization = Organization,
            DateRange = dateRange,
            Summary = Summary,
            Skills = [.. Skills],
            Evidence = [.. Evidence.Select(e => e.Value.Trim()).Where(e => !string.IsNullOrWhiteSpace(e))],
            Contexts = [.. Contexts],
            Links = [.. Links.Select(link => link.Value.Trim()).Where(link => !string.IsNullOrWhiteSpace(link))],
            Notes = Notes
        };

        return experience;
    }

    private bool IsBlank =>
        string.IsNullOrWhiteSpace(Title)
            && Type == default
            && string.IsNullOrWhiteSpace(Organization)
            && string.IsNullOrWhiteSpace(StartDateText)
            && string.IsNullOrWhiteSpace(EndDateText)
            && string.IsNullOrWhiteSpace(Summary)
            && Skills.Count == 0
            && Evidence.Count == 0
            && Contexts.Count == 0
            && Links.Count == 0
            && string.IsNullOrWhiteSpace(Notes);

    private bool TryCreateDateRange(out DateRange dateRange)
    {
        if (!PartialDateParser.TryParse(StartDateText, out var start) ||
            !PartialDateParser.TryParse(EndDateText, out var end))
        {
            dateRange = new DateRange();
            return false;
        }

        dateRange = start is null && end is null
            ? new DateRange()
            : new DateRange { Start = start, End = end };

        return true;
    }

    public static ValidationResult? ValidateStartDateText(string? value, ValidationContext context)
    {
        var viewModel = (ExperienceEditViewModel)context.ObjectInstance;

        if (!PartialDateParser.TryParse(value, out var start))
        {
            return new ValidationResult("Enter a valid start date using YYYY, YYYY-MM, or YYYY-MM-DD.");
        }

        if (!PartialDateParser.TryParse(viewModel.EndDateText, out var end))
        {
            // The end-date property will report its own parsing error.
            return ValidationResult.Success;
        }

        if (IsStartAfterEnd(start, end))
        {
            return new ValidationResult("Start date must not be later than end date.");
        }

        return ValidationResult.Success;
    }

    public static ValidationResult? ValidateEndDateText(string? value, ValidationContext context)
    {
        var viewModel = (ExperienceEditViewModel)context.ObjectInstance;

        if (!PartialDateParser.TryParse(value, out var end))
        {
            return new ValidationResult("Enter a valid end date using YYYY, YYYY-MM, or YYYY-MM-DD.");
        }

        if (!PartialDateParser.TryParse(viewModel.StartDateText, out var start))
        {
            // The start-date property will report its own parsing error.
            return ValidationResult.Success;
        }

        if (IsStartAfterEnd(start, end))
        {
            return new ValidationResult("Start date must not be later than end date.");
        }

        return ValidationResult.Success;
    }

    private static bool IsStartAfterEnd(PartialDate? start, PartialDate? end)
    {
        if (start is null || end is null)
            return false;

        // Missing month/day components are treated as January/the first day.
        var startKey = (start.Year, start.Month ?? 1, start.Day ?? 1);

        var endKey = (end.Year, end.Month ?? 1, end.Day ?? 1);

        return startKey.CompareTo(endKey) > 0;
    }
}