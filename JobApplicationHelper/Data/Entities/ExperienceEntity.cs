using JobApplicationHelper.Models;

namespace JobApplicationHelper.Data.Entities;

public sealed class ExperienceEntity
{
    public string Id { get; set; } = "";

    public string Title { get; set; } = "";

    public ExperienceType Type { get; set; }

    public string? Organization { get; set; }

    public int? StartMonth { get; set; }

    public int? StartYear { get; set; }

    public int? EndMonth { get; set; }

    public int? EndYear { get; set; }

    public string Summary { get; set; } = "";

    public string? Notes { get; set; }

    public ICollection<ExperienceEvidenceEntity> Evidence { get; set; } = [];

    public ICollection<ExperienceSkillEntity> Skills { get; set; } = [];

    public ICollection<ExperienceContextEntity> Contexts { get; set; } = [];

    public ICollection<ExperienceLinkEntity> Links { get; set; } = [];
}