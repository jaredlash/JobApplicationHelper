namespace JobApplicationHelper.Models;

public sealed class JobRequirement
{
    public string Requirement { get; init; } = "";

    public RequirementCategory Category { get; init; }

    public RequirementPriority Priority { get; init; }
}