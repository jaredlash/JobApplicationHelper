using JobApplicationHelper.Domain.Models;

namespace JobApplicationHelper.Application.Services;

public interface IExperienceBankImportService
{
    Task<ExperienceBankImportModel> ImportAsync(string filePath, CancellationToken cancellationToken = default);
}