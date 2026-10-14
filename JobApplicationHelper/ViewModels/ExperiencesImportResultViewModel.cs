using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JobApplicationHelper.Contracts.Experiences;
using JobApplicationHelper.Services.Api;
using JobApplicationHelper.WindowService;
using Microsoft.Extensions.DependencyInjection;

namespace JobApplicationHelper.ViewModels;

public partial class ExperiencesImportResultViewModel : ViewModelBase
{
    private readonly ExperiencesApiClient experiencesApiClient;
    private readonly IServiceProvider serviceProvider;
    private readonly IWindowService windowService;

    public ExperiencesImportResultViewModel(ExperiencesApiClient experiencesApiClient, IServiceProvider serviceProvider, IWindowService windowService)
    {
        this.experiencesApiClient = experiencesApiClient;
        this.serviceProvider = serviceProvider;
        this.windowService = windowService;
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TotalCountDescription))]
    [NotifyPropertyChangedFor(nameof(ShowResolveOption))]
    private ImportExperiencesResponse result = new(0, 0, 0);

    public string TotalCountDescription => $"{Result.AddedCount + Result.SkippedCount + Result.PendingCount} total processed";

    public bool ShowResolveOption => Result.PendingCount > 0;

    [RelayCommand]
    private async Task ResolvePendingImports(CancellationToken cancellationToken = default)
    {
        var resolvePendingViewModel = serviceProvider.GetRequiredService<ComparePendingExperienceViewModel>();
        var firstPendingImport = await experiencesApiClient.GetNextPendingExperienceImportAsync(cancellationToken);

        if (firstPendingImport == null)
        {
            windowService.ShowAlertDialog("All imports resolved", "All pending imports have been automatically resolved.");
        }
        else
        {
            resolvePendingViewModel.PendingExperienceImport = firstPendingImport;
            windowService.ShowDialog(resolvePendingViewModel);
        }

        TryClose(true);
    }

    [RelayCommand]
    private void Close()
    {
        TryClose(true);
    }
}
