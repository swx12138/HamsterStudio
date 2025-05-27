using HamsterStudio.Barefeet.Interfaces;
using HamsterStudio.WebApi;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace HamsterStudioGUI;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application, IHamsterApp
{
    public string FileStorageHome { get; set; } = @"D:\HamsterStudioHome";

    public App()
    {
        //AppDomain.CurrentDomain.FirstChanceException += (sender, e) =>
        //{
        //    Debug.WriteLine($"异常捕获: {e.Exception}");
        //};
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        Profile.Run();
    }

}

