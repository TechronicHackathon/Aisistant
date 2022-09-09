public class Sessions
{

    public static string GetSessionID(HttpContext httpContext)
    {
        var sessionID = "123abd";
        if (httpContext != null && httpContext.Session.Id != null)
        {
            sessionID = httpContext.Session.Id;

        }
        return sessionID;
    }
}