using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DomainDetective;

/// <summary>
/// Converts <see cref="CountryId"/> values to and from JSON using extension helpers.
/// </summary>
internal sealed class CountryIdConverter : JsonConverter<CountryId>
{
    /// <inheritdoc/>
    public override CountryId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        return CountryIdExtensions.TryParse(value, out var id) ? id : default;
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, CountryId value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToName());
    }
}
