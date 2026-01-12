using CommunityToolkit.Mvvm.Input;
using HamsterStudio.Barefeet.MVVM;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Windows.Input;

namespace HamsterStudio.Gallery.Models;

public partial class FileViewModel : KnownViewModel
{
    public ICommand ShowImageCommand { get; }

    public FileViewModel(FileInfo fileInfo, ILogger<FileViewModel> logger) : base(logger)
    {
        DisplayName = fileInfo.Name;
        ShowImageCommand = new RelayCommand(() =>
        {
            var imageBrowser = new HandyControl.Controls.ImageBrowser(new Uri(fileInfo.FullName));
            imageBrowser.ShowDialog();
        });
        _ = new RelayCommand(() =>
        {
            OpenCvSharp.Cv2.NamedWindow(DisplayName, OpenCvSharp.WindowFlags.AutoSize);
            OpenCvSharp.Cv2.SetWindowProperty(DisplayName, OpenCvSharp.WindowPropertyFlags.Topmost, 1);
            var img = OpenCvSharp.Cv2.ImRead(fileInfo.FullName);
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
    }

}
