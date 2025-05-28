
namespace HamsterStudio.WebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddWebApiServices();

            builder.Build()
                .ConfigureWebApi()
                .Run();
        }
    }
}
