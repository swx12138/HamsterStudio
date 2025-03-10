using HamsterStudio.ImageTool.ViewModels;
using HamsterStudio.Toolkits.SysCall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HamsterStudio.ImageTool.Views
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

        private void Window_Drop(object sender, DragEventArgs e)
        {
            if (DataContext is MainViewModel viewModel)
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
}
