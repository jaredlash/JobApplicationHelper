using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JobApplicationHelper.Contracts.Experiences;
using JobApplicationHelper.Domain.Models;
using JobApplicationHelper.Services.Api;
using JobApplicationHelper.WindowService;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using System.ComponentModel.Design.Serialization;

namespace JobApplicationHelper.ViewModels;

public partial class ComparePendingExperienceViewModel : ViewModelBase
{
    private readonly ExperiencesApiClient experiencesApiClient;
    private readonly IWindowService windowService;

    public ComparePendingExperienceViewModel(ExperiencesApiClient experiencesApiClient, IWindowService windowService)
    {
        this.experiencesApiClient = experiencesApiClient;
        this.windowService = windowService;
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HeadingCurrent))]
    [NotifyPropertyChangedFor(nameof(HeadingPending))]
    [NotifyPropertyChangedFor(nameof(ShowTitle))]
    [NotifyPropertyChangedFor(nameof(ShowExperienceType))]
    [NotifyPropertyChangedFor(nameof(ShowOrganization))]
    [NotifyPropertyChangedFor(nameof(ShowDateRange))]
    [NotifyPropertyChangedFor(nameof(ShowSummary))]
    [NotifyPropertyChangedFor(nameof(SkillsCurrent))]
    [NotifyPropertyChangedFor(nameof(SkillsPending))]
    [NotifyPropertyChangedFor(nameof(ShowSkills))]
    [NotifyPropertyChangedFor(nameof(EvidenceCurrent))]
    [NotifyPropertyChangedFor(nameof(EvidencePending))]
    [NotifyPropertyChangedFor(nameof(ShowEvidence))]
    [NotifyPropertyChangedFor(nameof(ContextsCurrent))]
    [NotifyPropertyChangedFor(nameof(ContextsPending))]
    [NotifyPropertyChangedFor(nameof(ShowContext))]
    [NotifyPropertyChangedFor(nameof(LinksCurrent))]
    [NotifyPropertyChangedFor(nameof(LinksPending))]
    [NotifyPropertyChangedFor(nameof(ShowLinks))]
    [NotifyPropertyChangedFor(nameof(ShowNotes))]
    private PendingExperienceImport? pendingExperienceImport;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowTitle))]
    [NotifyPropertyChangedFor(nameof(ShowExperienceType))]
    [NotifyPropertyChangedFor(nameof(ShowOrganization))]
    [NotifyPropertyChangedFor(nameof(ShowDateRange))]
    [NotifyPropertyChangedFor(nameof(ShowSummary))]
    [NotifyPropertyChangedFor(nameof(ShowSkills))]
    [NotifyPropertyChangedFor(nameof(ShowEvidence))]
    [NotifyPropertyChangedFor(nameof(ShowContext))]
    [NotifyPropertyChangedFor(nameof(ShowLinks))]
    [NotifyPropertyChangedFor(nameof(ShowNotes))]
    private bool showOnlyDifferences = false;

    public string HeadingCurrent => PendingExperienceImport is not null
        ? BuildHeadingFromExperience(PendingExperienceImport.Current)
        : string.Empty;

    public string HeadingPending => PendingExperienceImport is not null
        ? BuildHeadingFromExperience(PendingExperienceImport.Pending)
        : string.Empty;

    private string BuildHeadingFromExperience(Experience experience) => string.IsNullOrWhiteSpace(experience.Organization)
            ? $"{experience.Type} - {experience.Title}"
            : $"{experience.Organization} - {experience.Title}";

    public bool ShowTitle => PendingExperienceImport is not null
        ? !ShowOnlyDifferences || PendingExperienceImport.Current.Title != PendingExperienceImport.Pending.Title
        : false;

    public bool ShowExperienceType => PendingExperienceImport is not null
        ? !ShowOnlyDifferences || PendingExperienceImport.Current.Type != PendingExperienceImport.Pending.Type
        : false;

    public bool ShowOrganization => PendingExperienceImport is not null
        ? !ShowOnlyDifferences || PendingExperienceImport.Current.Organization != PendingExperienceImport.Pending.Organization
        : false;

    public bool ShowDateRange => PendingExperienceImport is not null
        ? !ShowOnlyDifferences || (PendingExperienceImport.Current.DateRange?.ToString() ?? string.Empty) != (PendingExperienceImport.Pending.DateRange?.ToString() ?? string.Empty)
        : false;

    public bool ShowSummary => PendingExperienceImport is not null
        ? !ShowOnlyDifferences || PendingExperienceImport.Current.Summary != PendingExperienceImport.Pending.Summary
        : false;

    public string SkillsCurrent => PendingExperienceImport is not null
        ? string.Join(", ", PendingExperienceImport.Current.Skills)
        : string.Empty;

    public string SkillsPending => PendingExperienceImport is not null
        ? string.Join(", ", PendingExperienceImport.Pending.Skills)
        : string.Empty;

    public bool ShowSkills => PendingExperienceImport is not null
        ? !ShowOnlyDifferences || SkillsCurrent != SkillsPending
        : false;

    public string EvidenceCurrent => PendingExperienceImport is not null
        ? string.Join("\n", PendingExperienceImport.Current.Evidence)
        : string.Empty;

    public string EvidencePending => PendingExperienceImport is not null
        ? string.Join("\n", PendingExperienceImport.Pending.Evidence)
        : string.Empty;

    public bool ShowEvidence => PendingExperienceImport is not null
        ? !ShowOnlyDifferences || EvidenceCurrent != EvidencePending
        : false;

    public string ContextsCurrent => PendingExperienceImport is not null
        ? string.Join(", ", PendingExperienceImport.Current.Contexts)
        : string.Empty;

    public string ContextsPending => PendingExperienceImport is not null
        ? string.Join(", ", PendingExperienceImport.Pending.Contexts)
        : string.Empty;

    public bool ShowContext => PendingExperienceImport is not null
        ? !ShowOnlyDifferences || ContextsCurrent != ContextsPending
        : false;

    public string LinksCurrent => PendingExperienceImport is not null
        ? string.Join("\n", PendingExperienceImport.Current.Links)
        : string.Empty;

    public string LinksPending => PendingExperienceImport is not null
        ? string.Join("\n", PendingExperienceImport.Pending.Links)
        : string.Empty;

    public bool ShowLinks => PendingExperienceImport is not null
        ? !ShowOnlyDifferences || LinksCurrent != LinksPending
        : false;

    public bool ShowNotes => PendingExperienceImport is not null
        ? !ShowOnlyDifferences || PendingExperienceImport.Current.Notes != PendingExperienceImport.Pending.Notes
        : false;


    [RelayCommand]
    private async Task KeepExistingExperience(CancellationToken cancellationToken = default)
    {
        await Resolve(PendingExperienceImportResolution.Keep, cancellationToken);
    }

    [RelayCommand]
    private async Task ReplaceExperience(CancellationToken cancellationToken = default)
    {
        await Resolve(PendingExperienceImportResolution.Replace, cancellationToken);
    }

    private async Task Resolve(PendingExperienceImportResolution resolution, CancellationToken cancellationToken = default)
    {
        if (PendingExperienceImport is null)
            return;

        var experienceId = PendingExperienceImport.Current.Id;
        await experiencesApiClient.ResolvePendingExperienceImportAsync(experienceId, resolution, cancellationToken);

        await LoadNext(cancellationToken);
    }

    private async Task LoadNext(CancellationToken cancellationToken = default)
    {
        var next = await experiencesApiClient.GetNextPendingExperienceImportAsync(cancellationToken);
        if (next is null)
        {
            windowService.ShowAlertDialog("All done", "All pending experience imports have been resolved!");

            TryClose(true);
        }

        PendingExperienceImport = next;
    }

    [RelayCommand]
    private void Close()
    {
        TryClose(true);
    }
}
