using Newtonsoft.Json.Linq;

namespace PTTGC.Prat.Common;

public static class Util
{
    public static bool Get(this IDictionary<string, bool> dict, string key)
    {
        if (dict.ContainsKey(key))
        {
            return dict[key];
        }

        return false;
    }

    public static string Get(this IDictionary<string, string> dict, string key)
    {
        if (dict.ContainsKey(key))
        {
            return dict[key];
        }

        return string.Empty;
    }

    public static JObject FixJson( this string prompResponse)
    {
        prompResponse = prompResponse.Trim();

        if (prompResponse.StartsWith("```json"))
        {
            prompResponse = prompResponse.Substring("```json".Length);
        }

        if (prompResponse.IndexOf("```") > 0)
        {
            prompResponse = prompResponse.Substring(0, prompResponse.IndexOf("```"));
        }

        prompResponse = prompResponse.Trim();

        return JObject.Parse(prompResponse);
    }

    public static float? TryGetFloat(this JObject input, string key)
    {
        if ( input.ContainsKey(key) == false )
        {
            return null;
        }

        if (input[key].Type == JTokenType.Float)
        {
            return (float)input[key];
        }

        if (input[key].Type == JTokenType.String)
        {
            float output = 0;
            var value = (string)input[key];

            if (float.TryParse(value, out output))
            {
                return output;
            }
        }

        return null;
    }

    public static bool? TryGetBool(this JObject input, string key)
    {
        if (input.ContainsKey(key) == false)
        {
            return null;
        }

        if (input[key].Type == JTokenType.Boolean)
        {
            return (bool)input[key];
        }

        if (input[key].Type == JTokenType.Float)
        {
            return ((float)input[key]) != 0;
        }

        if (input[key].Type == JTokenType.String)
        {
            var value = (string)input[key];
            return value.Trim().ToLowerInvariant() == "true";
        }

        return null;
    }

    public static string TryGetString(this JObject input, string key)
    {
        if (input.ContainsKey(key) == false)
        {
            return null;
        }

        if (input[key].Type == JTokenType.String)
        {
            return (string)input[key];
        }

        return null;
    }

    public record TitleDescription( string title, string description);
}
