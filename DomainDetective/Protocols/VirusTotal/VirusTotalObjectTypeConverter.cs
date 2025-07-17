using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DomainDetective;

/// <summary>
/// Converts <see cref="VirusTotalObjectType"/> values to and from JSON.
/// </summary>
internal sealed class VirusTotalObjectTypeConverter : JsonConverter<VirusTotalObjectType>
{
    public override VirusTotalObjectType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        return value switch
        {
            "ip_address" => VirusTotalObjectType.IpAddress,
            "domain" => VirusTotalObjectType.Domain,
            "url" => VirusTotalObjectType.Url,
            _ => VirusTotalObjectType.Unknown
        };
    }

    public override void Write(Utf8JsonWriter writer, VirusTotalObjectType value, JsonSerializerOptions options)
    {
        var str = value switch
        {
            VirusTotalObjectType.IpAddress => "ip_address",
            VirusTotalObjectType.Domain => "domain",
            VirusTotalObjectType.Url => "url",
            _ => "unknown"
        };
        writer.WriteStringValue(str);
    }
}
