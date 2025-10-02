using HamsterStudio.Toolkits.DragDrop;
using HandyControl.Interactivity;
using System.Windows;

namespace HamsterStudio.HandyUtil.DragDrop;

public class DragDropBehavior : Behavior<FrameworkElement>
{
    protected string AcceptDataFormat = DataFormats.FileDrop;

    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.AllowDrop = true;
        AssociatedObject.DragOver += OnDragOver;
        AssociatedObject.Drop += OnDrop;
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();
        AssociatedObject.DragOver -= OnDragOver;
        AssociatedObject.Drop -= OnDrop;
    }

    private void OnDragOver(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(AcceptDataFormat))
        {
            e.Effects = DragDropEffects.Copy;
        }
        else
        {
            e.Effects = DragDropEffects.None;
        }
        e.Handled = true;
    }

    protected virtual void OnDrop(object sender, DragEventArgs e)
    {
        if (AssociatedObject.DataContext is FileDroppableBase fileDroppable)
        {
            var data = (string[])e.Data.GetData(DataFormats.FileDrop) ?? [];
            fileDroppable.Drop(data);
            e.Handled = true;
        }
    }

}
