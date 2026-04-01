using HamsterStudio.Barefeet.FileSystem;
using HamsterStudio.ImageTool.ViewModels;
using HandyControl.Controls;
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
    /// ImageStitcherView.xaml 的交互逻辑
    /// </summary>
    public partial class ImageStitcherView : UserControl
    {
        public ImageStitcherView()
        {
            InitializeComponent();
        }

        private void ImageSelector_ImageSelected(object sender, RoutedEventArgs e)
        {
            //if(sender is ImageSelector imageSelector && imageSelector.DataContext is HamstertFileInfo fileInfo)
            //{
            //    fileInfo.PlacementNew(imageSelector.Uri.AbsolutePath);
            //}
            return;
        }
    }
}
