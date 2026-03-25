using HamsterStudio.Gallery.ViewModels;
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

namespace HamsterStudio.Gallery.Views
{
    /// <summary>
    /// ThumbnailModeView.xaml 的交互逻辑
    /// </summary>
    public partial class ThumbnailModeView : UserControl
    {
        public ThumbnailModeView()
        {
            InitializeComponent();
        }

        private void Pagination_PageUpdated(object sender, HandyControl.Data.FunctionEventArgs<int> e)
        {
            if (DataContext is ThumbnailModeViewModel vm)
            {
                vm.OnPageIndexChanged();
            }
        }

        private void TextBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (sender is TextBox tb && tb.Parent is FrameworkElement fe)
            {
                fe.Focus();
            }
        }
    }
}
