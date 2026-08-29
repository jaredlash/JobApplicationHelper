namespace JobApplicationHelper.Models;

public sealed class JobRequirement
{
    private readonly RequirementEvidence _evidence;
    public JobRequirement()
    {
        _evidence = new RequirementEvidence();
    }
    public string Requirement { get; init; } = "";

    public RequirementCategory Category { get; init; }

    public RequirementPriority Priority { get; init; }

    public RequirementEvidence Evidence => _evidence;
}