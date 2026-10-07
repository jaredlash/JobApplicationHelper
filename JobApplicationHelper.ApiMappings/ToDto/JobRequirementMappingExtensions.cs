using JobApplicationHelper.Contracts.CoverLetters;
using JobApplicationHelper.Contracts.Experiences;
using JobApplicationHelper.Contracts.JobRequirements;
using JobApplicationHelper.Domain.Models;

namespace JobApplicationHelper.ApiMappings.ToDto;

public static class JobRequirementMappingExtensions
{
    public static JobRequirementsDto ToDto(this JobRequirements jobRequirements)
    {
        var requirementsWithEvidenceDtos = jobRequirements.Requirements
            .Select(r => new JobRequirementWithEvidenceDto(
                r.Requirement,
                r.Category.ToString(),
                r.Priority.ToString(),
                r.Evidence.ToDto()))
            .ToList();
        return new JobRequirementsDto(requirementsWithEvidenceDtos);
    }

    public static RequirementEvidenceDto ToDto(this RequirementEvidence requirementEvidence)
    {
        if (requirementEvidence.NoSupportingEvidence) return new RequirementEvidenceDto(true, []);

        var evidenceDtos = requirementEvidence.Evidences
            .Select(e => new EvidenceDto(
                e.Experience.ToDto(),
                e.EvidenceNote
                ))
            .ToList();

        return new RequirementEvidenceDto(false, evidenceDtos);
    }

}
