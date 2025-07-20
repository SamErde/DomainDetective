using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DomainDetective;

/// <summary>
/// JSON converter for <see cref="CountryId"/> values.
/// </summary>
internal sealed class CountryIdJsonConverter : JsonConverter<CountryId>
{
    /// <inheritdoc/>
    public override CountryId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        if (string.IsNullOrWhiteSpace(value) || !CountryIdExtensions.TryParse(value, out var id))
        {
            return default;
        }
        return id;
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, CountryId value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToName());
    }
}

/// <summary>
/// JSON converter for <see cref="LocationId"/> values.
/// </summary>
internal sealed class LocationIdJsonConverter : JsonConverter<LocationId>
{
    /// <inheritdoc/>
    public override LocationId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        if (string.IsNullOrWhiteSpace(value) || !LocationIdExtensions.TryParse(value, out var id))
        {
            return default;
        }
        return id;
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, LocationId value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToName());
    }
}
