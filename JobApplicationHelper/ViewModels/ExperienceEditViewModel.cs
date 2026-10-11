using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JobApplicationHelper.Contracts.Experiences;
using JobApplicationHelper.Domain.Models;
using JobApplicationHelper.Services.Api;

namespace JobApplicationHelper.ViewModels;

public partial class ExperienceEditViewModel : ViewModelBase
{
    private readonly ExperiencesApiClient experiencesApiClient;

    public ExperienceEditViewModel(ExperiencesApiClient experiencesApiClient)
    {
        this.experiencesApiClient = experiencesApiClient;
        DateRange = new DateRange();
    }

    private Experience? _originalExperience;

    public bool IsEditMode => _originalExperience is not null;

    public string DialogTitle => IsEditMode ? "Edit Experience" : "Add Experience";

    [ObservableProperty]
    private ExperienceId id;

    [ObservableProperty]
    private string title = string.Empty;

    [ObservableProperty]
    private ExperienceType type;

    [ObservableProperty]
    private string? organization;

    [ObservableProperty]
    private DateRange dateRange;

    [ObservableProperty]
    private string? summary;

    [ObservableProperty]
    private List<string> skills = [];

    [ObservableProperty]
    private List<string> evidence = [];

    [ObservableProperty]
    private List<string> contexts = [];

    [ObservableProperty]
    private List<string> links = [];

    [ObservableProperty]
    private string? notes;

    public void InitializeForAdd()
    {
        _originalExperience = null;

        Id = new ExperienceId(Guid.NewGuid());
        Title = string.Empty;
        Type = default;
        Organization = null;
        DateRange = new DateRange();
        Summary = null;
        Skills = [];
        Evidence = [];
        Contexts = [];
        Links = [];
        Notes = null;

        OnPropertyChanged(nameof(IsEditMode));
        OnPropertyChanged(nameof(DialogTitle));
    }

    public void InitializeForEdit(Experience experience)
    {
        _originalExperience = experience;

        Id = experience.Id;
        Title = experience.Title;
        Type = experience.Type;
        Organization = experience.Organization;
        //DateRange = CloneDateRange(experience.DateRange);
        Summary = experience.Summary;
        Skills = [.. experience.Skills];
        Evidence = [.. experience.Evidence];
        Contexts = [.. experience.Contexts];
        Links = [.. experience.Links];
        Notes = experience.Notes;

        OnPropertyChanged(nameof(IsEditMode));
        OnPropertyChanged(nameof(DialogTitle));
    }

    [RelayCommand]
    private async Task SaveAsync(CancellationToken cancellationToken = default)
    {
        ValidateAllProperties();

        //if (HasErrors)
        //    return;

        //if (!TryCreateDateRange(out var dateRange))
        //    return;

        //var experience = CreateExperience(dateRange);

        //try
        //{
        //    await experiencesApiClient.AddOrReplaceAsync(experience, cancellationToken);
        //    TryClose(true);
        //}
        //catch (Exception ex)
        //{
        //    // Expose an appropriate error message for the UI.
        //    //ErrorMessage = ex.Message;
        //}
    }

    [RelayCommand]
    private void Cancel()
    {
        TryClose(false);
    }
}