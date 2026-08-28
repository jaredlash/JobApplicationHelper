using JobApplicationHelper.Models;

namespace JobApplicationHelper.Services;

public class TempYamlExperienceBankService : IExperienceBankService
{
    private readonly FileService fileService;
    private ExperienceBank? experienceBank = null;

    public TempYamlExperienceBankService(FileService fileService)
    {
        this.fileService = fileService;
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
        experienceBank ??= await fileService.LoadExperienceBank();
        
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
}
