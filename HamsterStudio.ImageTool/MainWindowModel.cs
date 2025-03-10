using CommunityToolkit.Mvvm.ComponentModel;
using HamsterStudio.ImageTool.ViewModels;
using HamsterStudio.ImageTool.Views;

namespace HamsterStudio.ImageTool;

partial class MainWindowModel : ObservableObject
{
    public MainView MainView { get; set; } = new();

    public MainViewModel MainViewModel => MainView.DataContext as MainViewModel;
   

}
