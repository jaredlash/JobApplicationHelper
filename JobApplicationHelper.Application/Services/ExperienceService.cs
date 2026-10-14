using JobApplicationHelper.Application.Repositories;
using JobApplicationHelper.Domain.Models;

namespace JobApplicationHelper.Application.Services;

public sealed class ExperienceService : IExperienceService
{
    private readonly IExperienceRepository repository;

    public ExperienceService(IExperienceRepository repository)
    {
        this.repository = repository;
    }

    public Task<IReadOnlyList<Experience>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return repository.GetAllAsync(cancellationToken);
    }

    public Task<Experience?> GetAsync(ExperienceId id, CancellationToken cancellationToken = default)
    {
        return repository.GetAsync(id, cancellationToken);
    }

    public Task AddOrReplaceAsync(Experience experience, CancellationToken cancellationToken = default)
    {
        return repository.AddOrReplaceAsync(experience, cancellationToken: cancellationToken);
    }

    public Task DeleteAsync(ExperienceId id, CancellationToken cancellationToken = default)
    {
        return repository.DeleteAsync(id, cancellationToken);
    }

    public Task<IReadOnlyList<string>> GetContextSuggestions(CancellationToken cancellationToken = default)
    {
        return repository.GetContextSuggestions(cancellationToken);
    }

    public Task<IReadOnlyList<string>> GetSkillSuggestions(CancellationToken cancellationToken = default)
    {
        return repository.GetSkillSuggestions(cancellationToken);
    }
}