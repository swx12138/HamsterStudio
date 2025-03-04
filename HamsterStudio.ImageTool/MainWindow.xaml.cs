using HamsterStudio.ImageTool.ViewModels;
using HamsterStudio.Toolkits.SysCall;
using System.Windows;
using System.Windows.Navigation;

namespace HamsterStudio.ImageTool;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void Window_Drop(object sender, DragEventArgs e)
    {
        if (DataContext is MainWindowModel viewModel)
        {
            if (e.Data.GetData(DataFormats.FileDrop) is string[] files)
            {
                viewModel.LoadFiles(files);
            }
        }

    }

    private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        ExplorerShell.SelectFile(e.Uri.AbsolutePath);
    }
}