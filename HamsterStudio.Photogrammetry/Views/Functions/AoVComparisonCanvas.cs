using HamsterStudio.Photogrammetry.ViewModels;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace HamsterStudio.Photogrammetry.Views.Functions;

/// <summary>
/// 在一个区域中叠加绘制多个镜头的视角（AoV），用于直观对比。
/// 顶点（相机位置）位于底部中央，每个条目绘制一个扇形视场。
/// </summary>
public class AoVComparisonCanvas : FrameworkElement
{
    public static readonly DependencyProperty ItemsSourceProperty =
        DependencyProperty.Register(
            nameof(ItemsSource),
            typeof(IEnumerable),
            typeof(AoVComparisonCanvas),
            new FrameworkPropertyMetadata(null, OnItemsSourceChanged));

    public IEnumerable? ItemsSource
    {
        get => (IEnumerable?)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    private IEnumerable? observedItems_;
    private INotifyCollectionChanged? observedCollection_;

    private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((AoVComparisonCanvas)d).RebindItemsSource();
    }

    private void RebindItemsSource()
    {
        UnhookCollection();
        observedItems_ = ItemsSource;
        observedCollection_ = ItemsSource as INotifyCollectionChanged;
        HookCollection();
        InvalidateVisual();
    }

    private void HookCollection()
    {
        if (observedCollection_ is null)
        {
            return;
        }

        observedCollection_.CollectionChanged += OnCollectionChanged;
        foreach (var item in observedItems_ ?? Array.Empty<object>())
        {
            HookItem(item);
        }
    }

    private void UnhookCollection()
    {
        if (observedCollection_ is null)
        {
            return;
        }

        observedCollection_.CollectionChanged -= OnCollectionChanged;
        foreach (var item in observedItems_ ?? Array.Empty<object>())
        {
            UnhookItem(item);
        }
    }

    private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Reset:
                foreach (var item in observedItems_ ?? Array.Empty<object>())
                {
                    UnhookItem(item);
                }
                foreach (var item in observedItems_ ?? Array.Empty<object>())
                {
                    HookItem(item);
                }
                break;
            case NotifyCollectionChangedAction.Add:
                HookItems(e.NewItems);
                break;
            case NotifyCollectionChangedAction.Remove:
                UnhookItems(e.OldItems);
                break;
            case NotifyCollectionChangedAction.Replace:
                UnhookItems(e.OldItems);
                HookItems(e.NewItems);
                break;
        }

        InvalidateVisual();
    }

    private void HookItems(IList? items)
    {
        if (items is null)
        {
            return;
        }

        foreach (var item in items)
        {
            HookItem(item);
        }
    }

    private void UnhookItems(IList? items)
    {
        if (items is null)
        {
            return;
        }

        foreach (var item in items)
        {
            UnhookItem(item);
        }
    }

    private void HookItem(object? item)
    {
        if (item is INotifyPropertyChanged npc)
        {
            npc.PropertyChanged += OnItemPropertyChanged;
        }
    }

    private void UnhookItem(object? item)
    {
        if (item is INotifyPropertyChanged npc)
        {
            npc.PropertyChanged -= OnItemPropertyChanged;
        }
    }

    private void OnItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        InvalidateVisual();
    }

    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
        base.OnRenderSizeChanged(sizeInfo);
        InvalidateVisual();
    }

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);

        double width = ActualWidth;
        double height = ActualHeight;
        if (width <= 0 || height <= 0)
        {
            return;
        }

        dc.DrawRectangle(Brushes.White, null, new Rect(0, 0, width, height));

        var entries = ItemsSource?
            .OfType<IAngleOfViewEntry>()
            .Where(e => e.FocalLength > 0 && e.AngleOfView > 0)
            .ToList() ?? [];

        if (entries.Count == 0)
        {
            DrawCenteredText(dc, "请添加焦距", width, height);
            return;
        }

        const double margin = 36;
        double apexX = width / 2;
        double apexY = height - margin;

        // 顶部直线距离根据最宽视角自适应：保证最宽的三角形刚好能放入画布
        double halfWidth = width / 2 - margin;
        double maxHalfAngle = Math.Min(entries.Max(e => e.AngleOfView) / 2.0, 89.9); // 防止 tan 发散
        double tanMax = Math.Tan(maxHalfAngle * Math.PI / 180.0);
        double availableHeight = height - margin * 2;
        double distance = Math.Min(availableHeight, halfWidth / tanMax);
        if (distance < 20)
        {
            return;
        }

        double lineY = apexY - distance;

        // 底部参考线（模拟焦平面）
        var referencePen = new Pen(Brushes.LightGray, 1);
        referencePen.Freeze();
        dc.DrawLine(referencePen, new Point(margin, apexY), new Point(width - margin, apexY));

        // 中心轴线
        var axisBrush = new SolidColorBrush(Color.FromArgb(70, 0, 0, 0));
        axisBrush.Freeze();
        var axisPen = new Pen(axisBrush, 1);
        axisPen.Freeze();
        dc.DrawLine(axisPen, new Point(apexX, apexY), new Point(apexX, lineY));

        // 顶部公共参考线（所有视角延长到此直线，形成三角形）
        dc.DrawLine(referencePen, new Point(margin, lineY), new Point(width - margin, lineY));

        foreach (var entry in entries)
        {
            DrawTriangle(dc, entry, apexX, apexY, distance);
        }

        // 顶点标记（相机位置）
        dc.DrawEllipse(Brushes.Black, null, new Point(apexX, apexY), 3, 3);

        DrawLegend(dc, entries);
    }

    private static void DrawTriangle(DrawingContext dc, IAngleOfViewEntry entry, double apexX, double apexY, double radius)
    {
        double halfAngleRad = entry.AngleOfView / 2.0 * Math.PI / 180.0;

        double lineY = apexY - radius;
        double dy = lineY - apexY; // 负数（沿中心轴向上）

        double rightAngle = -Math.PI / 2.0 + halfAngleRad;
        double leftAngle = -Math.PI / 2.0 - halfAngleRad;

        // 两条边延长到公共直线 y = lineY 上的交点
        double tRight = dy / Math.Sin(rightAngle);
        double tLeft = dy / Math.Sin(leftAngle);

        var apex = new Point(apexX, apexY);
        var right = new Point(apexX + tRight * Math.Cos(rightAngle), lineY);
        var left = new Point(apexX + tLeft * Math.Cos(leftAngle), lineY);

        var geometry = new StreamGeometry();
        using (var ctx = geometry.Open())
        {
            ctx.BeginFigure(apex, true, true);
            ctx.LineTo(left, true, false);
            ctx.LineTo(right, true, false);
        }
        geometry.Freeze();

        var color = (entry.Brush as SolidColorBrush)?.Color ?? Colors.SteelBlue;
        var fill = new SolidColorBrush(Color.FromArgb(48, color.R, color.G, color.B));
        fill.Freeze();
        var pen = new Pen(entry.Brush, 1.5);
        pen.Freeze();

        dc.DrawGeometry(fill, pen, geometry);
    }

    private void DrawLegend(DrawingContext dc, IReadOnlyList<IAngleOfViewEntry> entries)
    {
        const double startX = 12;
        const double startY = 12;
        const double lineHeight = 20;
        const double swatchSize = 12;

        var typeface = new Typeface("Microsoft YaHei");
        double pixelsPerDip = VisualTreeHelper.GetDpi(this).PixelsPerDip;

        var texts = entries
            .Select(e => (Entry: e, Text: FormatEntry(e)))
            .ToList();

        double maxWidth = texts.Max(t => MeasureText(t.Text, typeface, pixelsPerDip).Width);
        double legendWidth = maxWidth + 42;
        double legendHeight = texts.Count * lineHeight + 12;

        var background = new SolidColorBrush(Color.FromArgb(210, 255, 255, 255));
        background.Freeze();
        dc.DrawRoundedRectangle(background, null, new Rect(startX, startY, legendWidth, legendHeight), 4, 4);

        double y = startY + 6;
        foreach (var (entry, text) in texts)
        {
            dc.DrawRectangle(entry.Brush, null, new Rect(startX + 8, y + 2, swatchSize, swatchSize));
            dc.DrawText(FormatText(text, typeface, pixelsPerDip), new Point(startX + 26, y));
            y += lineHeight;
        }
    }

    private static string FormatEntry(IAngleOfViewEntry entry)
        => $"{entry.FocalLength:0.#} mm   {entry.AngleOfView:0.0}°";

    private static FormattedText FormatText(string text, Typeface typeface, double pixelsPerDip)
        => new(text, CultureInfo.InvariantCulture, FlowDirection.LeftToRight, typeface, 12, Brushes.Black, pixelsPerDip);

    private static Size MeasureText(string text, Typeface typeface, double pixelsPerDip)
        => new(FormatText(text, typeface, pixelsPerDip).Width, 0);

    private void DrawCenteredText(DrawingContext dc, string text, double width, double height)
    {
        var typeface = new Typeface("Microsoft YaHei");
        double pixelsPerDip = VisualTreeHelper.GetDpi(this).PixelsPerDip;
        var formatted = FormatText(text, typeface, pixelsPerDip);
        dc.DrawText(formatted, new Point((width - formatted.Width) / 2, (height - formatted.Height) / 2));
    }
}
