using CommunityToolkit.Mvvm.Input;
using HamsterStudio.Barefeet.Logging;
using HamsterStudio.Barefeet.MVVM;
using HamsterStudio.Gallery.Views;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace HamsterStudio.Gallery.Models;

public partial class FileModel : KnownViewModel
{
    public string Filename { get; }

    public Lazy<ImageSource?> Thumbnail { get; }

    public ICommand ShowImageCommand { get; }

    public FileModel(string filename)
    {
        Filename = filename;
        DisplayName = Path.GetFileName(filename);
        ShowImageCommand = new RelayCommand(() =>
        {
            var imageBrowser = new HandyControl.Controls.ImageBrowser(new Uri(filename));
            imageBrowser.ShowDialog();
        });

        _ = new RelayCommand(() =>
        {
            OpenCvSharp.Cv2.NamedWindow(DisplayName, OpenCvSharp.WindowFlags.AutoSize);
            OpenCvSharp.Cv2.SetWindowProperty(DisplayName, OpenCvSharp.WindowPropertyFlags.Topmost, 1);
            var img = OpenCvSharp.Cv2.ImRead(filename);
            double scale = 1.0;
            OpenCvSharp.Cv2.ImShow(DisplayName, img);
            OpenCvSharp.Cv2.SetMouseCallback(DisplayName, (ev, x, y, flags, ud) =>
            {
                if (ev == OpenCvSharp.MouseEventTypes.MouseWheel)
                {
                    if (OpenCvSharp.Cv2.GetMouseWheelDelta(flags) > 0)
                    {
                        scale += 0.1;
                    }
                    else
                    {
                        scale -= 0.1;
                    }

                    var newWidth = img.Width * scale;
                    if (newWidth < Constants.ThumbnailSize)
                    { return; }

                    var newHeight = img.Height * scale;
                    if (newHeight < Constants.ThumbnailSize)
                    { return; }

                    var mat = new OpenCvSharp.Mat();
                    OpenCvSharp.Cv2.Resize(img, mat, new OpenCvSharp.Size(newWidth, newHeight));
                    OpenCvSharp.Cv2.ImShow(DisplayName, mat);
                }
            });
        });
        Thumbnail = new Lazy<ImageSource?>(() =>
        {
            try
            {
                BitmapImage bitmap = new();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(Filename);
                bitmap.DecodePixelWidth = (int)Constants.ThumbnailSize;
                bitmap.EndInit();
                bitmap.Freeze();
                return bitmap;
            }
            catch (Exception ex)
            {
                Logger.Shared.Critical(ex);
                return null;
            }
        });
    }

}

public partial class FileGroupModel : KnownViewModel
{
    public string GroupName { get; }

    public ObservableCollection<FileModel> Files { get; } = [];

    public Lazy<ImageSource?> Thumbnail { get; }

    public ICommand ViewCommand { get; }

    public FileGroupModel(string groupName)
    {
        DisplayName = Path.GetFileName(groupName);
        GroupName = groupName;
        ViewCommand = new RelayCommand(() =>
        {
            FileGroupWindow window = new();
            window.DataContext = this;
            _ = window.ShowDialog();
        });
        Thumbnail = new Lazy<ImageSource?>(() =>
        {
            if (Files.Count == 0)
            {
                return null;
            }
            return Files.First().Thumbnail.Value;
        });
    }

}
