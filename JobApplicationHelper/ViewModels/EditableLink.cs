using CommunityToolkit.Mvvm.ComponentModel;

namespace JobApplicationHelper.ViewModels;

public partial class EditableLink : ObservableObject
{
    [ObservableProperty]
    private string value = string.Empty;

    public EditableLink()
    {
    }

    public EditableLink(string value)
    {
        Value = value;
    }
}