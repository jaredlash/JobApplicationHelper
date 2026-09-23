using System.ComponentModel.DataAnnotations;

namespace JobApplicationHelper.Domain.Models;

public record CandidateOptions
{
    [Required(AllowEmptyStrings = false)]
    public string CandidateFullName { get; init; } = string.Empty;
}
