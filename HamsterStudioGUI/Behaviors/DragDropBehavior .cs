using HamsterStudioGUI.Models;
using HandyControl.Interactivity;
using System.Windows;
using System.Windows.Input;

namespace HamsterStudioGUI.Behaviors;

public class DragDropBehavior : Behavior<UIElement>
{
    public static readonly DependencyProperty DropCommandProperty =
        DependencyProperty.Register(nameof(DropCommand), typeof(ICommand), typeof(DragDropBehavior));

    //public static readonly DependencyProperty AcceptDataFormatProperty =
    //    DependencyProperty.Register(nameof(AcceptDataFormatProperty), typeof(DataFormats), typeof(DragDropBehavior));

    public ICommand DropCommand
    {
        get => (ICommand)GetValue(DropCommandProperty);
        set => SetValue(DropCommandProperty, value);
    }

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
        if (e.Data.GetDataPresent(DataFormats.FileDrop) ||
            e.Data.GetDataPresent("DataContext"))
        {
            e.Effects = DragDropEffects.Copy;
        }
        else
        {
            e.Effects = DragDropEffects.None;
        }
        e.Handled = true;
    }

    private void OnDrop(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop) || e.Data.GetDataPresent("DataContext"))
        {
            var data = e.Data.GetData(DataFormats.FileDrop) ?? new string[] { (e.Data.GetData("DataContext") as ImageModelDim).Path };
            if (DropCommand?.CanExecute(data) == true)
            {
                DropCommand.Execute(data);
            }
        }
        e.Handled = true;
    }

}
