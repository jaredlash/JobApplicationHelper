namespace JobApplicationHelper.Models;

public sealed class PreviousJobRequirementImpl
{
    public string Requirement { get; set; } = string.Empty;
    public List<EvidenceReference> Evidence { get; set; } = [];
    public MatchStrength Match { get; set; } = MatchStrength.None;
}
