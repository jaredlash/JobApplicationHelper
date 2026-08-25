using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JobApplicationHelper.Models;
using JobApplicationHelper.Services;
using System.Security.Policy;

namespace JobApplicationHelper.ViewModels;

public partial class ExperienceEditViewModel : ViewModelBase
{
    private readonly IExperienceBankService _experienceBankService;

    private Experience? _originalExperience;

    public bool IsEditMode => _originalExperience is not null;

    public string DialogTitle =>
        IsEditMode ? "Edit Experience" : "Add Experience";

    [ObservableProperty]
    private string _id = string.Empty;

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private ExperienceType _type;

    [ObservableProperty]
    private string? _organization;

    [ObservableProperty]
    private DateRange? _dateRange;

    [ObservableProperty]
    private string? _summary;

    [ObservableProperty]
    private List<string> _skills = [];

    [ObservableProperty]
    private List<string> _evidence = [];

    [ObservableProperty]
    private List<string> _contexts = [];

    [ObservableProperty]
    private List<string> _links = [];

    [ObservableProperty]
    private string? _notes;

    public ExperienceEditViewModel(
        IExperienceBankService experienceBankService)
    {
        _experienceBankService = experienceBankService;
    }

    public void InitializeForAdd()
    {
        _originalExperience = null;

        Id = Guid.NewGuid().ToString("N");
        Title = string.Empty;
        Type = default;
        Organization = null;
        DateRange = null;
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
        ArgumentNullException.ThrowIfNull(experience);

        _originalExperience = experience;

        Id = experience.Id;
        Title = experience.Title;
        Type = experience.Type;
        Organization = experience.Organization;
        DateRange = CloneDateRange(experience.DateRange);
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
    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(Id) ||
            string.IsNullOrWhiteSpace(Title))
        {
            return;
        }

        var experience = CreateExperience();

        if (IsEditMode)
        {
            await _experienceBankService.UpdateAsync(experience);
        }
        else
        {
            await _experienceBankService.AddAsync(experience);
        }

        TryClose(true);
    }

    [RelayCommand]
    private void Cancel()
    {
        TryClose(false);
    }

    private Experience CreateExperience()
    {
        return new Experience
        {
            Id = Id.Trim(),
            Title = Title.Trim(),
            Type = Type,
            Organization = string.IsNullOrWhiteSpace(Organization)
                ? null
                : Organization.Trim(),
            DateRange = CloneDateRange(DateRange),
            Summary = string.IsNullOrWhiteSpace(Summary)
                ? null
                : Summary.Trim(),
            Skills = [.. Skills],
            Evidence = [.. Evidence],
            Contexts = [.. Contexts],
            Links = [.. Links],
            Notes = string.IsNullOrWhiteSpace(Notes)
                ? null
                : Notes.Trim()
        };
    }

    private static DateRange? CloneDateRange(DateRange? dateRange)
    {
        if (dateRange is null)
        {
            return null;
        }

        return new DateRange
        {
            Start = dateRange.Start is null
                ? null
                : new PartialDate
                {
                    Year = dateRange.Start.Year,
                    Month = dateRange.Start.Month
                },

            End = dateRange.End is null
                ? null
                : new PartialDate
                {
                    Year = dateRange.End.Year,
                    Month = dateRange.End.Month
                }
        };
    }
}