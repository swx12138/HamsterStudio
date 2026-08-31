using System.IO;
using System.Windows;
using System.Windows.Media;

namespace HamsterStudio.HandyUtil;

/// <summary>图片查看工具：在新窗口中全屏展示图片（基于 HandyControl ImageViewer）。</summary>
public static class ImagePreview
{
    /// <summary>在新窗口中展示指定路径的图片。</summary>
    /// <param name="path">图片文件路径。</param>
    /// <param name="title">窗口标题，默认取文件名。</param>
    public static void Show(string path, string? title = null)
    {
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            return;

        var viewer = new HandyControl.Controls.ImageViewer
        {
            Uri = new Uri(path, UriKind.Absolute),
            ShowToolBar = true,
        };

        var window = new Window
        {
            Content = viewer,
            Title = title ?? Path.GetFileName(path),
            Width = 1280,
            Height = 720,
            WindowState = WindowState.Maximized,
            WindowStartupLocation = WindowStartupLocation.CenterScreen,
            //Background = Brushes.Black,
        };

        window.Show();
    }
}
