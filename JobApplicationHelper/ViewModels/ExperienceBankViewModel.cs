using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JobApplicationHelper.Models;
using JobApplicationHelper.Services;
using JobApplicationHelper.WindowService;
using System.Collections.ObjectModel;

namespace JobApplicationHelper.ViewModels;

public partial class ExperienceBankViewModel : ViewModelBase
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IExperienceBankService _experienceBankService;
    private readonly IWindowService _windowService;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectedExperience))]
    [NotifyCanExecuteChangedFor(nameof(EditCommand))]
    private int _selectedIndex = -1;

    public ObservableCollection<Experience> Experiences { get; } = [];

    public Experience? SelectedExperience =>
        SelectedIndex >= 0 && SelectedIndex < Experiences.Count
            ? Experiences[SelectedIndex]
            : null;

    public ExperienceBankViewModel(
        IServiceProvider serviceProvider,
        IExperienceBankService experienceBankService,
        IWindowService windowService)
    {
        _serviceProvider = serviceProvider;
        _experienceBankService = experienceBankService;
        _windowService = windowService;
    }

    public async Task InitializeAsync(
        CancellationToken cancellationToken = default)
    {
        await ReloadAsync(cancellationToken);
    }

    private async Task ReloadAsync(
        CancellationToken cancellationToken = default)
    {
        var experiences = await _experienceBankService.GetAllAsync(
            cancellationToken);

        Experiences.Clear();

        foreach (var experience in experiences)
        {
            Experiences.Add(experience);
        }

        if (Experiences.Count == 0)
        {
            SelectedIndex = -1;
        }
        else if (SelectedIndex >= Experiences.Count)
        {
            SelectedIndex = Experiences.Count - 1;
        }

        OnPropertyChanged(nameof(SelectedExperience));
    }

    [RelayCommand]
    private void Add()
    {
        // TODO:
        // Create the ExperienceEditViewModel using _serviceProvider.
        //
        // Show the dialog using _windowService.
        //
        // If the dialog returns true, reload the experience list.
    }

    [RelayCommand(CanExecute = nameof(CanEdit))]
    private void Edit()
    {
        var experience = SelectedExperience;

        if (experience is null)
        {
            return;
        }

        // TODO:
        // Create the ExperienceEditViewModel using _serviceProvider.
        // Pass 'experience' to it for editing.
        //
        // Show the dialog using _windowService.
        //
        // If the dialog returns true, reload the experience list.
    }

    private bool CanEdit()
    {
        return SelectedExperience is not null;
    }

    [RelayCommand]
    private void Remove()
    {
        // TODO:
        // Implement removal later.
    }

    [RelayCommand]
    private void Import()
    {
        // TODO:
        // Implement YAML import later.
    }

    [RelayCommand]
    private void Export()
    {
        // TODO:
        // Implement YAML export later.
    }
}