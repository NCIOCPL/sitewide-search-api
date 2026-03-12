
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NCI.OCPL.Api.SiteWideSearch;

/// <summary>
/// Custom converter for deserializing a SiteWideSearchResult from JSON.
/// This is necessary to handle the facts that:
/// a) Elasticsearch may return the description as a single string, an array of strings,
///    or a nested array of arrays of strings.
/// b) The JSON property names in Elasticsearch don't match the property names on the
///    SiteWideSearchResult class, and we don't want to use [JsonPropertyName] attributes on
///    the class itself because it would interfere with serialization of the same class for
///    end-user output.
/// </summary>
class SiteWideSearchResultConverter : JsonConverter<SiteWideSearchResult>
{
    public override SiteWideSearchResult Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using (JsonDocument jsonDoc = JsonDocument.ParseValue(ref reader))
        {
            var root = jsonDoc.RootElement;

            string GetString(string propertyName) =>
                root.TryGetProperty(propertyName, out JsonElement element) ? element.GetString() : null;

            return new SiteWideSearchResult
            {
                Title = GetString("title"),
                URL = GetString("url"),
                ContentType = GetString("metatag.dcterms.type"),
                Description = ReadDescription(root)
            };
        }
    }

    /// <summary>
    /// Reads the metatag.description field, handling the various formats Elasticsearch may use:
    /// a single string, an array of strings, or a nested array of arrays of strings.
    /// Only the first value is returned.
    /// </summary>
    /// <param name="root">The root JSON element of the document.</param>
    /// <returns>The first description string found, or an empty string if the array is empty.</returns>
    private static string ReadDescription(JsonElement root)
    {
        var element = root.GetProperty("metatag.description");

        switch (element.ValueKind)
        {
            case JsonValueKind.String:
                return element.GetString();

            case JsonValueKind.Array:
                if (element.GetArrayLength() == 0)
                    return String.Empty;

                var first = element[0];

                // Handle nested array: [["description", ...]]
                if (first.ValueKind == JsonValueKind.Array)
                {
                    if (first.GetArrayLength() == 0)
                        return String.Empty;
                    return first[0].GetString() ?? String.Empty;
                }

                return first.GetString() ?? String.Empty;

            default:
                throw new JsonException($"Don't know how to work with tokens of type '{element.ValueKind}'.");
        }
    }

    /// <summary>
    /// Writes the JSON representation of a <see cref="SiteWideSearchResult"/> using
    /// the output property names.
    /// </summary>
    /// <param name="writer">The Utf8JsonWriter to write to.</param>
    /// <param name="value">The value.</param>
    /// <param name="options">The serializer options.</param>
    public override void Write(Utf8JsonWriter writer, SiteWideSearchResult value, JsonSerializerOptions options)
    {
        // Apply the naming policy from options (e.g. camelCase), falling back to the property name as-is.
        string Name(string name) => options.PropertyNamingPolicy?.ConvertName(name) ?? name;

        writer.WriteStartObject();
        writer.WriteString(Name(nameof(SiteWideSearchResult.Title)), value.Title);
        writer.WriteString(Name(nameof(SiteWideSearchResult.URL)), value.URL);
        writer.WriteString(Name(nameof(SiteWideSearchResult.ContentType)), value.ContentType);
        writer.WriteString(Name(nameof(SiteWideSearchResult.Description)), value.Description);
        writer.WriteEndObject();
    }
}