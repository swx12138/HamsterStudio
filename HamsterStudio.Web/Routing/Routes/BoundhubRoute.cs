using HamsterStudio.Barefeet.Task;
using NetCoreServer;

namespace HamsterStudio.Web.Routing.Routes;

internal class BoundhubRoute(HamsterTaskManager taskManager) : IRoute
{

    public bool IsMyCake(string url)
    {
        return url.Equals("/boundhub", StringComparison.CurrentCultureIgnoreCase);
    }

    //public void Response(HttpListenerRequest request, ref HttpListenerResponse response)
    //{
    //    /*
    //     * [{"uri":"","title":"","cookie":""},...]
    //     */
    //    StreamReader stream = new(request.InputStream);
    //    string raw = stream.ReadToEnd();
    //    var descriptors = JsonSerializer.Deserialize<List<VideoDescriptor>>(raw);
    //    if(descriptors == null || descriptors.Count == 0)
    //    {
    //        response.FromPlain("Failed.");
    //        return;
    //    }

    //    foreach (var descriptor in descriptors)
    //    {
    //        var task = new BoundhubVideoTask(descriptor);
    //        taskManager.EnqueueTask(task);
    //    }

    //    response.FromJson(JsonSerializer.Serialize(new
    //    {
    //        code = 0,
    //        message = "succeed",
    //        total = descriptors.Count,
    //    }));
    //}

    public HttpResponse GetHttpResponse(HttpRequest request)
    {
        return new HttpResponse().MakeOkResponse();
    }
}
