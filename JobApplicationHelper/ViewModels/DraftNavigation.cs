namespace JobApplicationHelper.ViewModels;

public class DraftNavigation : IDraftNavigation
{
    private DraftWindowViewModel? _parent = null;

    public void Initialize(DraftWindowViewModel? parent)
    {
        _parent = parent;
    }  

    public void GoToTab(DraftTab tab)
    {
        _parent?.SelectedTabIndex = (int)tab;
    }
}
