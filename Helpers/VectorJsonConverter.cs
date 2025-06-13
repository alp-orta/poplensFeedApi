using System.Text.Json;
using System.Text.Json.Serialization;
using Pgvector;

namespace poplensFeedApi.Helpers { 
    public class VectorJsonConverter : JsonConverter<Vector> {
        public override Vector? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
            var values = JsonSerializer.Deserialize<float[]>(ref reader, options);
            if (values == null) return null;
            return new Vector(values);
        }

        public override void Write(Utf8JsonWriter writer, Vector value, JsonSerializerOptions options) {
            JsonSerializer.Serialize(writer, value.ToArray(), options);
        }
    }


}
