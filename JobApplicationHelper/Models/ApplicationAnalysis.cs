namespace JobApplicationHelper.Models;

public sealed class ApplicationAnalysis
{
    public List<PreviousJobRequirementImpl> JobRequirements { get; set; } = [];

    public List<CandidateStrength> CandidateStrengths { get; set; } = [];

    public List<string> RecommendedThemes { get; set; } = [];

    public List<string> PotentialGaps { get; set; } = [];

    public string SuggestedApproach { get; set; } = string.Empty;
}
