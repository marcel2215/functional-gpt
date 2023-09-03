using FunctionalGPT.Properties;
using System.Text.Json.Serialization;

namespace FunctionalGPT;

internal class SnakeCaseEnumConverter : JsonStringEnumConverter
{
    public SnakeCaseEnumConverter() : base(new SnakeCaseNamingPolicy()) { }
}
