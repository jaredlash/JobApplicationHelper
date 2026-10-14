using CommunityToolkit.Mvvm.ComponentModel;
using JobApplicationHelper.Domain.Models;

namespace JobApplicationHelper.ViewModels;

public partial class DraftWindowViewModel : ViewModelBase
{
    public DraftWindowViewModel(JobRequirementsViewModel jobRequirements, CoverLetterViewModel coverLetter)
    {
        JobRequirements = jobRequirements;
        CoverLetter = coverLetter;
    }

    public void Initialize(JobApplicationId jobApplicationId, string countryCode, string jobPosting)
    {
        CoverLetter.ApplicationId = jobApplicationId;
        CoverLetter.CountryCode = countryCode; // TODO: Remove this when persisting applications in the database
        JobRequirements.JobPosting = jobPosting;
        JobRequirements.ApplicationId = jobApplicationId;
    }

    [ObservableProperty]
    private int selectedTabIndex = 0;

    public JobRequirementsViewModel JobRequirements { get; }

    public CoverLetterViewModel CoverLetter { get; }

}
