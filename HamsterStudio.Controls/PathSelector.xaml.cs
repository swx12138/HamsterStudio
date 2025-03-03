using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;

namespace HamsterStudio.Controls
{
    /// <summary>
    /// PathSelector.xaml 的交互逻辑
    /// </summary>
    public partial class PathSelector : UserControl
    {
        #region Path
        public string Path
        {
            get { return (string)GetValue(PathProperty); }
            set { SetValue(PathProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Path.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PathProperty =
            DependencyProperty.Register("Path", typeof(string), typeof(PathSelector), new PropertyMetadata(string.Empty, PropertyChangedCallback));

        #endregion

        #region PathOnly
        public bool PathOnly
        {
            get { return (bool)GetValue(PathOnlyProperty); }
            set { SetValue(PathOnlyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PathOnly.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PathOnlyProperty =
            DependencyProperty.Register("PathOnly", typeof(bool), typeof(PathSelector), new PropertyMetadata(false, PropertyChangedCallback));

        #endregion

        private static void PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PathSelector selector)
            {
                if (e.Property == PathProperty)
                {
                    selector.tbPath.Text = e.NewValue as string;
                }
            }
        }

        public PathSelector()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (PathOnly)
            {
                OpenFolderDialog dialog = new();
                dialog.InitialDirectory = Path;
                dialog.Multiselect = false;
                if (dialog.ShowDialog() ?? false)
                {
                    Path = dialog.FolderName;
                }
            }
            else
            {
                OpenFileDialog dialog = new();
                dialog.InitialDirectory = Path;
                dialog.Multiselect = false;
                if (dialog.ShowDialog() ?? false)
                {
                    Path = dialog.FileName;
                }
            }
        }

        private void tbPath_TextChanged(object sender, TextChangedEventArgs e)
        {
            Path = tbPath.Text;
        }
    }
}
