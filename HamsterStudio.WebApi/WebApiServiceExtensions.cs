using HamsterStudio.RedBook;
using HamsterStudio.WebApi.Controllers;
using Microsoft.AspNetCore.Mvc.ApplicationParts;

namespace HamsterStudio.WebApi;

public static class WebApiServiceExtensions
{
    public static IServiceCollection AddWebApiServices(this IServiceCollection services)
    {
        // Add services to the container.
        services.AddRedBookWebApiServices();

        services.AddControllers()
            .ConfigureApplicationPartManager(apm =>
            {
                apm.ApplicationParts.Add(new AssemblyPart(typeof(RedBookController).Assembly));
                apm.ApplicationParts.Add(new AssemblyPart(typeof(TestController).Assembly));
            });// 添加特定的程序集
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        services.AddCors(options => options.AddPolicy("AllowAll", p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

        return services;
    }

    public static WebApplication ConfigureWebApi(this WebApplication app)
    {
        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        //app.UseDefaultFiles();
        //app.UseStaticFiles();

        //app.UseHttpsRedirection();
        app.UseCors("AllowAll"); // 启用 CORS

        app.UseAuthorization();

        app.MapControllers();

        return app;
    }

}
