using CommunityToolkit.Mvvm.ComponentModel;
using HamsterStudio.ChatBox.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace HamsterStudio.ChatBox
{
    public class MainWindowModel : ObservableObject
    {
        public ChatBoxMainViewModel MainViewModel { get; }

        public MainWindowModel()
        {
            MainViewModel = App.ServiceProvider!.GetRequiredService<ChatBoxMainViewModel>();
        }
    }
}
