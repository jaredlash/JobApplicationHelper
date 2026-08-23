namespace JobApplicationHelper.Models;

public sealed class EvidenceReference
{
    public EvidenceSource Source { get; set; }

    public string Evidence { get; set; } = string.Empty;

    public List<string> ExperienceIds { get; set; } = [];
}