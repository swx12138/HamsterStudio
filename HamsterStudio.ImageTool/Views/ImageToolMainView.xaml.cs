using HamsterStudio.Barefeet.SysCall;
using HamsterStudio.ImageTool.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace HamsterStudio.ImageTool.Views
{
    /// <summary>
    /// MainView.xaml 的交互逻辑
    /// </summary>
    public partial class ImageToolMainView : UserControl
    {
        public ImageToolMainView()
        {
            InitializeComponent();
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            if (DataContext is ImageToolMainViewModel viewModel)
            {
                if (e.Data.GetData(DataFormats.FileDrop) is string[] files)
                {
                    viewModel.LoadFiles(files);
                }
            }

        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            ShellApi.SelectFile(e.Uri.AbsolutePath);
        }
    }
}
