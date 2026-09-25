using System.ComponentModel.DataAnnotations;

namespace JobApplicationHelper.Application.Configuration;

public sealed class ApplicationDocumentOptions
{
    [Required]
    [MinLength(1)]
    public string ApplicationsBasePath { get; set; } = string.Empty;

    [Required]
    [MinLength(1)]
    public string TemplateBasePath { get; set; } = string.Empty;

    [Required]
    [MinLength(1)]
    public List<ApplicationDocumentConfiguration> Documents { get; set; } = [];
}