using System.ComponentModel.DataAnnotations;

namespace JobApplicationHelper.Infrastructure.Configuration;

public record CandidateOptions
{
    [Required(AllowEmptyStrings = false)]
    public string CandidateFullName { get; init; } = string.Empty;
}
