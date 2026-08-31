using HamsterStudio.HandyUtil.DragDrop;
using HamsterStudio.Photogrammetry.ViewModels;
using System.Windows;

namespace HamsterStudio.Photogrammetry.Behaviors;

/// <summary>
/// 将拖放的文件交给 <see cref="ReferenceProjectViewModel"/> 处理。
/// </summary>
public class ReferenceProjectDropBehavior : DragDropBehavior
{
    protected override void OnDrop(object sender, DragEventArgs e)
    {
        if (AssociatedObject.DataContext is ReferenceProjectViewModel viewModel)
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop) ?? [];
            viewModel.DropFiles(files);
            e.Handled = true;
            return;
        }

        base.OnDrop(sender, e);
    }
}
