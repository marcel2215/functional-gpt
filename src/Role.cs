using System.Text.Json.Serialization;

namespace FunctionalGPT;

[JsonConverter(typeof(SnakeCaseEnumConverter))]
public enum Role
{
    System,
    User,
    Assistant,
    Function
}
