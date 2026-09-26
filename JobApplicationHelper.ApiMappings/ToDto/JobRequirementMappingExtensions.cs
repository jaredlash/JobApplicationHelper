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

    public static ExperienceDto ToDto(this Experience experience)
    {
        return new ExperienceDto(
                experience.Id,
                experience.Title,
                experience.Type.ToString(),
                experience.Organization,
                experience.DateRange is null
                    ? null
                    : new DateRangeDto(
                        experience.DateRange.Start is null
                            ? null
                            : new PartialDateDto(experience.DateRange.Start.Year, experience.DateRange.Start.Month, experience.DateRange.Start.Day),
                        experience.DateRange.End is null
                            ? null
                            : new PartialDateDto(experience.DateRange.End.Year, experience.DateRange.End.Month, experience.DateRange.End.Day)),
                experience.Summary,
                experience.Skills,
                experience.Evidence,
                experience.Contexts,
                experience.Links,
                experience.Notes);
    }
}
