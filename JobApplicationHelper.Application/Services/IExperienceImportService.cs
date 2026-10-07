using JobApplicationHelper.Domain.Models;

namespace JobApplicationHelper.Application.Services;

public interface IExperienceImportService
{
    Task<ExperienceImportResult> ImportAsync(ExperienceBankImportModel import, CancellationToken cancellationToken = default);
}