using JobApplicationHelper.Contracts.CoverLetters;
using JobApplicationHelper.Contracts.JobRequirements;
using JobApplicationHelper.Domain.Models;

namespace JobApplicationHelper.ApiMappings.ToDomain;

public static class JobRequirementMappingExtensions
{
    public static JobRequirement ToDomain(this JobRequirementDto jobRequirementDto)
    {
        return new JobRequirement
        {
            Requirement = jobRequirementDto.Requirement,
            Category = Enum.Parse<RequirementCategory>(
                        jobRequirementDto.Category,
                        ignoreCase: true),
            Priority = Enum.Parse<RequirementPriority>(
                        jobRequirementDto.Priority,
                        ignoreCase: true)
        };
    }

    public static JobRequirement ToDomain(this JobRequirementWithEvidenceDto jobRequirementWithEvidenceDto)
    {
        var jobRequirement = new JobRequirement
        {
            Requirement = jobRequirementWithEvidenceDto.Requirement,
            Category = Enum.Parse<RequirementCategory>(
                        jobRequirementWithEvidenceDto.Category,
                        ignoreCase: true),
            Priority = Enum.Parse<RequirementPriority>(
                        jobRequirementWithEvidenceDto.Priority,
                        ignoreCase: true)
        };

        if (jobRequirementWithEvidenceDto.Evidence.NoSupportingEvidence)
        {
            jobRequirement.Evidence.NoSupportingEvidence = true;
            return jobRequirement;
        }

        foreach (var evidenceDto in jobRequirementWithEvidenceDto.Evidence.Evidences)
        {
            jobRequirement.Evidence.AddEvidence(evidenceDto.ToDomain());
        }
        return jobRequirement;
    }

    public static Evidence ToDomain(this EvidenceDto evidenceDto)
    {
        return new Evidence
        {
            Experience = evidenceDto.Experience.ToDomain(),
            EvidenceNote = evidenceDto.EvidenceNote
        };
    }
}
