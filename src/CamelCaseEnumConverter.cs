using System.Text.Json;
using System.Text.Json.Serialization;

namespace FunctionalGPT;

internal class CamelCaseEnumConverter : JsonStringEnumConverter
{
    internal CamelCaseEnumConverter() : base(JsonNamingPolicy.CamelCase) { }
}
