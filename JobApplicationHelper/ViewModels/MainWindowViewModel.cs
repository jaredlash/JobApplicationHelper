using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JobApplicationHelper.Application.Services;
using JobApplicationHelper.Domain.Models;
using JobApplicationHelper.Services.Api;
using JobApplicationHelper.WindowService;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace JobApplicationHelper.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        private readonly IApplicationMaterialsService applicationMaterialsService;
        private readonly JobApplicationsApiClient jobApplicationsApiClient;
        private readonly IApiHealthService apiHealthService;
        private readonly IFolderLauncher folderLauncher;
        private readonly IWindowService windowService;
        private readonly IServiceProvider serviceProvider;

        public MainWindowViewModel(
            IApplicationMaterialsService applicationMaterialsService,
            JobApplicationsApiClient jobApplicationsApiClient,
            LocationsApiClient locationsApiClient,
            IApiHealthService apiHealthService,
            IFolderLauncher folderLauncher,
            IWindowService windowService,
            IServiceProvider serviceProvider)
        {
            
            this.applicationMaterialsService = applicationMaterialsService;
            this.jobApplicationsApiClient = jobApplicationsApiClient;
            this.apiHealthService = apiHealthService;
            this.folderLauncher = folderLauncher;
            this.windowService = windowService;
            this.serviceProvider = serviceProvider;
            Locations = [];
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

        partial void OnIncludeCoverLetterChanged(bool value)
        {
            ValidateProperty(JobPosting, nameof(JobPosting));
        }

        [ObservableProperty]
        [NotifyDataErrorInfo]
        [CustomValidation(typeof(MainWindowViewModel), nameof(ValidateJobPosting))]
        private string jobPosting = string.Empty;


        [ObservableProperty]
        private bool openNewFolder;

        [ObservableProperty]
        private string statusMessage = string.Empty;

        [RelayCommand]
        public async Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            await apiHealthService.WaitUntilReadyAsync(cancellationToken);

            var locations = await serviceProvider.GetRequiredService<LocationsApiClient>().GetAllAsync(cancellationToken);
            Locations = new BindingList<Location>([.. locations]);

            ResetForm();
        }


        [RelayCommand]
        private async Task CreateApplication()
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
                var applicationId = await jobApplicationsApiClient.CreateJobApplicationAsync(applicationFile);
                string newFolder = await jobApplicationsApiClient.GetApplicationFolderAsync(applicationId);
                StatusMessage = $"Application folder for {CompanyName} - {PositionTitle} created successfully.";

                if (OpenNewFolder)
                {
                    folderLauncher.OpenFolder(newFolder);
                }

                if (IncludeCoverLetter)
                {
                    OpenCoverletterDraftWindow(applicationId, JobPosting);
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

        private void OpenCoverletterDraftWindow(JobApplicationId applicationId, string jobPosting)
        {
            var draftWindowViewModel = serviceProvider.GetService<DraftWindowViewModel>() ?? throw new InvalidOperationException("DraftWindowViewModel not registered in DI container.");

            draftWindowViewModel.CoverLetter.ApplicationId = applicationId;
            draftWindowViewModel.CoverLetter.CountryCode = SelectedLocation.CountryCode; // TODO: Remove this when persisting the applications in a database
            draftWindowViewModel.JobRequirements.JobPosting = jobPosting;

            windowService.ShowWindow(draftWindowViewModel);
        }
        public static ValidationResult? ValidateJobPosting(string? jobPosting, ValidationContext context)
        {
            var viewModel = (MainWindowViewModel)context.ObjectInstance;

            if (!viewModel.IncludeCoverLetter)
                return ValidationResult.Success;

            if (!string.IsNullOrWhiteSpace(jobPosting))
                return ValidationResult.Success;

            return new ValidationResult("Job posting is required when generating a cover letter.", [nameof(JobPosting)]);
        }
    }
}
