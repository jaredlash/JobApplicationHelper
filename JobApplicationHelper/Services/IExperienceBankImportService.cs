using JobApplicationHelper.Models;

namespace JobApplicationHelper.Services;

public interface IExperienceBankImportService
{
    Task<ExperienceBank> ImportAsync(string filePath, CancellationToken cancellationToken = default);
}