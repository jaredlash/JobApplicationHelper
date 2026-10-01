using JobApplicationHelper.Domain.Models;
using JobApplicationHelper.Infrastructure.Persistence.Entities;

namespace JobApplicationHelper.Infrastructure.Persistence.Mapping;

public static class CoverLetterDraftsMapping
{
    public static CoverLetterDraftEntity ToEntity(this CoverLetterDraft draft)
    {
        return new CoverLetterDraftEntity
        {
            JobApplicationId = draft.JobApplicationId.Value,
            Draft = draft.Draft,
            CreatedAt = draft.CreatedAt
        };
    }

    public static CoverLetterDraft ToDomain(this CoverLetterDraftEntity entity)
    {
        return CoverLetterDraft.Rehydrate(
            new JobApplicationId(entity.JobApplicationId),
            entity.Draft,
            entity.CreatedAt
        );
    }

    public static void UpdateEntity(this CoverLetterDraftEntity entity, CoverLetterDraft draft)
    {
        entity.Draft = draft.Draft;
        entity.CreatedAt = draft.CreatedAt;
    }
}