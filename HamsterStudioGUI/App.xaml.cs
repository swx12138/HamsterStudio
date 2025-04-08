using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Windows;
using HamsterStudio.Barefeet.Interfaces;

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

}

