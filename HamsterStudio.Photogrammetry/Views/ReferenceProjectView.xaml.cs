using System.Windows;
using System.Windows.Controls;

namespace HamsterStudio.Photogrammetry.Views
{
    /// <summary>
    /// ReferenceProjectView.xaml 的交互逻辑
    /// </summary>
    public partial class ReferenceProjectView : UserControl
    {
        public ReferenceProjectView()
        {
            InitializeComponent();
        }

        private void ResetViewer_Click(object sender, RoutedEventArgs e)
        {
            if (Viewer.Uri is not { } uri)
            {
                return;
            }

            // 使用 SetCurrentValue 而非直接赋值，
            // 以免清掉 XAML 中 Uri 的绑定（否则切换选中项后预览不再更新）。
            Viewer.SetCurrentValue(HandyControl.Controls.ImageViewer.UriProperty, null);
            Viewer.SetCurrentValue(HandyControl.Controls.ImageViewer.UriProperty, uri);
        }
    }
}
