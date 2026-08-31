using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;

namespace JobApplicationHelper.ViewModels;

public partial class DraftWindowViewModel : ViewModelBase
{

    public DraftWindowViewModel(JobRequirementsViewModel jobRequirements, CoverLetterViewModel coverLetter)
    {
        JobRequirements = jobRequirements;
        CoverLetter = coverLetter;
    }

    [ObservableProperty]
    private int selectedTabIndex = 0;

    public JobRequirementsViewModel JobRequirements { get; }

    public CoverLetterViewModel CoverLetter { get; }

}
