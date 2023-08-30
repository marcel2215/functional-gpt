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
}
