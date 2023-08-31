﻿using System.ComponentModel;
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
            { "type",  parameter.ParameterType.ToJsonType() }
        };

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

    private static string ToJsonType(this Type type)
    {
        if (type == typeof(bool))
        {
            return "boolean";
        }

        if (type == typeof(byte) || type == typeof(sbyte))
        {
            return "integer";
        }

        if (type == typeof(short) || type == typeof(ushort))
        {
            return "integer";
        }

        if (type == typeof(int) || type == typeof(nint) || type == typeof(long))
        {
            return "integer";
        }

        if (type == typeof(uint) || type == typeof(ulong))
        {
            return "integer";
        }

        if (type == typeof(float) || type == typeof(double) || type == typeof(decimal))
        {
            return "number";
        }

        if (type == typeof(char) || type == typeof(string))
        {
            return "string";
        }

        if (type == typeof(Guid))
        {
            return "string";
        }

        if (type == typeof(DateTime) || type == typeof(DateOnly) || type == typeof(TimeOnly) || type == typeof(TimeSpan))
        {
            return "string";
        }

        if (type == typeof(Uri))
        {
            return "string";
        }

        if (type.IsEnum)
        {
            return "string";
        }

        throw new NotSupportedException($"The parameter type '{type.Name}' is currently not supported.");
    }
}
