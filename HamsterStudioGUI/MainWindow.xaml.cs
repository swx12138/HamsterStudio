using HamsterStudio.Barefeet.Services;
using HamsterStudioGUI.Models;
using System.Windows;

namespace HamsterStudioGUI;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {

    }

    private void Window_Closed(object sender, EventArgs e)
    {
        if (DataContext is MainWindowModel model)
        {
            model.Dispose();
        }
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        return;
        var svc = App.ResloveService<DataStorageMgmt>();
        svc.BeforePersist += (sender, e) =>
        {
            var rect = new HamsterStudio.Barefeet.SysCall.Rect()
            {
                Left = (int)Left,
                Top = (int)Top,
                Right = (int)(Left + Width),
                Bottom = (int)(Top + Height)
            };
            svc.Set("window-position", rect);
        };

        var pos = svc.Get<HamsterStudio.Barefeet.SysCall.Rect>("window-position");
        if (pos.Left != pos.Right && pos.Top != pos.Bottom)
        {
            Left = pos.Left;
            Top = pos.Top;
            Width = pos.Right - pos.Left;
            Height = pos.Bottom - pos.Top;
        }

    }
}