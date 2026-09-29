using System.ComponentModel.DataAnnotations;

namespace JobApplicationHelper.Infrastructure.Configuration;

public sealed class BackgroundJobOptions
{
    [Range(1, 100)]
    public int MaxConcurrency { get; set; } = 1;
}
