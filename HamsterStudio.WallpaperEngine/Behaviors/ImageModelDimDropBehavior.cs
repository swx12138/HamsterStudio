using HamsterStudio.HandyUtil.DragDrop;
using HamsterStudio.WallpaperEngine.Services;
using System.Windows;

namespace HamsterStudio.WallpaperEngine.Behaviors;

public class ImageModelDimDropBehavior : DragDropBehavior
{    
    protected override void OnAttached()
    {
        base.OnAttached();
        AcceptDataFormat = nameof(ImageModelDim);
    }

    protected override void OnDrop(object sender, DragEventArgs e)
    {
        if (AssociatedObject.DataContext is DesktopWallpaperInfo dwi)
        {
            e.Handled = true;
            var data = (ImageModelDim)e.Data.GetData(dwi.AcceptDataFormat) ?? null;
            if (data == null) return;
            dwi.Drop(data);
        }
        else
        {
            base.OnDrop(sender, e);
        }
    }
}
