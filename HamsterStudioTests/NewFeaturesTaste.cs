using System.Runtime.InteropServices;

// based off of https://bitbucket.org/ciniml/desktopwallpaper
[ComImport]
[Guid("B92B56A9-8B55-4E14-9A89-0199BBB6F93B")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
public interface IDesktopWallpaper
{
    void SetWallpaper([MarshalAs(UnmanagedType.LPWStr)] string monitorID, [MarshalAs(UnmanagedType.LPWStr)] string wallpaper);

    [return: MarshalAs(UnmanagedType.LPWStr)]
    string GetWallpaper([MarshalAs(UnmanagedType.LPWStr)] string monitorID);

    [return: MarshalAs(UnmanagedType.LPWStr)]
    string GetMonitorDevicePathAt(uint monitorIndex);

    [return: MarshalAs(UnmanagedType.U4)]
    uint GetMonitorDevicePathCount();

    [return: MarshalAs(UnmanagedType.Struct)]
    Rect GetMonitorRECT([MarshalAs(UnmanagedType.LPWStr)] string monitorID);

    void SetBackgroundColor([MarshalAs(UnmanagedType.U4)] uint color);

    [return: MarshalAs(UnmanagedType.U4)]
    uint GetBackgroundColor();

    void SetPosition([MarshalAs(UnmanagedType.I4)] DesktopWallpaperPosition position);

    [return: MarshalAs(UnmanagedType.I4)]
    DesktopWallpaperPosition GetPosition();

    void SetSlideshow(IntPtr items);

    IntPtr GetSlideshow();

    void SetSlideshowOptions(DesktopSlideshowDirection options, uint slideshowTick);

    void GetSlideshowOptions(out DesktopSlideshowDirection options, out uint slideshowTick);

    void AdvanceSlideshow([MarshalAs(UnmanagedType.LPWStr)] string monitorID, [MarshalAs(UnmanagedType.I4)] DesktopSlideshowDirection direction);

    DesktopSlideshowDirection GetStatus();

    bool Enable();
}

[ComImport]
[Guid("C2CF3110-460E-4fc1-B9D0-8A1C0C9CC4BD")]
public class DesktopWallpaper
{
}

[Flags]
public enum DesktopSlideshowOptions
{
    None = 0,
    ShuffleImages = 0x01
}

[Flags]
public enum DesktopSlideshowState
{
    None = 0,
    Enabled = 0x01,
    Slideshow = 0x02,
    DisabledByRemoteSession = 0x04
}

public enum DesktopSlideshowDirection
{
    Forward = 0,
    Backward = 1
}

public enum DesktopWallpaperPosition
{
    Center = 0,
    Tile = 1,
    Stretch = 2,
    Fit = 3,
    Fill = 4,
    Span = 5,
}

[StructLayout(LayoutKind.Sequential)]
public struct Rect
{
    public int Left;
    public int Top;
    public int Right;
    public int Bottom;
}


namespace HamsterStudioTests
{

    [TestClass]
    public class NewFeaturesTaste
    {
        private IDesktopWallpaper Wallpaper = (IDesktopWallpaper)new DesktopWallpaper();

        /// <summary>
        /// 为特定显示器设置壁纸
        /// </summary>
        /// <param name="monitorIndex">显示器索引（从0开始）</param>
        /// <param name="filePath">图片路径</param>
        /// <returns>是否成功</returns>
        public bool SetWallpaperForMonitor(uint monitorIndex, string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !System.IO.File.Exists(filePath))
                return false;

            try
            {
                uint monitorCount = Wallpaper.GetMonitorDevicePathCount();
                for (uint i = 0; i < monitorCount; i++)
                {
                    var m = Wallpaper.GetMonitorDevicePathAt(i);
                    Console.WriteLine(m);
                    Console.WriteLine(Wallpaper.GetWallpaper(m));
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"设置多显示器壁纸失败: {ex.Message}");
                return false;
            }
        }


        [TestMethod]
        public void MyTestMethod()
        {
            SetWallpaperForMonitor(0, @"E:\Pictures\bizhi\Concat_56415153_uid_16461231.png");
        }
    }
}
