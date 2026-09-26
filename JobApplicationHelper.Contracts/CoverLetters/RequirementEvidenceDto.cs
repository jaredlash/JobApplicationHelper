namespace JobApplicationHelper.Contracts.CoverLetters;

public sealed record RequirementEvidenceDto(bool NoSupportingEvidence, IReadOnlyList<EvidenceDto> Evidences);