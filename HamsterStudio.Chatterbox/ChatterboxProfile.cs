using HamsterStudio.Chatterbox.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HamsterStudio.Chatterbox;

public static class ChatterboxProfile
{
    public static void RegisterService(IServiceCollection services)
    {
        services.AddTransient<ChatSessionViewModel>();
    }

}
