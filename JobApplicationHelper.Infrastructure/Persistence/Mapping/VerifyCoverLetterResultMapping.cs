using JobApplicationHelper.Domain.Models;
using JobApplicationHelper.Infrastructure.Persistence.Entities;
using System.Text.Json;

namespace JobApplicationHelper.Infrastructure.Persistence.Mapping;

public static class VerifyCoverLetterResultMapping
{
    public static VerifyCoverLetterResultEntity ToEntity(this VerifyCoverLetterResult result)
    {
        return new VerifyCoverLetterResultEntity
        {
            JobApplicationId = result.JobApplicationId.Value,
            VerificationResultJson = JsonSerializer.Serialize(result.VerificationResult),
            CreatedAt = result.CreatedAt
        };
    }

    public static VerifyCoverLetterResult ToDomain(this VerifyCoverLetterResultEntity entity)
    {
        var verificationResult = JsonSerializer.Deserialize<VerificationResult>(entity.VerificationResultJson)
            ?? throw new InvalidOperationException($"Could not deserialize verification result for job application '{entity.JobApplicationId}'.");

        return VerifyCoverLetterResult.Rehydrate(
            new JobApplicationId(entity.JobApplicationId),
            verificationResult,
            entity.CreatedAt);
    }

    public static void UpdateEntity(this VerifyCoverLetterResultEntity entity, VerifyCoverLetterResult result)
    {
        entity.VerificationResultJson = JsonSerializer.Serialize(result.VerificationResult);
        entity.CreatedAt = result.CreatedAt;
    }
}