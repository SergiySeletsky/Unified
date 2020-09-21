using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Unified.Json
{
    /// <summary>
    /// UnifiedIdConverter.
    /// </summary>
    public class UnifiedIdConverter : JsonConverter
    {
        /// <inheritdoc/>
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(UnifiedId);
        }

        /// <inheritdoc/>
        public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            return UnifiedId.Parse(JToken.Load(reader).ToString());
        }

        /// <inheritdoc/>
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            JToken.FromObject(value.ToString()).WriteTo(writer);
        }
    }
}
