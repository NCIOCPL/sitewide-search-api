using System;

using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

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
        /// <param name="reader">The JsonReader to read from.</param>
        /// <param name="objectType">Type of the destination object (always System.String).</param>
        /// <param name="existingValue">The existing value of the destination object</param>
        /// <param name="hasExistingValue">Boolean. Does the destination object already have a value?</param>
        /// <param name="serializer">The calling serializer</param>
        /// <returns></returns>
        public override string ReadJson(JsonReader reader, Type objectType, string existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            // If it's just a string, return the value.
            if(reader.TokenType == JsonToken.String) {
                return (string)reader.Value;
            }
            else if(reader.TokenType == JsonToken.StartArray)
            {
                string value;

                // This is an array.
                // Record the current depth.
                int initialDepth = reader.Depth;

                // Advance to next token, and check for a possible nested array.
                while(reader.Read() && reader.TokenType == JsonToken.StartArray)
                    continue;

                // Get the value, if one exists.
                switch (reader.TokenType)
                {
                    case JsonToken.String:
                        value = (String)reader.Value;
                        break;

                    case JsonToken.EndArray:
                        value = String.Empty;
                        break;

                    default:
                        throw new InvalidOperationException($"Don't know how to work with tokens of type '{reader.TokenType}'.");
                }

                // Advance to end of the outermost array.
                while(reader.Depth > initialDepth)
                    reader.Read();

                return value;
            }
            else
            {
                throw new InvalidOperationException($"Don't know how to work with tokens of type '{reader.TokenType}'.");
            }
        }

        /// <summary>
        /// Mark the converter as not being used for writing JSON.
        /// </summary>
        public override bool CanWrite => false;

        /// <summary>
        /// Writes the JSON representation of the object.
        /// </summary>
        /// <param name="writer">The JsonWriter to write to.</param>
        /// <param name="value">The value.</param>
        /// <param name="serializer">The calling serializer.</param>
        public override void WriteJson(JsonWriter writer, string value, JsonSerializer serializer)
        {
            throw new NotImplementedException("This converter not intended for writing.");
        }
   }
}