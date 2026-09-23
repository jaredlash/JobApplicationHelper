using System.ComponentModel.DataAnnotations;

namespace JobApplicationHelper.Domain.Models;

public class LocationsOptions
{
    [Required]
    [MinLength(1)]
    public List<Location> Locations { get; set; } = [];
}
