using CommunityToolkit.Mvvm.ComponentModel;

namespace JobApplicationHelper.ViewModels;

public partial class EditableEvidence : ObservableObject
{
    [ObservableProperty]
    private string value = string.Empty;

    public EditableEvidence()
    {
    }

    public EditableEvidence(string value)
    {
        Value = value;
    }
}