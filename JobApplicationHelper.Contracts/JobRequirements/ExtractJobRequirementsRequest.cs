namespace JobApplicationHelper.Contracts.JobRequirements;

public record ExtractJobRequirementsRequest(Guid JobApplicationId, string Priority);