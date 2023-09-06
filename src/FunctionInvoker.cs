using FunctionalGPT.Properties;
using System.Text.Json;

namespace FunctionalGPT;

internal static class FunctionInvoker
{
    internal static async Task<string> InvokeAsync(Delegate function, string arguments, CancellationToken cancellationToken = default)
    {
        var argumentsDocument = JsonDocument.Parse(arguments);
        var argumentDictionary = new Dictionary<string, string>();

        foreach (var argument in argumentsDocument.RootElement.EnumerateObject())
        {
            var simplifiedName = argument.Name.Replace("_", "").ToLowerInvariant();
            var value = argument.Value.GetRawText();

            argumentDictionary.Add(simplifiedName, value);
        }

        var argumentList = new List<object?>();
        foreach (var parameter in function.Method.GetParameters())
        {
            if (parameter.ParameterType == typeof(CancellationToken))
            {
                argumentList.Add(cancellationToken);
                continue;
            }

            var simplifiedName = parameter.Name!.ToLowerInvariant();
            if (argumentDictionary.TryGetValue(simplifiedName, out var argument))
            {
                try
                {
                    var options = new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = new SnakeCaseNamingPolicy(),
                        Converters = { new SnakeCaseEnumConverter() }
                    };

                    var value = JsonSerializer.Deserialize(argument, parameter.ParameterType, options);
                    argumentList.Add(value);
                }
                catch
                {
                    return $"{{\"is_success\":false,\"error\":\"Argument does not match parameter type.\",\"parameter\":\"{parameter.Name.ToPascalCase()}\",\"type\":\"{parameter.ParameterType}\"}}";
                }
            }
            else if (parameter.IsOptional && parameter.DefaultValue != null)
            {
                argumentList.Add(parameter.DefaultValue);
            }
            else
            {
                return $"{{\"is_success\":false,\"error\":\"Value is missing for required parameter.\",\"parameter\":\"{parameter.Name.ToSnakeCase()}\"}}";
            }
        }

        var argumentArray = argumentList.ToArray();
        var invocationResult = function.DynamicInvoke(argumentArray);

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
