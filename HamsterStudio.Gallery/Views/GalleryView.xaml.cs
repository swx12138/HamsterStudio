using HamsterStudio.Gallery.Models;
using HamsterStudio.Gallery.ViewModels;
using HamsterStudio.Toolkits;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace HamsterStudio.Gallery.Views
{
    /// <summary>
    /// GalleryView.xaml 的交互逻辑
    /// </summary>
    public partial class GalleryView : UserControl
    {
        public GalleryView()
        {
            InitializeComponent();
        }

        private CancellationTokenSource cancellationTokenSource = new();
        private static object lockobj = new();
        private BackgroundWorker backgroundWorker = new();

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue == null)
            {
                return;
            }

            if (e.OldValue == null)
            {
                Trace.TraceInformation($"Selected folder {(e.NewValue as GalleryFolderModel)!.DirInfo.FullName}");
            }
            else
            {
                Trace.TraceInformation($"Changed selected folder to {(e.NewValue as GalleryFolderModel)!.DirInfo.FullName} form {(e.OldValue as GalleryFolderModel)!.DirInfo.FullName} ");
            }

            if (DataContext is GalleryViewModel2 gvm2 && e.NewValue is GalleryFolderModel gf)
            {
                gvm2.ThumbnailModeViewModel.OnSelectedFolderChanged(gf);
                return;
            }
        }

    }
}
