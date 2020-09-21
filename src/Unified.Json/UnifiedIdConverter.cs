using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Unified.Json
{
    /// <summary>
    /// UnifiedIdConverter.
    /// </summary>
    public class UnifiedIdConverter : JsonConverter<UnifiedId>
    {
        /// <inheritdoc/>
        public override UnifiedId ReadJson(JsonReader reader, Type objectType, UnifiedId existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            return UnifiedId.Parse(JToken.Load(reader).ToString());
        }

        /// <inheritdoc/>
        public override void WriteJson(JsonWriter writer, UnifiedId value, JsonSerializer serializer)
        {
            JToken.FromObject(value.ToString()).WriteTo(writer);
        }
    }
}
