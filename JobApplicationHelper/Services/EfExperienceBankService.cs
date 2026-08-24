using JobApplicationHelper.Data;
using JobApplicationHelper.Data.Entities;
using JobApplicationHelper.Models;
using Microsoft.EntityFrameworkCore;

namespace JobApplicationHelper.Services;

public sealed class EfExperienceBankService : IExperienceBankService
{
    private readonly ExperienceBankDbContext _dbContext;

    public EfExperienceBankService(ExperienceBankDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Experience>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var entities = await _dbContext.Experiences
            .AsNoTracking()
            .Include(e => e.Evidence)
            .Include(e => e.Links)
            .Include(e => e.Skills)
                .ThenInclude(es => es.Skill)
            .Include(e => e.Contexts)
                .ThenInclude(ec => ec.Context)
            .OrderBy(e => e.Title)
            .ToListAsync(cancellationToken);

        return entities
            .Select(MapToDomain)
            .ToList();
    }

    public async Task<Experience?> GetByIdAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        var entity = await _dbContext.Experiences
            .AsNoTracking()
            .Include(e => e.Evidence)
            .Include(e => e.Links)
            .Include(e => e.Skills)
                .ThenInclude(es => es.Skill)
            .Include(e => e.Contexts)
                .ThenInclude(ec => ec.Context)
            .SingleOrDefaultAsync(
                e => e.Id == id,
                cancellationToken);

        return entity is null
            ? null
            : MapToDomain(entity);
    }

    public async Task AddAsync(
        Experience experience,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(experience);

        ArgumentException.ThrowIfNullOrWhiteSpace(experience.Id);

        var exists = await _dbContext.Experiences
            .AnyAsync(
                e => e.Id == experience.Id,
                cancellationToken);

        if (exists)
        {
            throw new InvalidOperationException(
                $"An experience with ID '{experience.Id}' already exists.");
        }

        var entity = await MapToEntityAsync(
            experience,
            cancellationToken);

        _dbContext.Experiences.Add(entity);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(
        Experience experience,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(experience);

        ArgumentException.ThrowIfNullOrWhiteSpace(experience.Id);

        var entity = await _dbContext.Experiences
            .Include(e => e.Evidence)
            .Include(e => e.Links)
            .Include(e => e.Skills)
            .Include(e => e.Contexts)
            .SingleOrDefaultAsync(
                e => e.Id == experience.Id,
                cancellationToken);

        if (entity is null)
        {
            throw new KeyNotFoundException(
                $"Experience with ID '{experience.Id}' was not found.");
        }

        await UpdateEntityAsync(
            entity,
            experience,
            cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        var entity = await _dbContext.Experiences
            .SingleOrDefaultAsync(
                e => e.Id == id,
                cancellationToken);

        if (entity is null)
        {
            throw new KeyNotFoundException(
                $"Experience with ID '{id}' was not found.");
        }

        _dbContext.Experiences.Remove(entity);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static Experience MapToDomain(
        ExperienceEntity entity)
    {
        return new Experience
        {
            Id = entity.Id,
            Title = entity.Title,
            Type = entity.Type,
            Organization = entity.Organization,

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

            Notes = entity.Notes
        };
    }

    private static DateRange? MapDateRange(ExperienceEntity entity)
    {
        if (entity.StartYear is not int startYear)
        {
            return null;
        }

        return new DateRange
        {
            Start = new PartialDate
            {
                Year = startYear,
                Month = entity.StartMonth
            },

            End = entity.EndYear is not int endYear
                ? null
                : new PartialDate
                {
                    Year = endYear,
                    Month = entity.EndMonth
                }
        };
    }

    private async Task<ExperienceEntity> MapToEntityAsync(
        Experience experience,
        CancellationToken cancellationToken)
    {
        var entity = new ExperienceEntity
        {
            Id = experience.Id,
            Title = experience.Title,
            Type = experience.Type,
            Organization = experience.Organization,
            Summary = experience.Summary ?? string.Empty,
            Notes = experience.Notes
        };

        MapDateRangeToEntity(
            experience.DateRange,
            entity);

        await PopulateCollectionsAsync(
            entity,
            experience,
            cancellationToken);

        return entity;
    }

    private static void MapDateRangeToEntity(
        DateRange? dateRange,
        ExperienceEntity entity)
    {
        if (dateRange is null)
        {
            entity.StartMonth = null;
            entity.StartYear = null;
            entity.EndMonth = null;
            entity.EndYear = null;

            return;
        }

        entity.StartMonth = dateRange.Start?.Month;
        entity.StartYear = dateRange.Start?.Year;
        entity.EndMonth = dateRange.End?.Month;
        entity.EndYear = dateRange.End?.Year;
    }

    private async Task UpdateEntityAsync(
        ExperienceEntity entity,
        Experience experience,
        CancellationToken cancellationToken)
    {
        entity.Title = experience.Title;
        entity.Type = experience.Type;
        entity.Organization = experience.Organization;
        entity.Summary = experience.Summary ?? string.Empty;
        entity.Notes = experience.Notes;

        MapDateRangeToEntity(
            experience.DateRange,
            entity);

        _dbContext.ExperienceEvidence.RemoveRange(entity.Evidence);
        _dbContext.ExperienceLinks.RemoveRange(entity.Links);
        _dbContext.ExperienceSkills.RemoveRange(entity.Skills);
        _dbContext.ExperienceContexts.RemoveRange(entity.Contexts);

        entity.Evidence.Clear();
        entity.Links.Clear();
        entity.Skills.Clear();
        entity.Contexts.Clear();

        await PopulateCollectionsAsync(
            entity,
            experience,
            cancellationToken);
    }

    private async Task PopulateCollectionsAsync(
        ExperienceEntity entity,
        Experience experience,
        CancellationToken cancellationToken)
    {
        var evidence = experience.Evidence
            .Where(e => !string.IsNullOrWhiteSpace(e))
            .Select(e => e.Trim())
            .ToList();

        for (var i = 0; i < evidence.Count; i++)
        {
            entity.Evidence.Add(
                new ExperienceEvidenceEntity
                {
                    ExperienceId = entity.Id,
                    SortOrder = i,
                    Text = evidence[i]
                });
        }

        var links = experience.Links
            .Where(l => !string.IsNullOrWhiteSpace(l))
            .Select(l => l.Trim())
            .ToList();

        for (var i = 0; i < links.Count; i++)
        {
            entity.Links.Add(
                new ExperienceLinkEntity
                {
                    ExperienceId = entity.Id,
                    SortOrder = i,
                    Url = links[i]
                });
        }

        var skillNames = experience.Skills
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(s => s.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase);

        foreach (var skillName in skillNames)
        {
            var skill = await GetOrCreateSkillAsync(
                skillName,
                cancellationToken);

            entity.Skills.Add(
                new ExperienceSkillEntity
                {
                    ExperienceId = entity.Id,
                    SkillId = skill.Id,
                    Skill = skill
                });
        }

        var contextNames = experience.Contexts
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .Select(c => c.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase);

        foreach (var contextName in contextNames)
        {
            var context = await GetOrCreateContextAsync(
                contextName,
                cancellationToken);

            entity.Contexts.Add(
                new ExperienceContextEntity
                {
                    ExperienceId = entity.Id,
                    ContextId = context.Id,
                    Context = context
                });
        }
    }

    private async Task<SkillEntity> GetOrCreateSkillAsync(
        string name,
        CancellationToken cancellationToken)
    {
        var normalizedName = name.Trim();

        var skill = await _dbContext.Skills
            .SingleOrDefaultAsync(
                s => s.Name == normalizedName,
                cancellationToken);

        if (skill is not null)
        {
            return skill;
        }

        skill = new SkillEntity
        {
            Name = normalizedName
        };

        _dbContext.Skills.Add(skill);

        return skill;
    }

    private async Task<ContextEntity> GetOrCreateContextAsync(
        string name,
        CancellationToken cancellationToken)
    {
        var normalizedName = name.Trim();

        var context = await _dbContext.Contexts
            .SingleOrDefaultAsync(
                c => c.Name == normalizedName,
                cancellationToken);

        if (context is not null)
        {
            return context;
        }

        context = new ContextEntity
        {
            Name = normalizedName
        };

        _dbContext.Contexts.Add(context);

        return context;
    }
}