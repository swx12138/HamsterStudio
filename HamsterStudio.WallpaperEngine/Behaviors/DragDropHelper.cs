using System.Windows;
using System.Windows.Input;

namespace HamsterStudio.WallpaperEngine.Behaviors;

public static class DragDropHelper
{
    #region 拖动源属性

    public static readonly DependencyProperty IsDragSourceProperty =
        DependencyProperty.RegisterAttached("IsDragSource", typeof(bool), typeof(DragDropHelper),
            new PropertyMetadata(false, OnIsDragSourceChanged));

    public static bool GetIsDragSource(DependencyObject obj) => (bool)obj.GetValue(IsDragSourceProperty);
    public static void SetIsDragSource(DependencyObject obj, bool value) => obj.SetValue(IsDragSourceProperty, value);

    private static void OnIsDragSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is UIElement element)
        {
            if ((bool)e.NewValue)
            {
                element.PreviewMouseLeftButtonDown += OnPreviewMouseLeftButtonDown;
                element.PreviewMouseMove += OnPreviewMouseMove;
            }
            else
            {
                element.PreviewMouseLeftButtonDown -= OnPreviewMouseLeftButtonDown;
                element.PreviewMouseMove -= OnPreviewMouseMove;
            }
        }
    }

    private static void OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _dragStartPoint = e.GetPosition(null);
    }

    private static void OnPreviewMouseMove(object sender, MouseEventArgs e)
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

    private static Point? _dragStartPoint;

    private static void StartDrag(UIElement sourceElement)
    {
        if (sourceElement == null) return;

        var dataContext = (sourceElement as FrameworkElement)?.DataContext;
        if (dataContext == null) return;

        var dataObject = new DataObject();
        dataObject.SetData("DataContext", dataContext);
        dataObject.SetData("SourceElement", sourceElement);

        // 关键：设置拖放效果为 Copy，这样不会移动原始元素
        DragDrop.DoDragDrop(sourceElement, dataObject, DragDropEffects.Copy);
    }

    #endregion

    #region 放置目标属性

    public static readonly DependencyProperty IsDropTargetProperty =
        DependencyProperty.RegisterAttached("IsDropTarget", typeof(bool), typeof(DragDropHelper),
            new PropertyMetadata(false, OnIsDropTargetChanged));

    public static bool GetIsDropTarget(DependencyObject obj) => (bool)obj.GetValue(IsDropTargetProperty);
    public static void SetIsDropTarget(DependencyObject obj, bool value) => obj.SetValue(IsDropTargetProperty, value);

    private static void OnIsDropTargetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is UIElement element)
        {
            element.AllowDrop = (bool)e.NewValue;
            if ((bool)e.NewValue)
            {
                element.DragOver += OnDragOver;
                element.Drop += OnDrop;
                element.DragEnter += OnDragEnter;
                element.DragLeave += OnDragLeave;
            }
            else
            {
                element.DragOver -= OnDragOver;
                element.Drop -= OnDrop;
                element.DragEnter -= OnDragEnter;
                element.DragLeave -= OnDragLeave;
            }
        }
    }

    private static void OnDragEnter(object sender, DragEventArgs e)
    {
        SetIsDragOver(sender as DependencyObject, true);
        e.Handled = true;
    }

    private static void OnDragLeave(object sender, DragEventArgs e)
    {
        SetIsDragOver(sender as DependencyObject, false);
        e.Handled = true;
    }

    private static void OnDragOver(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent("DataContext"))
        {
            e.Effects = DragDropEffects.Copy; // 使用 Copy 而不是 Move
            e.Handled = true;
        }
        else
        {
            e.Effects = DragDropEffects.None;
        }
    }

    private static void OnDrop(object sender, DragEventArgs e)
    {
        SetIsDragOver(sender as DependencyObject, false);

        if (e.Data.GetDataPresent("DataContext"))
        {
            var dataContext = e.Data.GetData("DataContext");
            var targetElement = sender as FrameworkElement;

            // 执行放置命令
            var command = GetDropCommand(targetElement);
            if (command != null && command.CanExecute(dataContext))
            {
                command.Execute(dataContext);
            }

            // 也可以直接设置 DataContext
            var setDataContext = GetSetDataContextOnDrop(targetElement);
            if (setDataContext && targetElement != null)
            {
                targetElement.DataContext = dataContext;
            }
        }
        e.Handled = true;
    }

    #endregion

    #region 辅助属性

    public static readonly DependencyProperty IsDragOverProperty =
        DependencyProperty.RegisterAttached("IsDragOver", typeof(bool), typeof(DragDropHelper));

    public static bool GetIsDragOver(DependencyObject obj) => (bool)obj.GetValue(IsDragOverProperty);
    public static void SetIsDragOver(DependencyObject obj, bool value) => obj.SetValue(IsDragOverProperty, value);

    public static readonly DependencyProperty DropCommandProperty =
        DependencyProperty.RegisterAttached("DropCommand", typeof(ICommand), typeof(DragDropHelper));

    public static ICommand GetDropCommand(DependencyObject obj) => (ICommand)obj.GetValue(DropCommandProperty);
    public static void SetDropCommand(DependencyObject obj, ICommand value) => obj.SetValue(DropCommandProperty, value);

    public static readonly DependencyProperty SetDataContextOnDropProperty =
        DependencyProperty.RegisterAttached("SetDataContextOnDrop", typeof(bool), typeof(DragDropHelper),
            new PropertyMetadata(false));

    public static bool GetSetDataContextOnDrop(DependencyObject obj) => (bool)obj.GetValue(SetDataContextOnDropProperty);
    public static void SetSetDataContextOnDrop(DependencyObject obj, bool value) => obj.SetValue(SetDataContextOnDropProperty, value);

    #endregion
}
