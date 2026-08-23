using CommunityToolkit.Mvvm.ComponentModel;

namespace JobApplicationHelper.ViewModels;

public class ViewModelBase : ObservableValidator
{
    public Action<bool?>? CloseAction { get; set; } = null;


    public void TryClose()
    {
        CloseAction?.Invoke(true);
    }

    public void TryClose(bool? result)
    {
        CloseAction?.Invoke(result);
    }

    public virtual void OnClose()
    {

    }
}