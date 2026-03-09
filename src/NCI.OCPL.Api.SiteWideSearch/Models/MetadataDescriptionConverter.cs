using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NCI.OCPL.Api.SiteWideSearch
{
    /// <summary>
    /// Converts a JSON element containing either a single string, or an array of strings, into
    /// a single string.
    /// </summary>
    public class MetadataDescriptionConverter : JsonConverter<string>
    {
        /// <summary>
        /// Responsible for reading a JSON element containing either a single string or an array of strings
        /// and converting it into a single string by discarding all but the first one.
        /// </summary>
        /// <param name="reader">The Utf8JsonReader to read from.</param>
        /// <param name="typeToConvert">Type of the destination object (always System.String).</param>
        /// <param name="options">The serializer options.</param>
        /// <returns>The first string value found, or an empty string if the array is empty.</returns>
        public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // If it's just a string, return the value.
            if (reader.TokenType == JsonTokenType.String)
            {
                return reader.GetString();
            }
            else if (reader.TokenType == JsonTokenType.StartArray)
            {
                string value;

                // Record the current depth.
                int initialDepth = reader.CurrentDepth;

                // Advance to next token, and check for a possible nested array.
                while (reader.Read() && reader.TokenType == JsonTokenType.StartArray)
                    continue;

                // Get the value, if one exists.
                switch (reader.TokenType)
                {
                    case JsonTokenType.String:
                        value = reader.GetString();
                        break;

                    case JsonTokenType.EndArray:
                        value = String.Empty;
                        break;

                    default:
                        throw new JsonException($"Don't know how to work with tokens of type '{reader.TokenType}'.");
                }

                // Advance to end of the outermost array.
                while (reader.CurrentDepth > initialDepth)
                    reader.Read();

                return value;
            }
            else
            {
                throw new JsonException($"Don't know how to work with tokens of type '{reader.TokenType}'.");
            }
        }

        /// <summary>
        /// Writes the JSON representation of the object.
        /// </summary>
        /// <param name="writer">The Utf8JsonWriter to write to.</param>
        /// <param name="value">The value.</param>
        /// <param name="options">The serializer options.</param>
        public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value);
        }
   }
}