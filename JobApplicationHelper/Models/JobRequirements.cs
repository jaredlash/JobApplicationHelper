using System.Text.Json;
using System.Text.Json.Serialization;

namespace JobApplicationHelper.Models;

public sealed class JobRequirements
{
    public List<JobRequirement> Requirements { get; init; } = [];

    public override string ToString()
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        options.Converters.Add(new JsonStringEnumConverter());

        return JsonSerializer.Serialize(this, options); 
    }
}
