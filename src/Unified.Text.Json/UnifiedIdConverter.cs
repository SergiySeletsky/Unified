using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Unified.Text.Json
{
    /// <summary>
    /// Converts a <see cref="UnifiedId"/> to and from its canonical 13-character HEX32 string
    /// representation for System.Text.Json, including use as a dictionary key.
    /// </summary>
    public sealed class UnifiedIdJsonConverter : JsonConverter<UnifiedId>
    {
        /// <inheritdoc/>
        public override UnifiedId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return UnifiedId.Parse(reader.GetString() ?? string.Empty);
        }

        /// <inheritdoc/>
        public override void Write(Utf8JsonWriter writer, UnifiedId value, JsonSerializerOptions options)
        {
            if (writer is null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            writer.WriteStringValue(value.ToString());
        }

        /// <inheritdoc/>
        public override UnifiedId ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return UnifiedId.Parse(reader.GetString() ?? string.Empty);
        }

        /// <inheritdoc/>
        public override void WriteAsPropertyName(Utf8JsonWriter writer, UnifiedId value, JsonSerializerOptions options)
        {
            if (writer is null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            writer.WritePropertyName(value.ToString());
        }
    }
}
