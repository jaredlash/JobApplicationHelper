using System.ComponentModel.DataAnnotations;

namespace JobApplicationHelper.Models;

public class LocationsOptions
{
    [Required]
    [MinLength(1)]
    public List<Location> Locations { get; set; } = [];
}
