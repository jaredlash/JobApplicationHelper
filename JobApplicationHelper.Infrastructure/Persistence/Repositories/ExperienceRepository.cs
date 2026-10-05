using JobApplicationHelper.Application.Repositories;
using JobApplicationHelper.Domain.Models;
using JobApplicationHelper.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace JobApplicationHelper.Infrastructure.Persistence.Repositories;

public sealed class ExperienceRepository : IExperienceRepository
{
    private readonly JobApplicationHelperDbContext _dbContext;

    public ExperienceRepository(JobApplicationHelperDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Experience>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var entities = await QueryExperiences()
            .OrderBy(e => e.Title)
            .ToListAsync(cancellationToken);

        return entities.Select(MapToDomain).ToList();
    }

    public async Task<Experience?> GetAsync(ExperienceId id, CancellationToken cancellationToken = default)
    {
        var entity = await QueryExperiences().SingleOrDefaultAsync(e => e.Id == id.Value, cancellationToken);

        return entity is null
            ? null
            : MapToDomain(entity);
    }

    public async Task AddOrReplaceAsync(Experience experience, CancellationToken cancellationToken = default)
    {
        var existing = await _dbContext.Experiences
            .Include(e => e.Evidence)
            .Include(e => e.Links)
            .Include(e => e.Skills)
            .Include(e => e.Contexts)
            .SingleOrDefaultAsync(e => e.Id == experience.Id.Value, cancellationToken);

        if (existing is null)
        {
            var entity = await CreateEntityAsync(experience, cancellationToken);

            _dbContext.Experiences.Add(entity);
        }
        else
        {
            await ReplaceEntityAsync(existing, experience, cancellationToken);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<ExperienceEntity> CreateEntityAsync(Experience experience, CancellationToken cancellationToken)
    {
        var entity = new ExperienceEntity
        {
            Id = experience.Id.Value,
            Title = experience.Title,
            Type = experience.Type,
            Organization = experience.Organization,
            Summary = experience.Summary,
            Notes = experience.Notes
        };

        MapDateRangeToEntity(entity, experience.DateRange);

        await PopulateCollectionsAsync(entity, experience, cancellationToken);

        return entity;
    }

    private async Task ReplaceEntityAsync(ExperienceEntity entity, Experience experience, CancellationToken cancellationToken)
    {
        entity.Title = experience.Title;
        entity.Type = experience.Type;
        entity.Organization = experience.Organization;
        entity.Summary = experience.Summary;
        entity.Notes = experience.Notes;

        MapDateRangeToEntity(entity, experience.DateRange);

        entity.Evidence.Clear();
        entity.Links.Clear();
        entity.Skills.Clear();
        entity.Contexts.Clear();

        await PopulateCollectionsAsync(entity, experience, cancellationToken);
    }

    public async Task DeleteAsync(ExperienceId id, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.Experiences.SingleOrDefaultAsync(e => e.Id == id.Value, cancellationToken)
            ?? throw new KeyNotFoundException($"Experience with ID '{id}' was not found.");

        _dbContext.Experiences.Remove(entity);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private IQueryable<ExperienceEntity> QueryExperiences()
    {
        return _dbContext.Experiences
            .AsNoTracking()
            .Include(e => e.Evidence)
            .Include(e => e.Links)
            .Include(e => e.Skills)
                .ThenInclude(e => e.Skill)
            .Include(e => e.Contexts)
                .ThenInclude(e => e.Context);
    }

    private async Task PopulateCollectionsAsync(ExperienceEntity entity, Experience experience, CancellationToken cancellationToken)
    {
        var evidenceIndex = 0;

        foreach (var evidence in experience.Evidence.Where(e => !string.IsNullOrWhiteSpace(e)))
        {
            entity.Evidence.Add(new ExperienceEvidenceEntity
            {
                Id = Guid.NewGuid(),
                ExperienceId = entity.Id,
                SortOrder = evidenceIndex++,
                Text = evidence.Trim()
            });
        }

        var linkIndex = 0;

        foreach (var link in experience.Links.Where(l => !string.IsNullOrWhiteSpace(l)))
        {
            entity.Links.Add(new ExperienceLinkEntity
            {
                Id = Guid.NewGuid(),
                ExperienceId = entity.Id,
                SortOrder = linkIndex++,
                Url = link.Trim(),
                Description = null
            });
        }

        foreach (var skillName in experience.Skills.Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => s.Trim()).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var skill = await GetOrCreateSkillAsync(skillName, cancellationToken);

            entity.Skills.Add(new ExperienceSkillEntity
            {
                ExperienceId = entity.Id,
                SkillId = skill.Id
            });
        }

        foreach (var contextName in experience.Contexts
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .Select(c => c.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var context = await GetOrCreateContextAsync(contextName, cancellationToken);

            entity.Contexts.Add(new ExperienceContextEntity
            {
                ExperienceId = entity.Id,
                ContextId = context.Id
            });
        }
    }

    private async Task<ContextEntity> GetOrCreateContextAsync(string name, CancellationToken cancellationToken)
    {
        var context = await _dbContext.Contexts.SingleOrDefaultAsync(c => c.Name == name, cancellationToken);

        if (context is not null)
            return context;

        context = new ContextEntity
        {
            Id = Guid.NewGuid(),
            Name = name
        };

        _dbContext.Contexts.Add(context);

        return context;
    }

    private async Task<SkillEntity> GetOrCreateSkillAsync(string name, CancellationToken cancellationToken)
    {
        var skill = await _dbContext.Skills.SingleOrDefaultAsync(s => s.Name == name, cancellationToken);

        if (skill is not null)
            return skill;

        skill = new SkillEntity
        {
            Id = Guid.NewGuid(),
            Name = name
        };

        _dbContext.Skills.Add(skill);

        return skill;
    }

    private static Experience MapToDomain(ExperienceEntity entity)
    {
        return new Experience
        {
            Id = new ExperienceId(entity.Id),
            Title = entity.Title,
            Type = entity.Type,
            Organization = entity.Organization ?? string.Empty,

            DateRange = MapDateRange(entity),

            Summary = entity.Summary,

            Skills = entity.Skills
                .OrderBy(e => e.Skill.Name)
                .Select(e => e.Skill.Name)
                .ToList(),

            Evidence = entity.Evidence
                .OrderBy(e => e.SortOrder)
                .Select(e => e.Text)
                .ToList(),

            Contexts = entity.Contexts
                .OrderBy(e => e.Context.Name)
                .Select(e => e.Context.Name)
                .ToList(),

            Links = entity.Links
                .OrderBy(e => e.SortOrder)
                .Select(e => e.Url)
                .ToList(),

            Notes = entity.Notes ?? string.Empty
        };
    }

    private static void MapDateRangeToEntity(ExperienceEntity entity, DateRange? dateRange)
    {
        entity.StartYear = dateRange?.Start?.Year;
        entity.StartMonth = dateRange?.Start?.Month;
        entity.StartDay = dateRange?.Start?.Day;

        entity.EndYear = dateRange?.End?.Year;
        entity.EndMonth = dateRange?.End?.Month;
        entity.EndDay = dateRange?.End?.Day;
    }

    private static DateRange? MapDateRange(ExperienceEntity entity)
    {
        return new DateRange
        {
            Start = entity.StartYear is not int startYear
                ? null
                : new PartialDate
                {
                    Year = startYear,
                    Month = entity.StartMonth,
                    Day = entity.StartDay
                },

            End = entity.EndYear is not int endYear
                ? null
                : new PartialDate
                {
                    Year = endYear,
                    Month = entity.EndMonth,
                    Day = entity.EndDay
                }
        };
    }
}