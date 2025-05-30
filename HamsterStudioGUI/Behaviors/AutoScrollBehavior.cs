using HandyControl.Interactivity;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace HamsterStudioGUI.Behaviors;

public class AutoScrollBehavior : Behavior<DataGrid>
{
    protected override void OnAttached()
    {
        base.OnAttached();
        InitializeTimer();
        var itemsSource = AssociatedObject.ItemsSource as INotifyCollectionChanged;
        if (itemsSource != null)
            itemsSource.CollectionChanged += OnCollectionChanged;
    }

    private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Add)
        {
            Dispatcher.BeginInvoke(() =>
            {
                var scrollViewer = FindVisualChild<ScrollViewer>(AssociatedObject);
                scrollViewer?.ScrollToEnd();
            }, DispatcherPriority.ContextIdle);
        }
    }

    private static T? FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
    {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is T result)
                return result;
            result = FindVisualChild<T>(child);
            if (result != null)
                return result;
        }
        return null;
    }

    private DispatcherTimer _scrollTimer;

    private void InitializeTimer()
    {
        _scrollTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(300) };
        _scrollTimer.Tick += (s, e) =>
        {
            var scrollViewer = FindVisualChild<ScrollViewer>(AssociatedObject);
            scrollViewer?.ScrollToEnd();
            _scrollTimer.Stop();
        };
    }

    private void OnItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Add)
        {
            _scrollTimer.Stop();
            _scrollTimer.Start();
        }
    }

}
