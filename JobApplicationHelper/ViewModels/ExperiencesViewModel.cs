using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JobApplicationHelper.Domain.Models;
using JobApplicationHelper.Services.Api;
using JobApplicationHelper.WindowService;
using System.Collections.ObjectModel;

namespace JobApplicationHelper.ViewModels;

public partial class ExperiencesViewModel : ViewModelBase, ISingleWindowViewModel
{
    private readonly IViewModelFactory viewModelFactory;
    private readonly ExperiencesApiClient experiencesApiClient;
    private readonly IWindowService _windowService;

    public Experience? SelectedExperience =>
        SelectedIndex >= 0 && SelectedIndex < Experiences.Count
            ? Experiences[SelectedIndex]
            : null;

    public ExperiencesViewModel(
        IViewModelFactory viewModelFactory,
        ExperiencesApiClient experiencesApiClient,
        IWindowService windowService)
    {
        this.viewModelFactory = viewModelFactory;
        this.experiencesApiClient = experiencesApiClient;
        _windowService = windowService;
    }


    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectedExperience))]
    [NotifyPropertyChangedFor(nameof(Skills))]
    [NotifyPropertyChangedFor(nameof(DateRange))]
    [NotifyPropertyChangedFor(nameof(Links))]
    [NotifyCanExecuteChangedFor(nameof(EditExperienceCommand))]
    [NotifyCanExecuteChangedFor(nameof(RemoveExperienceCommand))]
    private int selectedIndex = -1;

    public string Skills => string.Join(", ", SelectedExperience?.Skills ?? []);
    public string DateRange => SelectedExperience?.DateRange?.ToString() ?? string.Empty;
    public string Links => string.Join(", ", SelectedExperience?.Links ?? []);


    public ObservableCollection<Experience> Experiences { get; } = [];

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await ReloadAsync(cancellationToken: cancellationToken);
    }

    private async Task ReloadAsync(ExperienceId? selectedExperienceId = null, CancellationToken cancellationToken = default)
    {
        var experiences = await experiencesApiClient.GetAllAsync(cancellationToken);

        Experiences.Clear();

        foreach (var experience in experiences)
        {
            Experiences.Add(experience);
        }

        if (Experiences.Count == 0)
        {
            SelectedIndex = -1;
            return;
        }

        if (selectedExperienceId is not null)
        {
            
            var index = Experiences
                .Select((exp, i) => new { exp, i })
                .FirstOrDefault(x => x.exp.Id == selectedExperienceId)?.i ?? -1;

            SelectedIndex = index;
            return;
        }

        // No experience was specified for selection.
        SelectedIndex = -1;
    }

    [RelayCommand]
    private async Task AddExperienceAsync()
    {
        var viewModel = viewModelFactory.Create<ExperienceEditViewModel>();

        viewModel.InitializeForAdd();

        var result = _windowService.ShowDialog(viewModel);

        if (result == true)
        {
            await ReloadAsync();
        }
    }

    [RelayCommand(CanExecute = nameof(CanEditExperience))]
    private async Task EditExperienceAsync()
    {
        var experience = SelectedExperience;

        if (experience is null)
        {
            return;
        }

        var experienceId = experience.Id;

        var viewModel = viewModelFactory.Create<ExperienceEditViewModel>();

        viewModel.InitializeForEdit(experience);

        var result = _windowService.ShowDialog(viewModel);

        if (result == true)
        {
            await ReloadAsync(experienceId);
        }
    }
    private bool CanEditExperience() => SelectedExperience is not null;

    [RelayCommand(CanExecute = nameof(CanRemoveExperience))]
    private async Task RemoveExperienceAsync(CancellationToken cancellationToken = default)
    {
        if (SelectedExperience is null) return;

        await experiencesApiClient.DeleteAsync(SelectedExperience.Id.Value, cancellationToken);

        await ReloadAsync(null, cancellationToken);
    }
    private bool CanRemoveExperience() => SelectedExperience is not null;

    [RelayCommand]
    private async Task ImportExperience(CancellationToken cancellationToken = default)
    {
        var pendingCount = await experiencesApiClient.GetPendingExperienceImportCountAsync(cancellationToken);
        if (pendingCount > 0)
        {
            var response = _windowService.ShowConfirmDialog("Pending imports", "There are pending experience imports.  Do you want to resolve these first?");
            if (response == true)
            {
                var resolvePendingViewModel = viewModelFactory.Create<ComparePendingExperienceViewModel>();
                var firstPendingImport = await experiencesApiClient.GetNextPendingExperienceImportAsync(cancellationToken);

                if (firstPendingImport == null)
                {
                    _windowService.ShowAlertDialog("All imports resolved", "All pending imports have been automatically resolved.");
                }
                else
                {
                    resolvePendingViewModel.PendingExperienceImport = firstPendingImport;
                    _windowService.ShowDialog(resolvePendingViewModel);
                }
            }
        }


        var viewModel = viewModelFactory.Create<ImportExperiencesViewModel>();
        var result = _windowService.ShowDialog(viewModel);
        
        await ReloadAsync(SelectedExperience?.Id, cancellationToken);
    }

    [RelayCommand]
    private void ExportExperience()
    {
        // TODO: Implement YAML export later.
    }

    [RelayCommand]
    private void Close()
    {
        TryClose();
    }
}