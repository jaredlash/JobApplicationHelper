namespace JobApplicationHelper.Contracts.CoverLetters;

public sealed record VerifyCoverLetterRequest(Guid JobApplicationId, string Priority, CoverLetterDraftParametersDto DraftParameters, string Draft);