using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JobApplicationHelper.Domain.Models;
using JobApplicationHelper.Services.Api;
using JobApplicationHelper.WindowService;
using System.Collections.ObjectModel;

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
    private string title = string.Empty;

    [ObservableProperty]
    private ExperienceType type;

    public ExperienceType[] AvailableTypes => Enum.GetValues<ExperienceType>().OrderBy(x => x.ToString()).ToArray();

    [ObservableProperty]
    private string organization = string.Empty;

    [ObservableProperty]
    private string startDateText = string.Empty;

    [ObservableProperty]
    private string endDateText = string.Empty;

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
            //await experiencesApiClient.AddOrReplaceAsync(experience, cancellationToken);

            TryClose(true);
        }
        catch (Exception)
        {
            // Expose an appropriate error message for the UI.
            //ErrorMessage = ex.Message;
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
            Id = new ExperienceId(Guid.NewGuid()),
            Title = Title,
            Type = Type,
            Organization = Organization,
            DateRange = dateRange,
            Summary = Summary,
            Skills = [.. Skills],
            Evidence = Evidence.Select(e => e.Value.Trim()).Where(e => !string.IsNullOrWhiteSpace(e)).ToList(),
            Contexts = [.. Contexts],
            Links = Links.Select(link => link.Value.Trim()).Where(link => !string.IsNullOrWhiteSpace(link)).ToList(),
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
}