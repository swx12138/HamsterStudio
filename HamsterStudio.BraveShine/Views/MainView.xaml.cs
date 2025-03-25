using HamsterStudio.BraveShine.Modelss.Bilibili.SubStruct;
using HamsterStudio.BraveShine.ViewModels;
using NLog;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace HamsterStudio.BraveShine.Views
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
            if (DataContext is MainViewModel viewModel)
            {
                if (sender is Button button && button.DataContext is WatchLaterDat dat)
                {
                    viewModel.RedirectCommand.Execute(dat.Bvid);
                }
            }
        }


        private void DataGrid_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is DataGrid dg && dg.DataContext is ObservableCollection<LogEventInfo> data)
            {
                dg.ScrollIntoView(data.Last());
            }
        }
    }
}
