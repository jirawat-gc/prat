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

    public record TitleDescription( string title, string description);
}
