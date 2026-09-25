using System.ComponentModel.DataAnnotations;

namespace JobApplicationHelper.Application.Configuration;

public class CandidateContentOptions
{
    [Required]
    [MinLength(1)]
    public string CandidateFullName { get; set; } = string.Empty;

    [Required]
    [MinLength(1)]
    public List<CandidateCvTextPath> CvTextPathsByCountryCode { get; set; } = [];
}
