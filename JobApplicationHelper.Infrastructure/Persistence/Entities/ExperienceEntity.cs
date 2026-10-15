using JobApplicationHelper.Domain.Models;

namespace JobApplicationHelper.Infrastructure.Persistence.Entities;

public sealed class ExperienceEntity
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public ExperienceType Type { get; set; }

    public string? Organization { get; set; }

    public int? StartDay { get; set; }

    public int? EndDay { get; set; }

    public int? StartMonth { get; set; }

    public int? StartYear { get; set; }

    public int? EndMonth { get; set; }

    public int? EndYear { get; set; }

    public string Summary { get; set; } = string.Empty;

    public string Notes { get; set; } = string.Empty;

    public string? LegacyId { get; set; }

    public ICollection<ExperienceEvidenceEntity> Evidence { get; set; } = [];

    public ICollection<ExperienceSkillEntity> Skills { get; set; } = [];

    public ICollection<ExperienceContextEntity> Contexts { get; set; } = [];

    public ICollection<ExperienceLinkEntity> Links { get; set; } = [];
}