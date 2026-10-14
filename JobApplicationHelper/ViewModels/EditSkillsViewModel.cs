using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JobApplicationHelper.Services.Api;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace JobApplicationHelper.ViewModels;

public partial class EditSkillsViewModel : ViewModelBase
{
    private readonly ExperiencesApiClient experiencesApiClient;

    public EditSkillsViewModel(ExperiencesApiClient experiencesApiClient)
    {
        this.experiencesApiClient = experiencesApiClient;
    }

    public ObservableCollection<EditableSuggestionViewModel> SkillEntries { get; } = [];

    [ObservableProperty]
    private IReadOnlyList<string> completionSuggestions = [];

    public IReadOnlyList<string> Skills =>
        SkillEntries
            .Select(entry => entry.Value.Trim())
            .Where(skill => !string.IsNullOrWhiteSpace(skill))
            .ToArray();

    public bool HasDuplicateSkills =>
        Skills.Distinct(StringComparer.OrdinalIgnoreCase).Count() != Skills.Count;

    public bool CanSave =>
        !HasDuplicateSkills;

    public async Task InitializeAsync(
        IEnumerable<string> skills,
        CancellationToken cancellationToken = default)
    {
        SkillEntries.Clear();

        foreach (var skill in skills)
        {
            AddSkillEntry(skill);
        }

        // Provide an empty entry when there are no existing skills.
        if (SkillEntries.Count == 0)
        {
            AddSkillEntry();
        }

        try
        {
            CompletionSuggestions = await experiencesApiClient.GetSkillSuggestions(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch
        {
            // Suggestions are optional. The user can still enter arbitrary skills.
            CompletionSuggestions = [];
        }

        foreach (var entry in SkillEntries)
        {
            entry.RefreshSuggestions();
        }
    }

    [RelayCommand]
    private void AddSkill()
    {
        AddSkillEntry();
    }

    [RelayCommand]
    private void RemoveSkill(EditableSuggestionViewModel? entry)
    {
        if (entry is null)
        {
            return;
        }

        SkillEntries.Remove(entry);
        entry.PropertyChanged -= OnSkillEntryPropertyChanged;

        OnSkillsChanged();
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private void Save()
    {
        TryClose(true);
    }

    [RelayCommand]
    private void Cancel()
    {
        TryClose(false);
    }

    private void AddSkillEntry(string value = "")
    {
        var entry = new EditableSuggestionViewModel(value, CompletionSuggestions);

        entry.PropertyChanged += OnSkillEntryPropertyChanged;
        SkillEntries.Add(entry);

        OnSkillsChanged();
    }

    private void OnSkillEntryPropertyChanged(
        object? sender,
        PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(EditableSuggestionViewModel.Value))
        {
            OnSkillsChanged();
        }
    }

    private void OnSkillsChanged()
    {
        OnPropertyChanged(nameof(Skills));
        OnPropertyChanged(nameof(HasDuplicateSkills));
        OnPropertyChanged(nameof(CanSave));

        SaveCommand.NotifyCanExecuteChanged();
    }

    partial void OnCompletionSuggestionsChanged(
        IReadOnlyList<string> value)
    {
        foreach (var entry in SkillEntries)
        {
            entry.SetCompletionSuggestions(value);
        }
    }
}


