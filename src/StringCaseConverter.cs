using System.Text;

namespace FunctionalGPT;

internal static class StringCaseConverter
{
    internal static string ToSnakeCase(this string value)
    {
        var result = new StringBuilder();
        for (var i = 0; i < value.Length; i++)
        {
            var c = value[i];
            if (char.IsUpper(c))
            {
                if (i > 0 && value[i - 1] != '_')
                {
                    _ = result.Append('_');
                }

                _ = result.Append(char.ToLower(c));
            }
            else
            {
                _ = result.Append(c);
            }
        }

        return result.ToString();
    }
}
