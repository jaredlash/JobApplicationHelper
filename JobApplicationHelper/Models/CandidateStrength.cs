namespace JobApplicationHelper.Models;

public sealed class CandidateStrength
{
    public string Strength { get; set; } = string.Empty;

    public List<EvidenceReference> Evidence { get; set; } = [];
}
