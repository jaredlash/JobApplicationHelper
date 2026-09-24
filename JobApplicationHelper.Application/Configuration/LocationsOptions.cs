using JobApplicationHelper.Domain.Models;
using System.ComponentModel.DataAnnotations;

namespace JobApplicationHelper.Application.Configuration;

public class LocationsOptions
{
    [Required]
    [MinLength(1)]
    public List<Location> Locations { get; set; } = [];
}
