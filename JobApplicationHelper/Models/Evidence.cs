namespace JobApplicationHelper.Models;

public class Evidence
{
    public required Experience Experience { get; init; }
    public string EvidenceNote { get; set; } = string.Empty;
}