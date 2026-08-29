using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JobApplicationHelper.Models;
using JobApplicationHelper.Services;
using JobApplicationHelper.WindowService;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace JobApplicationHelper.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {

        private readonly FileService _fileService;
        private readonly IWindowService windowService;
        private readonly IServiceProvider serviceProvider;

        public MainWindowViewModel(LocationService locationService, FileService fileService, IWindowService windowService, IServiceProvider serviceProvider)
        {
            Locations = new BindingList<Location>([.. locationService.GetLocations()]);
            this._fileService = fileService;
            this.windowService = windowService;
            this.serviceProvider = serviceProvider;
            ResetForm();
        }

        [ObservableProperty]
        private BindingList<Location> locations;

        [ObservableProperty]
        private Location selectedLocation = default!;

        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Required(ErrorMessage = "Company name is required.")]
        private string companyName = string.Empty;

        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Required(ErrorMessage = "Position title is required.")]
        private string positionTitle = string.Empty;

        [ObservableProperty]
        private string city = string.Empty;

        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Required(ErrorMessage = "URL is required.")]
        private string url = string.Empty;

        [ObservableProperty]
        private bool includeCoverLetter;

        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Required(ErrorMessage = "Job posting is required.")]
        private string jobPosting = string.Empty;

        [ObservableProperty]
        private bool openNewFolder;

        [ObservableProperty]
        private string statusMessage = string.Empty;

        [RelayCommand]
        private void CreateApplication()
        {
            ValidateAllProperties();
            if (HasErrors)
            {
                StatusMessage = "Please fix validation errors before creating the application.";
                return;
            }

            var applicationFile = new ApplicationFile(
                CountryCode: SelectedLocation.CountryCode,
                IncludeCoverLetter: IncludeCoverLetter,
                CompanyName: CompanyName,
                PositionTitle: PositionTitle,
                URL: Url,
                City: string.IsNullOrWhiteSpace(City) ? null : City,
                JobPosting: JobPosting
            );
            try
            {
                string newFolder = _fileService.CreateApplicationDocuments(applicationFile);
                StatusMessage = $"Application folder for {CompanyName} - {PositionTitle} created successfully.";

                if (OpenNewFolder)
                {
                    FileService.OpenFolder(newFolder);
                }

                if (IncludeCoverLetter)
                {
                    OpenCoverletterDraftWindow(newFolder, JobPosting);
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error creating application: {ex.Message}";
            }
        }

        [RelayCommand]
        private void ResetForm()
        {
            CompanyName = string.Empty;
            PositionTitle = string.Empty;
            City = string.Empty;
            Url = string.Empty;
            IncludeCoverLetter = true;
            OpenNewFolder = true;
            StatusMessage = string.Empty;
            SelectedLocation = Locations.FirstOrDefault()!;
            ClearErrors();
        }

        private void OpenCoverletterDraftWindow(string outputFolder, string jobPosting)
        {
            var draftWindowViewModel = serviceProvider.GetService<DraftWindowViewModel>() ?? throw new InvalidOperationException("DraftWindowViewModel not registered in DI container.");

            draftWindowViewModel.CoverLetter.CvText = _fileService.GetCVText(SelectedLocation.CountryCode);
            draftWindowViewModel.CoverLetter.OutputFolder = outputFolder;
            draftWindowViewModel.JobRequirements.JobPosting = jobPosting;

            windowService.ShowWindow(draftWindowViewModel);
        }
    }
}
