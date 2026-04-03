using HamsterStudio.Barefeet.MVVM;
using HamsterStudio.Chatterbox.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HamsterStudio.Chatterbox;

public class MainWindowModel : KnownViewModel
{
    public ChatSessionViewModel ChatSession { get; private set; }
    public MainWindowModel() : base(null)
    {
        DisplayName = "Chatterbox";

        var app = (App)(App.Current);
        logger = app.ServiceProvider.GetRequiredService<ILogger<MainWindowModel>>();
        ChatSession = app.ServiceProvider.GetRequiredService<ChatSessionViewModel>();

    }

}
