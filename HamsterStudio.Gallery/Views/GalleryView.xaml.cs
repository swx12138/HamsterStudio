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
                gvm2.OnSelectedFolderChanged(gf);
                return;

                cancellationTokenSource.Cancel();
                cancellationTokenSource = new CancellationTokenSource();
                backgroundWorker = new BackgroundWorker() { WorkerSupportsCancellation = true };
                backgroundWorker.DoWork += (sender, e) =>
                {
                    lock (lockobj)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            gvm2.CurrentImages.Clear();
                        });

                        foreach (var file in gf.Files)
                        {
                            if (e.Cancel)
                            {
                                break;
                            }

                            var imgSrc = ImageUtils.LoadImageSource(file.FullName, 400);
                            if (imgSrc == null)
                            {
                                Trace.TraceError($"Load image {file.Name} failed!!!");
                                continue;
                            }

                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                gvm2.CurrentImages.Add(imgSrc);
                            });
                        }
                    }
                };
                backgroundWorker.RunWorkerAsync(cancellationTokenSource.Token);
            }
        }

        private void Pagination_PageUpdated(object sender, HandyControl.Data.FunctionEventArgs<int> e)
        {
            if (DataContext is GalleryViewModel2 gvm2)
            {
                gvm2.OnPageIndexChanged();
            }
        }
    }
}
