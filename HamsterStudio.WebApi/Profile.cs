using HamsterStudio.WebApi.Controllers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.DependencyInjection;

namespace HamsterStudio.WebApi
{
    public static class Profile
    {
        public static void Run()
        {
            var builder = WebApplication.CreateBuilder();

            builder.Services.AddControllers()
                .ConfigureApplicationPartManager(apm =>
            {
                // 移除默认的应用程序集
                apm.ApplicationParts.Clear();

                // 添加特定的程序集
                apm.ApplicationParts.Add(new AssemblyPart(typeof(TestController).Assembly));

            });                  // 添加控制器
            builder.WebHost.UseUrls("http://localhost:5000");   // 更改监听地址

            var app = builder.Build();

            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.MapControllers();

            app.RunAsync();
        }
    }
}
