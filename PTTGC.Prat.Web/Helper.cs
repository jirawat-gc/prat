namespace PTTGC.Prat.Web;

public static class Helper
{
    public static string ShowIf( bool expression, string whenFalse = "" )
    {
        return expression ? "show" : whenFalse;
    }

    /// <summary>
    /// Alias to "ShowIf" - making class 'collapsible, fading, switchable, disabling' activate
    /// </summary>
    /// <param name="expression"></param>
    /// <param name="whenFalse"></param>
    /// <returns></returns>
    public static string ActiveIf(bool expression, string whenFalse = "")
    {
        return expression ? "show" : whenFalse;
    }

    public static string HideIf(bool expression, string whenFalse = "")
    {
        return expression ? "hide" : whenFalse;
    }
}
