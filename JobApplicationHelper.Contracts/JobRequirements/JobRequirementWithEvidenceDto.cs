using JobApplicationHelper.Contracts.CoverLetters;

namespace JobApplicationHelper.Contracts.JobRequirements;

public sealed record JobRequirementWithEvidenceDto(
    string Requirement,
    string Category,
    string Priority,
    RequirementEvidenceDto Evidence);
