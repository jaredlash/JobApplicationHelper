
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace JobApplicationHelper.ViewModels;

public partial class EditableSuggestionViewModel : ObservableObject
{
    private IReadOnlyList<string> completionSuggestions = [];

    public EditableSuggestionViewModel(string value, IReadOnlyList<string> completionSuggestions)
    {
        this.value = value;
        this.completionSuggestions = completionSuggestions;

        RefreshSuggestions();
    }

    [ObservableProperty]
    private string value = string.Empty;

    [ObservableProperty]
    private string? selectedSuggestion;

    [ObservableProperty]
    private IReadOnlyList<string> suggestions = [];

    public bool HasSuggestions => Suggestions.Count > 0;

    partial void OnValueChanged(string value)
    {
        SelectedSuggestion = null;
        RefreshSuggestions();
    }

    partial void OnSuggestionsChanged(IReadOnlyList<string> value)
    {
        OnPropertyChanged(nameof(HasSuggestions));
    }

    public void SetCompletionSuggestions(IReadOnlyList<string> suggestions)
    {
        completionSuggestions = suggestions;
        RefreshSuggestions();
    }


    public void RefreshSuggestions()
    {
        var text = Value.Trim();

        Suggestions = string.IsNullOrEmpty(text)
            ? []
            : completionSuggestions
                .Where(suggestion =>
                    suggestion.StartsWith(text, StringComparison.OrdinalIgnoreCase)
                    && !string.Equals(suggestion, text, StringComparison.OrdinalIgnoreCase))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
    }


    public bool AcceptSuggestion()
    {
        if (Suggestions.Count == 0)
        {
            return false;
        }

        Value = SelectedSuggestion ?? Suggestions[0];
        SelectedSuggestion = null;

        return true;
    }

    public bool MoveSuggestionSelection(int direction)
    {
        if (Suggestions.Count == 0)
        {
            return false;
        }

        var currentIndex = SelectedSuggestion is null
            ? -1
            : Suggestions
                .ToList()
                .FindIndex(suggestion => string.Equals(suggestion, SelectedSuggestion, StringComparison.OrdinalIgnoreCase));

        var nextIndex = direction switch
        {
            > 0 => currentIndex < 0
                ? 0
                : (currentIndex + 1) % Suggestions.Count,

            < 0 => currentIndex < 0
                ? Suggestions.Count - 1
                : (currentIndex - 1 + Suggestions.Count) % Suggestions.Count,

            _ => currentIndex
        };

        SelectedSuggestion = Suggestions[nextIndex];
        return true;
    }

    [RelayCommand]
    private void MoveSuggestionUp()
    {
        MoveSuggestionSelection(-1);
    }

    [RelayCommand]
    private void MoveSuggestionDown()
    {
        MoveSuggestionSelection(1);
    }

    [RelayCommand]
    private void AcceptSelectedSuggestion()
    {
        AcceptSuggestion();
    }
}


