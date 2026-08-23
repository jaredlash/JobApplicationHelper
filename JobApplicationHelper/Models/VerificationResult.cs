namespace JobApplicationHelper.Models;

public sealed class VerificationResult
{
    public bool IsValid => Errors.Count == 0 && UnsupportedClaims.Count == 0 && StyleViolations.Count == 0 && RequiredCorrections.Count == 0;

    public List<string> Errors { get; set; } = [];

    public List<string> UnsupportedClaims { get; set; } = [];

    public List<string> StyleViolations { get; set; } = [];

    public List<string> RequiredCorrections { get; set; } = [];
}
