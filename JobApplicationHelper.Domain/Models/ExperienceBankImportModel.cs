namespace JobApplicationHelper.Domain.Models;

public sealed class ExperienceBankImportModel
{
    public int Version { get; set; } = 1;

    public List<ExperienceImportModel> Experiences { get; set; } = [];
}