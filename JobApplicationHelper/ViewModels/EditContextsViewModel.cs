
using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JobApplicationHelper.Services.Api;

namespace JobApplicationHelper.ViewModels;

public partial class EditContextsViewModel : ViewModelBase
{
    private readonly ExperiencesApiClient experiencesApiClient;

    public EditContextsViewModel(ExperiencesApiClient experiencesApiClient)
    {
        this.experiencesApiClient = experiencesApiClient;
    }

    public ObservableCollection<EditableSuggestionViewModel> ContextEntries { get; } = [];

    [ObservableProperty]
    private IReadOnlyList<string> completionSuggestions = [];

    public IReadOnlyList<string> Contexts =>
        ContextEntries
            .Select(entry => entry.Value.Trim())
            .Where(context => !string.IsNullOrWhiteSpace(context))
            .ToArray();

    public bool HasDuplicateContexts =>
        Contexts.Distinct(StringComparer.OrdinalIgnoreCase).Count()
        != Contexts.Count;

    public bool CanSave => !HasDuplicateContexts;

    public async Task InitializeAsync(IEnumerable<string> contexts, CancellationToken cancellationToken = default)
    {
        foreach (var entry in ContextEntries)
        {
            entry.PropertyChanged -= OnContextEntryPropertyChanged;
        }

        ContextEntries.Clear();

        foreach (var context in contexts)
        {
            AddContextEntry(context);
        }

        if (ContextEntries.Count == 0)
        {
            AddContextEntry();
        }

        try
        {
            CompletionSuggestions = await experiencesApiClient.GetContextSuggestions(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch
        {
            // Suggestions are optional; arbitrary contexts remain valid.
            CompletionSuggestions = [];
        }

        foreach (var entry in ContextEntries)
        {
            entry.SetCompletionSuggestions(CompletionSuggestions);
        }
    }

    [RelayCommand]
    private void AddContext()
    {
        AddContextEntry();
    }

    [RelayCommand]
    private void RemoveContext(EditableSuggestionViewModel? entry)
    {
        if (entry is null || !ContextEntries.Remove(entry))
        {
            return;
        }

        entry.PropertyChanged -= OnContextEntryPropertyChanged;
        OnContextsChanged();
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

    private void AddContextEntry(string value = "")
    {
        var entry = new EditableSuggestionViewModel(value, CompletionSuggestions);

        entry.PropertyChanged += OnContextEntryPropertyChanged;
        ContextEntries.Add(entry);

        OnContextsChanged();
    }

    private void OnContextEntryPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(EditableSuggestionViewModel.Value))
        {
            OnContextsChanged();
        }
    }

    private void OnContextsChanged()
    {
        OnPropertyChanged(nameof(Contexts));
        OnPropertyChanged(nameof(HasDuplicateContexts));
        OnPropertyChanged(nameof(CanSave));

        SaveCommand.NotifyCanExecuteChanged();
    }

    partial void OnCompletionSuggestionsChanged(IReadOnlyList<string> value)
    {
        foreach (var entry in ContextEntries)
        {
            entry.SetCompletionSuggestions(value);
        }
    }
}
