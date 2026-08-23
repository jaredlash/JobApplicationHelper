using System.ComponentModel.DataAnnotations;

namespace JobApplicationHelper.Models
{
    public record CandidateOptions
    {
        [Required(AllowEmptyStrings = false)]
        public string CandidateFullName { get; init; } = string.Empty;
    }
}
