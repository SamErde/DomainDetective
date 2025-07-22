using System.Text.Json;

namespace DomainDetective.Helpers;

public static class JsonOptions
{
    public static JsonSerializerOptions Default { get; } = new()
    {
        WriteIndented = true,
        Converters =
        {
            new IPAddressJsonConverter(),
            new CountryIdConverter(),
            new LocationIdConverter()
        }
    };
}
