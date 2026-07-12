using System.Collections.Generic;
using System.Text.Json;
using Unified.Text.Json;
using Xunit;

namespace Unified.Tests
{
    /// <summary>
    /// Tests for the System.Text.Json converter that serializes a UnifiedId as its canonical
    /// HEX32 string, including use as a dictionary key.
    /// </summary>
    public class SystemTextJsonTests
    {
        private const string Sample = "FVVVVVVVVVVVU";

        private static JsonSerializerOptions CreateOptions()
        {
            var options = new JsonSerializerOptions();
            options.Converters.Add(new UnifiedIdJsonConverter());
            return options;
        }

        private sealed class Entity
        {
            public UnifiedId Id { get; set; }

            public string Name { get; set; } = string.Empty;
        }

        [Fact]
        public void SerializesAsCanonicalString()
        {
            var entity = new Entity { Id = new UnifiedId(Sample), Name = "test" };

            var json = JsonSerializer.Serialize(entity, CreateOptions());

            Assert.Contains("\"Id\":\"" + Sample + "\"", json);
        }

        [Fact]
        public void RoundTripsThroughSerialization()
        {
            var options = CreateOptions();
            var entity = new Entity { Id = UnifiedId.NewId(), Name = "round-trip" };

            var json = JsonSerializer.Serialize(entity, options);
            var restored = JsonSerializer.Deserialize<Entity>(json, options);

            Assert.NotNull(restored);
            Assert.Equal(entity.Id, restored!.Id);
            Assert.Equal(entity.Name, restored.Name);
        }

        [Fact]
        public void SupportsUnifiedIdAsDictionaryKey()
        {
            var options = CreateOptions();
            var key = new UnifiedId(Sample);
            var map = new Dictionary<UnifiedId, string> { [key] = "value" };

            var json = JsonSerializer.Serialize(map, options);
            var restored = JsonSerializer.Deserialize<Dictionary<UnifiedId, string>>(json, options);

            Assert.Contains("\"" + Sample + "\":\"value\"", json);
            Assert.NotNull(restored);
            Assert.Equal("value", restored![key]);
        }
    }
}
