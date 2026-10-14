using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JobApplicationHelper.Services.Experiences;
using JobApplicationHelper.WindowService;
using Microsoft.Extensions.DependencyInjection;
using System.IO;

namespace JobApplicationHelper.ViewModels;

public partial class ImportExperiencesViewModel : ViewModelBase
{
    private readonly ExperiencesImportService experiencesImportService;
    private readonly IFileChooserDialog fileChooserDialog;
    private readonly IViewModelFactory viewModelFactory;
    private readonly IWindowService windowService;

    public ImportExperiencesViewModel(
        ExperiencesImportService experiencesImportService,
        IFileChooserDialog fileChooserDialog,
        IViewModelFactory viewModelFactory,
        IWindowService windowService)
    {
        this.experiencesImportService = experiencesImportService;
        this.fileChooserDialog = fileChooserDialog;
        this.viewModelFactory = viewModelFactory;
        this.windowService = windowService;
        SelectedFile = string.Empty;
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DisplayFilename))]
    [NotifyCanExecuteChangedFor(nameof(ImportExperiencesCommand))]
    private string selectedFile;

    public string DisplayFilename => string.IsNullOrWhiteSpace(SelectedFile)
        ? "(select file...)"
        : Path.GetFileName(SelectedFile) ?? string.Empty;

    [RelayCommand]
    private async Task ChooseFile(CancellationToken cancellationToken = default)
    {
        var initialPath = Path.GetDirectoryName(SelectedFile);

        fileChooserDialog.InitialFolder = string.IsNullOrWhiteSpace(initialPath) ? string.Empty : initialPath;
        fileChooserDialog.Title = "Choose Yaml experiences file to import";
        var result = fileChooserDialog.ShowDialog();

        var newFile = fileChooserDialog.ResultFile;

        if (result == true && !string.IsNullOrWhiteSpace(newFile))
            SelectedFile = newFile;
    }


    [RelayCommand(CanExecute = nameof(CanImportExperiences))]
    private async Task ImportExperiences(CancellationToken cancellationToken = default)
    {
        var result = await experiencesImportService.ImportYamlFileAsync(SelectedFile, cancellationToken);
        var resultViewModel = viewModelFactory.Create<ExperiencesImportResultViewModel>();

        resultViewModel.Initialize(result);

        windowService.ShowDialog(resultViewModel);
    }
    public bool CanImportExperiences => !string.IsNullOrWhiteSpace(SelectedFile);
}
