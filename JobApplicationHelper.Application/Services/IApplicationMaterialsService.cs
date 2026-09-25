using JobApplicationHelper.Domain.Models;

namespace JobApplicationHelper.Application.Services;

public interface IApplicationMaterialsService
{
    JobApplicationId CreateApplicationMaterials(ApplicationFile application);

    void SaveCoverLetterDraft(JobApplicationId applicationId, string draft);

    string GetApplicationFolder(JobApplicationId applicationId);
}
