using JobApplicationHelper.Domain.Models;
using JobApplicationHelper.ViewModels;
using JobApplicationHelper.Views;
using JobApplicationHelper.WindowService;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace JobApplicationHelper.DependencyInjection;

public static class WindowServiceCollectionExtensions
{
    public static IServiceCollection AddWindowServices(this IServiceCollection services)
    {

        services.AddSingleton<MainWindow>();
        services.AddTransient<MainWindowViewModel>();

        services.AddDraftServices();

        services.AddViewWithViewModel<VerificationResultDialogViewModel, VerificationResultDialog>();
        services.AddViewWithViewModel<ExperiencesViewModel, ExperiencesView>();
        services.AddViewWithViewModel<ImportExperiencesViewModel, ImportExperiencesView>();
        services.AddViewWithViewModel<ExperiencesImportResultViewModel, ExperiencesImportResultView>();
        services.AddViewWithViewModel<ComparePendingExperienceViewModel, ComparePendingExperienceView>();
        services.AddViewWithViewModel<ConfirmDialogViewModel, ConfirmDialogView>();
        services.AddViewWithViewModel<AlertDialogViewModel, AlertDialogView>();
        services.AddViewWithViewModel<ExperienceEditViewModel, ExperienceEditView>();
        services.AddViewWithViewModel<EditSkillsViewModel, EditSkillsView>();
        services.AddViewWithViewModel<EditContextsViewModel, EditContextsView>();

        services.AddSingleton<IOpenWindowRegistry, OpenWindowRegistry>();
        services.AddSingleton<IViewModelFactory, ViewModelFactory>();
        services.AddSingleton<IWindowService, WindowService.WindowService>();

        services.AddTransient<IFolderLauncher, FolderLauncher>();
        services.AddTransient<IFileChooserDialog, FileChooserDialog>();

        return services;
    }


    public static IServiceCollection AddDraftServices(this IServiceCollection services)
    {
        services.AddTransient<JobRequirementsViewModel>();
        services.AddTransient<CoverLetterViewModel>();

        services.AddTransient<DraftWindowViewModel>(sp =>
        {
            var navigation = new DraftNavigation();
            var draftParameters = new CoverLetterDraftParameters();

            var jobRequirements = ActivatorUtilities.CreateInstance<JobRequirementsViewModel>(sp, navigation, draftParameters);

            var coverLetter = ActivatorUtilities.CreateInstance<CoverLetterViewModel>(sp, navigation, draftParameters);

            var viewModel = new DraftWindowViewModel(jobRequirements, coverLetter);

            navigation.Initialize(viewModel);

            return viewModel;
        });

        services.AddKeyedTransient<Window, DraftWindow>(typeof(DraftWindowViewModel));

        return services;
    }
}
