using JobApplicationHelper.Domain.Models;

namespace JobApplicationHelper.Application.Services;

public sealed record VerifyCoverLetterJobPayload(CoverLetterDraftParameters DraftParameters, string Draft);