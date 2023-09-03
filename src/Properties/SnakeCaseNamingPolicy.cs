using System.Text.Json;

namespace FunctionalGPT.Properties;

internal class SnakeCaseNamingPolicy : JsonNamingPolicy
{
    public override string ConvertName(string name)
    {
        return name.ToSnakeCase();
    }
}
