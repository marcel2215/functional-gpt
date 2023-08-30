using System.Text.Json;
using System.Text.Json.Serialization;

namespace FunctionalGPT;

internal class CamelCaseEnumConverter : JsonStringEnumConverter
{
    public CamelCaseEnumConverter() : base(JsonNamingPolicy.CamelCase) { }
}
