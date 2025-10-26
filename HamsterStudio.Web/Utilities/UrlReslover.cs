namespace HamsterStudio.Web.Utilities;

public static class UrlReslover
{
    public static Uri ResloveUrlProtocol(string url, string defaultProtocol = "https")
    {
        if (url.StartsWith("http://") || url.StartsWith("https://"))
        {
            return new Uri(url);
        }
        else if (url.StartsWith("//"))
        {
            return new($"{defaultProtocol}:{url}");
        }
        return new($"{defaultProtocol}://{url}");
    }
}
