using HamsterStudio.Web.DataModels.Boundhub;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Text.Json;

namespace HamsterStudio.Web.Services.Routes
{
    internal class BoundhubRouteCache
    {
        readonly string Filename = ".bhub";

        ConcurrentBag<VideoDescriptor> VideoDescriptors;

        public BoundhubRouteCache()
        {
            try
            {
                var fstream = File.OpenRead(Filename);
                VideoDescriptors = new(JsonSerializer.Deserialize<List<VideoDescriptor>>(fstream) ?? []);
            }
            catch (Exception ex) {
                Trace.TraceError($"[{nameof(BoundhubRouteCache)}] : {ex.Message}");
            }
        }

        ~BoundhubRouteCache()
        {

        }

        public void PushBack(VideoDescriptor videoDescriptor)
        {
            VideoDescriptors.Add(videoDescriptor);
        }

    }
}
