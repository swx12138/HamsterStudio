using CommunityToolkit.Mvvm.ComponentModel;
using HamsterStudio.Barefeet.Interfaces;
using HamsterStudio.Barefeet.Logging;
using HamsterStudio.Bilibili.Models;
using HamsterStudio.RedBook.Services;
using HamsterStudio.Toolkits.Logging;
using HamsterStudio.Web.Routing;
using HamsterStudio.Web.Routing.Routes;
using HamsterStudioGUI.Models;
using HamsterStudioGUI.ViewModels;
using HamsterStudioGUI.Views;
using NetCoreServer;
using System.Collections.ObjectModel;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;

namespace HamsterStudioGUI;

partial class MainWindowModel : ObservableObject
{
    [ObservableProperty]
    private string _title = "Hamster Studio GUI";

    [ObservableProperty]
    private string _description = "Hamster Studio GUI is a desktop application for Hamster Studio.";

    [ObservableProperty]
    private bool _topmost = false;

    public ObservableCollectionTarget NlogTarget { get; } = new("App");

    public MainViewModel MainViewModel { get; } = new();

    public MainWindowModel()
    {
        Logger.Shared.AddTarget(NlogTarget, NLog.LogLevel.Info, NLog.LogLevel.Fatal);
    }

}
