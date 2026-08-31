using HamsterStudio.ChatBox.Services;
using HamsterStudio.ChatBox.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;

namespace HamsterStudio.ChatBox
{
    public static class ChatBoxProfile
    {
        public static void RegisterServices(IServiceCollection services)
        {
            services.AddSingleton(new HttpClient());
            services.AddSingleton<ChatService>();
            services.AddSingleton<ChatSessionService>();
            services.AddSingleton<ChatBoxMainViewModel>();
        }
    }
}
