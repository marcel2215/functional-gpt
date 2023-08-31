using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.Json.Nodes;

namespace FunctionalGPT;

internal static class FunctionSerializer
{
    internal static JsonArray Serialize(IEnumerable<Delegate> functions)
    {
        var functionsArray = new JsonArray();
        foreach (var function in functions)
        {
            var functionObject = Serialize(function);
            functionsArray.Add(functionObject);
        }

        return functionsArray;
    }

    private static JsonObject Serialize(Delegate function)
    {
        var propertiesObject = new JsonObject();
        var requiredArray = new JsonArray();

        foreach (var parameter in function.Method.GetParameters())
        {
            if (parameter.ParameterType == typeof(CancellationToken))
            {
                continue;
            }

            var parameterName = parameter.Name!.ToSnakeCase();
            var propertyObject = SerializeParameter(parameter);

            propertiesObject.Add(parameterName, propertyObject);

            if (!parameter.IsOptional || IsRequired(parameter))
            {
                requiredArray.Add(parameterName);
            }
        }

        var parametersObject = new JsonObject
        {
            { "type", "object" },
            { "properties", propertiesObject },
        };

        if (requiredArray.Any())
        {
            parametersObject.Add("required", requiredArray);
        }

        var functionObject = new JsonObject
        {
            { "name", function.Method.Name.ToSnakeCase() },
            { "parameters", parametersObject }
        };

        var description = GetDescription(function.Method);
        if (!string.IsNullOrEmpty(description))
        {
            functionObject.Add("description", description);
        }

        return functionObject;
    }

    private static JsonObject SerializeParameter(ParameterInfo parameter)
    {
        var propertyObject = new JsonObject
        {
            { "type",  parameter.ParameterType.ToJsonType(out var format, out var minValue, out var maxValue) }
        };

        if (IsEmailAddress(parameter))
        {
            propertyObject.Add("format", "email");
        }
        else if (format != null)
        {
            propertyObject.Add("format", format);
        }

        if (minValue != null)
        {
            propertyObject.Add("minimum", minValue);
        }

        if (maxValue != null)
        {
            propertyObject.Add("maximum", maxValue);
        }

        var (minLength, maxLength) = GetLengthConstraints(parameter);

        if (minLength != null)
        {
            propertyObject.Add("minLength", minLength);
        }

        if (maxLength != null)
        {
            propertyObject.Add("maxLength", maxLength);
        }

        if (parameter.ParameterType.IsEnum)
        {
            var membersArray = new JsonArray();
            foreach (var enumMember in Enum.GetNames(parameter.ParameterType))
            {
                membersArray.Add(enumMember.ToSnakeCase());
            }

            propertyObject.Add("enum", membersArray);
        }

        var description = GetDescription(parameter);
        if (!string.IsNullOrEmpty(description))
        {
            propertyObject.Add("description", description);
        }

        if (parameter.IsOptional && parameter.DefaultValue != null)
        {
            propertyObject.Add("default", parameter.DefaultValue.ToString());
        }

        return propertyObject;
    }

    private static bool IsRequired(ParameterInfo parameter)
    {
        var attribute = parameter.GetCustomAttribute<RequiredAttribute>();
        return attribute != null;
    }

    private static bool IsEmailAddress(ParameterInfo parameter)
    {
        var attribute = parameter.GetCustomAttribute<EmailAddressAttribute>();
        return attribute != null;
    }

    private static (int? Min, int? Max) GetLengthConstraints(ParameterInfo parameter)
    {
        int? min = null, max = null;

        var minLengthAttribute = parameter.GetCustomAttribute<MinLengthAttribute>();
        if (minLengthAttribute != null)
        {
            min = minLengthAttribute.Length;
        }

        var maxLengthAttribute = parameter.GetCustomAttribute<MaxLengthAttribute>();
        if (maxLengthAttribute != null)
        {
            max = maxLengthAttribute.Length;
        }

        return (min, max);
    }

    private static string? GetDescription(MemberInfo member)
    {
        var attribute = member.GetCustomAttribute<DescriptionAttribute>();
        return attribute?.Description;
    }

    private static string? GetDescription(ParameterInfo parameter)
    {
        var attribute = parameter.GetCustomAttribute<DescriptionAttribute>();
        return attribute?.Description;
    }

    private static string ToJsonType(this Type type, out string? format, out long? min, out long? max)
    {
        format = null;
        min = null;
        max = null;

        if (type == typeof(bool))
        {
            return "boolean";
        }

        if (type == typeof(byte))
        {
            min = byte.MinValue;
            max = byte.MaxValue;

            return "integer";
        }

        if (type == typeof(sbyte))
        {
            min = sbyte.MinValue;
            max = sbyte.MaxValue;

            return "integer";
        }

        if (type == typeof(short))
        {
            min = short.MinValue;
            max = short.MaxValue;

            return "integer";
        }

        if (type == typeof(ushort))
        {
            min = ushort.MinValue;
            max = ushort.MaxValue;

            return "integer";
        }

        if (type == typeof(int) || type == typeof(nint) || type == typeof(long))
        {
            return "integer";
        }

        if (type == typeof(uint) || type == typeof(ulong))
        {
            min = 0;
            return "integer";
        }

        if (type == typeof(float) || type == typeof(double) || type == typeof(decimal))
        {
            return "number";
        }

        if (type == typeof(char))
        {
            min = 1;
            max = 1;

            return "string";
        }

        if (type == typeof(string) || type.IsEnum)
        {
            return "string";
        }

        if (type == typeof(Guid))
        {
            format = "uuid";
            return "string";
        }

        if (type == typeof(DateTime))
        {
            format = "date-time";
            return "string";
        }

        if (type == typeof(DateOnly))
        {
            format = "date";
            return "string";
        }

        if (type == typeof(TimeOnly))
        {
            format = "time";
            return "string";
        }

        if (type == typeof(TimeSpan))
        {
            format = "duration";
            return "string";
        }

        if (type == typeof(Uri))
        {
            format = "uri";
            return "string";
        }

        throw new NotSupportedException($"The parameter type '{type.Name}' is currently not supported.");
    }
}
