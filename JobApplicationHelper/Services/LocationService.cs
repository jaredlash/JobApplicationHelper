using JobApplicationHelper.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JobApplicationHelper.Services;

public class LocationService
{
    private IReadOnlyList<Location> _locations;

    public LocationService(IOptions<LocationsOptions> options, ILogger<LocationService> logger)
    {
        if (options == null) throw new ArgumentNullException(nameof(options));

        var opts = options.Value;

        if (opts == null || opts.Locations == null || opts.Locations.Count == 0)
        {
            logger.LogError("Locations options are missing or empty. Ensure configuration provides a non-empty 'Locations' array.");
            throw new InvalidOperationException("Required configuration 'Locations' is missing or empty. Application cannot start without location configuration.");
        }

        _locations = opts.Locations.AsReadOnly();
    }

    public IReadOnlyList<Location> GetLocations()
    {
        return _locations;
    }
}
