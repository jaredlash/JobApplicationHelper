using JobApplicationHelper.Application.Configuration;
using JobApplicationHelper.Application.Services;
using JobApplicationHelper.Domain.Models;
using Microsoft.Extensions.Options;

namespace JobApplicationHelper.Infrastructure.Services;

public class TempYamlExperienceBankService : IExperienceService
{
    private readonly IExperienceBankImportService experienceBankImportService;
    private readonly ApplicationDocumentOptions documentOptions;
    private ExperienceBankImportModel? yamlExperienceBank = null;

    private List<Experience> experiences = [];

    public TempYamlExperienceBankService(IExperienceBankImportService experienceBankImportService, IOptions<ApplicationDocumentOptions> documentOptions)
    {
        this.experienceBankImportService = experienceBankImportService;
        this.documentOptions = documentOptions.Value;
    }

    public Task AddOrReplaceAsync(Experience experience, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task DeleteAsync(ExperienceId id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<IReadOnlyList<Experience>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        if (yamlExperienceBank is null)
        {
            yamlExperienceBank = await LoadExperienceBank();
            experiences = yamlExperienceBank.Experiences.Select(ye => ToExperience(ye)).ToList();
        }

        return experiences.AsReadOnly();
    }

    public async Task<Experience?> GetAsync(ExperienceId id, CancellationToken cancellationToken = default)
    {
        var experiences = await GetAllAsync(cancellationToken);

        return experiences.FirstOrDefault(e => e.Id == id);
    }

    private async Task<ExperienceBankImportModel> LoadExperienceBank()
    {
        string experienceBankPath = Path.Combine(documentOptions.TemplateBasePath, "experience.yaml");
        return await experienceBankImportService.ImportAsync(experienceBankPath);
    }

    private Experience ToExperience(ExperienceImportModel yamlExperience)
    {
        return new Experience
        {
            Id = new ExperienceId(Guid.NewGuid()),
            Title = yamlExperience.Title,
            Type = yamlExperience.Type,
            Organization = yamlExperience.Organization,
            DateRange = yamlExperience.DateRange,
            Summary = yamlExperience.Summary,
            Skills = yamlExperience.Skills ?? new List<string>(),
            Evidence = yamlExperience.Evidence ?? new List<string>(),
            Contexts = yamlExperience.Contexts ?? new List<string>(),
            Links = yamlExperience.Links ?? new List<string>(),
            Notes = yamlExperience.Notes
        };
    }
}
