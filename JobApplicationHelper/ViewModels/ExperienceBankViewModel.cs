using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JobApplicationHelper.Models;
using JobApplicationHelper.Services;
using JobApplicationHelper.WindowService;
using Microsoft.Extensions.DependencyInjection;
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
        await ReloadAsync(cancellationToken: cancellationToken);
    }

    private async Task ReloadAsync(
    string? selectedExperienceId = null,
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
            return;
        }

        if (!string.IsNullOrWhiteSpace(selectedExperienceId))
        {
            var index = -1;

            for (var i = 0; i < Experiences.Count; i++)
            {
                if (string.Equals(
                    Experiences[i].Id,
                    selectedExperienceId,
                    StringComparison.OrdinalIgnoreCase))
                {
                    index = i;
                    break;
                }
            }

            SelectedIndex = index;
            return;
        }

        // No experience was specified for selection.
        SelectedIndex = -1;
    }

    [RelayCommand]
    private async Task AddAsync()
    {
        var viewModel =
            _serviceProvider.GetRequiredService<ExperienceEditViewModel>();

        viewModel.InitializeForAdd();

        var result = _windowService.ShowDialog(viewModel);

        if (result == true)
        {
            await ReloadAsync();
        }
    }

    [RelayCommand(CanExecute = nameof(CanEdit))]
    private async Task EditAsync()
    {
        var experience = SelectedExperience;

        if (experience is null)
        {
            return;
        }

        var experienceId = experience.Id;

        var viewModel =
            _serviceProvider.GetRequiredService<ExperienceEditViewModel>();

        viewModel.InitializeForEdit(experience);

        var result = _windowService.ShowDialog(viewModel);

        if (result == true)
        {
            await ReloadAsync(experienceId);
        }
    }

    private bool CanEdit()
    {
        return SelectedExperience is not null;
    }

    [RelayCommand]
    private void Remove()
    {
        // TODO: Implement removal later.
    }

    [RelayCommand]
    private void Import()
    {
        // TODO: Implement YAML import later.
    }

    [RelayCommand]
    private void Export()
    {
        // TODO: Implement YAML export later.
    }
}