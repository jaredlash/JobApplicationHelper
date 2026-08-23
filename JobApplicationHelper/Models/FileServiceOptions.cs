using System.ComponentModel.DataAnnotations;

namespace JobApplicationHelper.Models;

public class FileServiceOptions
{
    [Required]
    public string TemplateBasePath { get; set; } = string.Empty;

    [Required]
    public string ApplicationBasePath { get; set; } = string.Empty;
}
