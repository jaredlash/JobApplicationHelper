namespace JobApplicationHelper.Models;

public sealed class ExperienceBank
{
    public int Version { get; set; } = 1;

    public List<Experience> Experiences { get; set; } = [];
}