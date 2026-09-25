using JobApplicationHelper.Application.Configuration;
using JobApplicationHelper.Application.Services;
using JobApplicationHelper.Domain.Models;
using JobApplicationHelper.Infrastructure.Services;

namespace JobApplicationHelper.Services;

public class TempYamlExperienceBankService : IExperienceBankService
{
    private readonly IExperienceBankImportService experienceBankImportService;
    private readonly ApplicationDocumentOptions documentOptions;
    private ExperienceBank? experienceBank = null;

    public TempYamlExperienceBankService(IExperienceBankImportService experienceBankImportService, ApplicationDocumentOptions documentOptions)
    {
        this.experienceBankImportService = experienceBankImportService;
        this.documentOptions = documentOptions;
    }

    public Task AddAsync(Experience experience, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<IReadOnlyList<Experience>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        experienceBank ??= await LoadExperienceBank();
        
        return experienceBank.Experiences.AsReadOnly();
    }

    public async Task<Experience?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var experiences = await GetAllAsync(cancellationToken);

        return experiences.FirstOrDefault(e => e.Id == id);
    }

    public Task UpdateAsync(Experience experience, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    private async Task<ExperienceBank> LoadExperienceBank()
    {
        string experienceBankPath = Path.Combine(documentOptions.TemplateBasePath, "experience.yaml");
        return await experienceBankImportService.ImportAsync(experienceBankPath);
    }
}
