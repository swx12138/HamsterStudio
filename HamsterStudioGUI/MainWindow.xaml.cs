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

}