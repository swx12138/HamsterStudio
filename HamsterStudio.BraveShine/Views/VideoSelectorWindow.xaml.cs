using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace HamsterStudio.BraveShine.Views
{
    /// <summary>
    /// VideoSelectorWindow.xaml 的交互逻辑
    /// </summary>
    public partial class VideoSelectorWindow : Window
    {
        public VideoSelectorWindow()
        {
            InitializeComponent();

        }

        public object Selected => mainView.SelectedItem;

        private DateTime lastClickTime = DateTime.UnixEpoch;

        private void Image_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var now = DateTime.Now;
            if (now - lastClickTime < TimeSpan.FromMilliseconds(200))
            {
                Close();
            }
            else
            {
                lastClickTime = now;
            }
        }

        private void UniformGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (sender is UniformGrid uniformGrid)
            {
                uniformGrid.InvalidateVisual();
                uniformGrid.UpdateLayout();
            }
        }
    }
}
