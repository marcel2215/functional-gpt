using System.Text.Json.Serialization;

namespace FunctionalGPT;

[JsonConverter(typeof(CamelCaseEnumConverter))]
public enum Role
{
    System,
    User,
    Assistant,
    Function
}
