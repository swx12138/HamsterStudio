using HamsterStudio.Barefeet.Logging;
using HamsterStudio.HandyUtil.DragDrop;
using HamsterStudio.WallpaperEngine.Services;
using System.Windows;

namespace HamsterStudio.WallpaperEngine.Behaviors;

public class ImageModelDimDropBehavior : DragDropBehavior
{
    protected override void OnAttached()
    {
        base.OnAttached();
    }

    protected override void OnDrop(object sender, DragEventArgs e)
    {
        if (AssociatedObject.DataContext is DesktopWallpaperInfo dwi)
        {
            e.Handled = true;
            foreach (var format in dwi.AcceptDataFormat)
            {
                try
                {
                    var data = e.Data.GetData(format) ?? null;
                    if (data == null) continue;
                    dwi.Drop(data);
                } catch (Exception ex)
                {
                    Logger.Shared.Warning($"Drop data format {format} failed: {ex.Message}");
                }
            }
        }
        else
        {
            base.OnDrop(sender, e);
        }
    }
}
