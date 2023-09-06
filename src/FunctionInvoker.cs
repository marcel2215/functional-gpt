using FunctionalGPT.Properties;
using System.Text.Json;

namespace FunctionalGPT;

internal static class FunctionInvoker
{
    internal static async Task<string> InvokeAsync(Delegate function, string arguments, CancellationToken cancellationToken = default)
    {
        var modelArguments = JsonDocument.Parse(arguments);
        var rawArguments = new Dictionary<string, string>();

        foreach (var rawArgument in modelArguments.RootElement.EnumerateObject())
        {
            var simplifiedArgumentName = rawArgument.Name.Replace("_", "").ToLowerInvariant();
            var rawArgumentValue = rawArgument.Value.GetRawText();

            rawArguments.Add(simplifiedArgumentName, rawArgumentValue);
        }

        var deserializedArguments = new List<object?>();
        foreach (var parameter in function.Method.GetParameters())
        {
            if (parameter.ParameterType == typeof(CancellationToken))
            {
                deserializedArguments.Add(cancellationToken);
                continue;
            }

            var simplifiedParameterName = parameter.Name!.ToLowerInvariant();
            if (rawArguments.TryGetValue(simplifiedParameterName, out var rawArgumentValue))
            {
                try
                {
                    var jsonOptions = new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = new SnakeCaseNamingPolicy(),
                        Converters = { new SnakeCaseEnumConverter() }
                    };

                    var argumentValue = JsonSerializer.Deserialize(rawArgumentValue, parameter.ParameterType, jsonOptions);
                    deserializedArguments.Add(argumentValue);
                }
                catch
                {
                    return $"{{\"is_success\":false,\"error\":\"Argument does not match parameter type.\",\"parameter\":\"{parameter.Name.ToPascalCase()}\",\"type\":\"{parameter.ParameterType}\"}}";
                }
            }
            else if (parameter.IsOptional && parameter.DefaultValue != null)
            {
                deserializedArguments.Add(parameter.DefaultValue);
            }
            else
            {
                return $"{{\"is_success\":false,\"error\":\"Value is missing for required parameter.\",\"parameter\":\"{parameter.Name.ToSnakeCase()}\"}}";
            }
        }

        var invocationResult = function.DynamicInvoke(deserializedArguments.ToArray());
        if (invocationResult is Task task)
        {
            await task.ConfigureAwait(false);

            var taskResultProperty = task.GetType().GetProperty("Result");
            if (taskResultProperty != null)
            {
                invocationResult = taskResultProperty.GetValue(task);
            }
        }

        if (invocationResult == null)
        {
            return "{\"is_success\": true}";
        }

        return JsonSerializer.Serialize(invocationResult);
    }
}
