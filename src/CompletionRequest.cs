using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace FunctionalGPT;

internal record CompletionRequest
{
    public CompletionRequest(string model, IEnumerable<Message> messages)
    {
        Model = model;
        Messages = messages;
    }

    [JsonPropertyName("model")]
    public string Model { get; set; }

    [JsonPropertyName("messages")]
    public IEnumerable<Message> Messages { get; set; }

    [JsonPropertyName("functions")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public JsonArray? Functions { get; set; }
}
