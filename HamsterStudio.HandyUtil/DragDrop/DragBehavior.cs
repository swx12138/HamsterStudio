using HamsterStudio.Toolkits.DragDrop;
using HandyControl.Interactivity;
using System.Windows;
using System.Windows.Input;

namespace HamsterStudio.HandyUtil.DragDrop;

public class DragBehavior : Behavior<FrameworkElement>
{
    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.PreviewMouseLeftButtonDown += AssociatedObject_PreviewMouseLeftButtonDown;
        AssociatedObject.PreviewMouseMove += AssociatedObject_PreviewMouseMove;
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();
        AssociatedObject.PreviewMouseLeftButtonDown -= AssociatedObject_PreviewMouseLeftButtonDown;
        AssociatedObject.PreviewMouseMove -= AssociatedObject_PreviewMouseMove;
    }

    private static Point? _dragStartPoint;

    private void AssociatedObject_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        _dragStartPoint = e.GetPosition(null);
    }

    private void AssociatedObject_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
    {
        if (e.LeftButton != MouseButtonState.Pressed || _dragStartPoint == null)
            return;

        var currentPosition = e.GetPosition(null);
        var diff = currentPosition - _dragStartPoint.Value;

        if (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
            Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
        {
            StartDrag(sender as UIElement);
            _dragStartPoint = null;
        }
    }

    private static void StartDrag(UIElement sourceElement)
    {
        if (sourceElement == null) return;

        var dataContext = (sourceElement as FrameworkElement)?.DataContext;
        if (dataContext == null) return;

        if (dataContext is not IDragable dragable)
        {
            return;
        }

        var dataObject = new DataObject();
        dataObject.SetData(dragable.DataFormat, dragable);

        // 关键：设置拖放效果为 Copy，这样不会移动原始元素
        System.Windows.DragDrop.DoDragDrop(sourceElement, dataObject, DragDropEffects.Copy);
    }

}
