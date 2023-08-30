using System.Text.Json.Serialization;

namespace FunctionalGPT;

internal record CompletionRequest
{
    internal CompletionRequest(string model, IEnumerable<Message> messages)
    {
        Model = model;
        Messages = messages;
    }

    [JsonPropertyName("model")]
    internal string Model { get; set; }

    [JsonPropertyName("messages")]
    internal IEnumerable<Message> Messages { get; set; }
}
