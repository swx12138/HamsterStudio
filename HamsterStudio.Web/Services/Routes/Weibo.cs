﻿using HamsterStudio.Web.Interfaces;
using HamsterStudio.Web.Request;
using HamsterStudio.Web.Services;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace HamsterStudio.Web.Services.Routes
{
    internal class Weibo : IRoute
    {
        public bool IsMyCake(string url)
        {
            return url.IndexOf("weibo") != -1;
        }

        public void Response(HttpListenerRequest request, ref HttpListenerResponse response)
        {
            using StreamReader sr = new(request.InputStream);
            var obj = JsonSerializer.Deserialize<JsonNode>(sr.ReadToEnd())!;

            var id = obj["id"]!;
            var urls = obj["urls"]!.AsArray()!.Select(x => x?.GetValue<string>());
            Log($"{id}[{urls.Count()}]");

            var names = Task.WhenAll(urls.Select(async (x, idx) =>
            {
                var name = $"{id}_{idx}_mys_{x?.Split("/").Last()}";
                await FileSaver.SaveFileFromUrl(x!, "miyoushe", name);
                Log($" - {name}");
                return name;
            })).Result.ToList();

            response.FromJson(JsonSerializer.Serialize(new
            {
                code = 0,
                message = "succeed",
                total = names.Count,
            }));
        }

        static void Log<T>(T msg)
        {
            Console.WriteLine(msg);
        }
    }
}
