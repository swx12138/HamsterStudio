using HamsterStudio.Bilibili.Models.Sub;
using HamsterStudioGUI.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace HamsterStudioGUI.Views
{
    /// <summary>
    /// MainView.xaml 的交互逻辑
    /// </summary>
    public partial class MainView : UserControl
    {
        public MainView()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is BraveShineModel viewModel)
            {
                if (sender is Button button && button.DataContext is WatchLaterDat dat)
                {
                    viewModel.RedirectCommand.Execute(dat.Bvid);
                }
            }
        }

    }
}
