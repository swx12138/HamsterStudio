namespace HamsterStudio.WebApi;

public class WebApiStartup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddWebApiServices();
    }

    public void Configure(IApplicationBuilder app)
    {
        var webApp = app as WebApplication;
        webApp?.ConfigureWebApi();
    }
}
