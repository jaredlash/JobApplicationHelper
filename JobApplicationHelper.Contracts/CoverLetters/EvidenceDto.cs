using JobApplicationHelper.Contracts.Experiences;

namespace JobApplicationHelper.Contracts.CoverLetters;

public sealed record EvidenceDto(ExperienceDto Experience, string EvidenceNote);