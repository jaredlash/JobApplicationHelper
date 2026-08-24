using System.ComponentModel.DataAnnotations;

namespace JobApplicationHelper.Configuration;

public sealed class ExperienceBankOptions
{
    [Required(AllowEmptyStrings = false)]
    public string DatabaseFileName { get; set; } = string.Empty;
}
