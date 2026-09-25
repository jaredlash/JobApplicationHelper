using System.ComponentModel.DataAnnotations;

namespace JobApplicationHelper.Application.Configuration;

public sealed class CandidateCvTextPath
{
    [Required]
    public string CountryCode { get; set; } = string.Empty;

    [Required]
    public string Path { get; set; } = string.Empty;
}
