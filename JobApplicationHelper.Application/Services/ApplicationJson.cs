using System.Text.Json;
using System.Text.Json.Serialization;

namespace JobApplicationHelper.Application.Services;

public static class ApplicationJson
{
    public static readonly JsonSerializerOptions Options = new()
    {
        Converters =
        {
            new JsonStringEnumConverter(namingPolicy: null, allowIntegerValues: false)
        },
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        PropertyNameCaseInsensitive = true
    };
}