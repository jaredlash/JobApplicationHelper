using JobApplicationHelper.Domain.Models;

namespace JobApplicationHelper.Application.Services;

public interface IExperienceBankImportService
{
    Task<ExperienceBank> ImportAsync(string filePath, CancellationToken cancellationToken = default);
}