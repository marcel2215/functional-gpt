using System.Text.Json.Serialization;

namespace FunctionalGPT;

public record FunctionCall
{
    public FunctionCall(string name, string arguments)
    {
        Name = name;
        Arguments = arguments;
    }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("arguments")]
    public string Arguments { get; set; }
}
