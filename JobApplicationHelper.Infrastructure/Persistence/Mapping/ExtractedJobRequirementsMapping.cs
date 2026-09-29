using JobApplicationHelper.Domain.Models;
using JobApplicationHelper.Infrastructure.Persistence.Entities;
using System.Text.Json;

namespace JobApplicationHelper.Infrastructure.Persistence;

public static class ExtractedJobRequirementsMapping
{
    public static ExtractedJobRequirementsEntity ToEntity(this ExtractedJobRequirements requirements)
    {
        return new ExtractedJobRequirementsEntity
        {
            JobApplicationId = requirements.JobApplicationId.Value,
            RequirementsJson = JsonSerializer.Serialize(requirements.Requirements),
            CreatedAt = requirements.CreatedAt
        };
    }

    public static ExtractedJobRequirements ToDomain(this ExtractedJobRequirementsEntity entity)
    {
        var requirements =
            JsonSerializer.Deserialize<JobRequirements>(entity.RequirementsJson)
            ?? throw new InvalidOperationException($"Could not deserialize extracted job requirements for job application '{entity.JobApplicationId}'.");

        return ExtractedJobRequirements.Rehydrate(
            new JobApplicationId(entity.JobApplicationId),
            requirements,
            entity.CreatedAt);
    }
}