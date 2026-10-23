using System.Text.Json.Nodes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SFA.DAS.Learning.AcceptanceTests.Helpers;

/// <summary>
/// The acceptance tests deserialise responses with Newtonsoft, which cannot build System.Text.Json's JsonNode
/// (the type of the old/new values on UpdateLearnerResult.ChangeDetails). Reads any JSON token, scalar or array, into one.
/// </summary>
internal class JsonNodeNewtonsoftConverter : JsonConverter<JsonNode>
{
    public override JsonNode? ReadJson(JsonReader reader, Type objectType, JsonNode? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var token = JToken.Load(reader);
        return token.Type == JTokenType.Null ? null : JsonNode.Parse(token.ToString(Formatting.None));
    }

    public override void WriteJson(JsonWriter writer, JsonNode? value, JsonSerializer serializer)
    {
        if (value == null)
        {
            writer.WriteNull();
            return;
        }

        writer.WriteRawValue(value.ToJsonString());
    }
}
