using HamsterStudio.BraveShine.Modelss.Bilibili.SubStruct;
using HamsterStudio.BraveShine.ViewModels;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HamsterStudio.BraveShine;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel viewModel)
        {
            if (sender is Button button && button.DataContext is WatchLaterDat dat)
            {
                viewModel.RedirectCommand.Execute(dat.Bvid);
            }
        }
    }

    private void locat_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DataContext is MainViewModel viewModel)
        {
            viewModel.RedirectLocationCommand.Execute(null);
        }
    }

}