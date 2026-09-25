using System.ComponentModel.DataAnnotations;

namespace JobApplicationHelper.Services.Api;

public sealed class ApiOptions
{
    [Required]
    public string BaseUrl { get; set; } = string.Empty;
}