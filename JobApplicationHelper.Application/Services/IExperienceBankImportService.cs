using JobApplicationHelper.Domain.Models;

namespace JobApplicationHelper.Application.Services;

public interface IExperienceBankImportService
{
    Task<ExperienceBankImportModel> ImportAsync(Stream stream, string sourceName, CancellationToken cancellationToken = default);
    Task<ExperienceBankImportModel> ImportAsync(string filePath, CancellationToken cancellationToken = default);
}