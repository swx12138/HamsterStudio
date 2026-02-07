using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace HamsterStudio.Controls;

/// <summary>
/// 可缩放、可拖动的图像控件
/// </summary>
public class ZoomableImage : Control
{
    #region 依赖属性

    public static readonly DependencyProperty ImageSourceProperty =
        DependencyProperty.Register("ImageSource", typeof(ImageSource), typeof(ZoomableImage),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, OnImageSourceChanged));

    public static readonly DependencyProperty ZoomProperty =
        DependencyProperty.Register("Zoom", typeof(double), typeof(ZoomableImage),
            new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.AffectsRender, OnZoomChanged, CoerceZoom));

    public static readonly DependencyProperty MinZoomProperty =
        DependencyProperty.Register("MinZoom", typeof(double), typeof(ZoomableImage),
            new PropertyMetadata(0.1));

    public static readonly DependencyProperty MaxZoomProperty =
        DependencyProperty.Register("MaxZoom", typeof(double), typeof(ZoomableImage),
            new PropertyMetadata(10.0));

    public static readonly DependencyProperty ZoomStepProperty =
        DependencyProperty.Register("ZoomStep", typeof(double), typeof(ZoomableImage),
            new PropertyMetadata(0.2));

    public static readonly DependencyProperty PanModeProperty =
        DependencyProperty.Register("PanMode", typeof(PanMode), typeof(ZoomableImage),
            new PropertyMetadata(PanMode.Always));

    public static readonly DependencyProperty StretchProperty =
        DependencyProperty.Register("Stretch", typeof(Stretch), typeof(ZoomableImage),
            new PropertyMetadata(Stretch.Uniform));

    public static readonly DependencyProperty ImageAlignmentProperty =
        DependencyProperty.Register("ImageAlignment", typeof(ImageAlignment), typeof(ZoomableImage),
            new PropertyMetadata(ImageAlignment.Center));

    #endregion

    #region 属性

    /// <summary>
    /// 图像源
    /// </summary>
    public ImageSource ImageSource
    {
        get => (ImageSource)GetValue(ImageSourceProperty);
        set => SetValue(ImageSourceProperty, value);
    }

    /// <summary>
    /// 当前缩放级别
    /// </summary>
    public double Zoom
    {
        get => (double)GetValue(ZoomProperty);
        set => SetValue(ZoomProperty, value);
    }

    /// <summary>
    /// 最小缩放级别
    /// </summary>
    public double MinZoom
    {
        get => (double)GetValue(MinZoomProperty);
        set => SetValue(MinZoomProperty, value);
    }

    /// <summary>
    /// 最大缩放级别
    /// </summary>
    public double MaxZoom
    {
        get => (double)GetValue(MaxZoomProperty);
        set => SetValue(MaxZoomProperty, value);
    }

    /// <summary>
    /// 缩放步长
    /// </summary>
    public double ZoomStep
    {
        get => (double)GetValue(ZoomStepProperty);
        set => SetValue(ZoomStepProperty, value);
    }

    /// <summary>
    /// 拖动模式
    /// </summary>
    public PanMode PanMode
    {
        get => (PanMode)GetValue(PanModeProperty);
        set => SetValue(PanModeProperty, value);
    }

    /// <summary>
    /// 图像拉伸模式
    /// </summary>
    public Stretch Stretch
    {
        get => (Stretch)GetValue(StretchProperty);
        set => SetValue(StretchProperty, value);
    }

    /// <summary>
    /// 图像对齐方式
    /// </summary>
    public ImageAlignment ImageAlignment
    {
        get => (ImageAlignment)GetValue(ImageAlignmentProperty);
        set => SetValue(ImageAlignmentProperty, value);
    }

    #endregion

    #region 内部字段

    private ScaleTransform _scaleTransform;
    private TranslateTransform _translateTransform;
    private TransformGroup _transformGroup;
    private Point _lastMousePosition;
    private bool _isDragging;
    private MatrixTransform _matrixTransform;
    private Point _imagePosition;

    #endregion

    #region 静态构造函数

    static ZoomableImage()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(ZoomableImage),
            new FrameworkPropertyMetadata(typeof(ZoomableImage)));
    }

    #endregion

    #region 构造函数

    public ZoomableImage()
    {
        InitializeTransforms();
        InitializeCommands();
    }

    private void InitializeTransforms()
    {
        _scaleTransform = new ScaleTransform();
        _translateTransform = new TranslateTransform();

        _transformGroup = new TransformGroup();
        _transformGroup.Children.Add(_scaleTransform);
        _transformGroup.Children.Add(_translateTransform);

        _matrixTransform = new MatrixTransform();
    }

    #endregion

    #region 命令

    public static readonly RoutedUICommand ZoomInCommand =
        new RoutedUICommand("放大", "ZoomIn", typeof(ZoomableImage));

    public static readonly RoutedUICommand ZoomOutCommand =
        new RoutedUICommand("缩小", "ZoomOut", typeof(ZoomableImage));

    public static readonly RoutedUICommand ResetViewCommand =
        new RoutedUICommand("重置", "ResetView", typeof(ZoomableImage));

    private void InitializeCommands()
    {
        CommandBindings.Add(new CommandBinding(ZoomInCommand, ExecuteZoomIn));
        CommandBindings.Add(new CommandBinding(ZoomOutCommand, ExecuteZoomOut));
        CommandBindings.Add(new CommandBinding(ResetViewCommand, ExecuteResetView));
    }

    private void ExecuteZoomIn(object sender, ExecutedRoutedEventArgs e)
    {
        ZoomIn();
    }

    private void ExecuteZoomOut(object sender, ExecutedRoutedEventArgs e)
    {
        ZoomOut();
    }

    private void ExecuteResetView(object sender, ExecutedRoutedEventArgs e)
    {
        ResetView();
    }

    #endregion

    #region 公共方法

    /// <summary>
    /// 放大图像
    /// </summary>
    public void ZoomIn()
    {
        Zoom += ZoomStep;
    }

    /// <summary>
    /// 缩小图像
    /// </summary>
    public void ZoomOut()
    {
        Zoom -= ZoomStep;
    }

    /// <summary>
    /// 重置视图
    /// </summary>
    public void ResetView()
    {
        Zoom = 1.0;
        _translateTransform.X = 0;
        _translateTransform.Y = 0;
        UpdateImagePosition();
    }

    /// <summary>
    /// 缩放到指定区域
    /// </summary>
    public void ZoomToRect(Rect rect)
    {
        double scaleX = ActualWidth / rect.Width;
        double scaleY = ActualHeight / rect.Height;
        double scale = Math.Min(scaleX, scaleY);

        Zoom = Math.Max(MinZoom, Math.Min(MaxZoom, scale));

        _translateTransform.X = -rect.X * Zoom;
        _translateTransform.Y = -rect.Y * Zoom;
    }

    /// <summary>
    /// 缩放到指定比例
    /// </summary>
    public void ZoomTo(double zoom, Point center)
    {
        double oldZoom = Zoom;
        Zoom = Math.Max(MinZoom, Math.Min(MaxZoom, zoom));

        // 保持缩放中心点不变
        double scaleChange = Zoom / oldZoom;
        Point mousePos = TranslatePoint(center, this);

        _translateTransform.X = mousePos.X - scaleChange * (mousePos.X - _translateTransform.X);
        _translateTransform.Y = mousePos.Y - scaleChange * (mousePos.Y - _translateTransform.Y);
    }

    #endregion

    #region 事件处理

    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
        base.OnMouseWheel(e);

        if (ImageSource == null) return;

        double oldZoom = Zoom;
        double zoom = e.Delta > 0 ?
            Zoom * (1 + ZoomStep) :
            Zoom / (1 + ZoomStep);

        Zoom = Math.Max(MinZoom, Math.Min(MaxZoom, zoom));

        // 以鼠标位置为中心缩放
        Point mousePos = e.GetPosition(this);
        double scaleChange = Zoom / oldZoom;

        _translateTransform.X = mousePos.X - scaleChange * (mousePos.X - _translateTransform.X);
        _translateTransform.Y = mousePos.Y - scaleChange * (mousePos.Y - _translateTransform.Y);

        e.Handled = true;
    }

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);

        if (CanPan())
        {
            _lastMousePosition = e.GetPosition(this);
            _isDragging = true;
            CaptureMouse();
            Cursor = Cursors.Hand;
            e.Handled = true;
        }
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);

        if (_isDragging && CanPan())
        {
            Point currentPosition = e.GetPosition(this);
            Vector delta = currentPosition - _lastMousePosition;

            _translateTransform.X += delta.X;
            _translateTransform.Y += delta.Y;

            _lastMousePosition = currentPosition;
            e.Handled = true;
        }
    }

    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonUp(e);

        if (_isDragging)
        {
            _isDragging = false;
            ReleaseMouseCapture();
            Cursor = Cursors.Arrow;
            e.Handled = true;
        }
    }

    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
        base.OnRenderSizeChanged(sizeInfo);
        UpdateImagePosition();
    }

    #endregion

    #region 辅助方法

    private bool CanPan()
    {
        return PanMode == PanMode.Always ||
               PanMode == PanMode.WhenZoomed && Zoom > 1.0;
    }

    private void UpdateImagePosition()
    {
        if (ImageSource == null) return;

        double imageWidth = ImageSource.Width * Zoom;
        double imageHeight = ImageSource.Height * Zoom;

        double availableWidth = ActualWidth;
        double availableHeight = ActualHeight;

        // 根据对齐方式计算位置
        switch (ImageAlignment)
        {
            case ImageAlignment.TopLeft:
                _imagePosition = new Point(0, 0);
                break;
            case ImageAlignment.TopCenter:
                _imagePosition = new Point((availableWidth - imageWidth) / 2, 0);
                break;
            case ImageAlignment.TopRight:
                _imagePosition = new Point(availableWidth - imageWidth, 0);
                break;
            case ImageAlignment.CenterLeft:
                _imagePosition = new Point(0, (availableHeight - imageHeight) / 2);
                break;
            case ImageAlignment.Center:
                _imagePosition = new Point(
                    (availableWidth - imageWidth) / 2,
                    (availableHeight - imageHeight) / 2);
                break;
            case ImageAlignment.CenterRight:
                _imagePosition = new Point(
                    availableWidth - imageWidth,
                    (availableHeight - imageHeight) / 2);
                break;
            case ImageAlignment.BottomLeft:
                _imagePosition = new Point(0, availableHeight - imageHeight);
                break;
            case ImageAlignment.BottomCenter:
                _imagePosition = new Point(
                    (availableWidth - imageWidth) / 2,
                    availableHeight - imageHeight);
                break;
            case ImageAlignment.BottomRight:
                _imagePosition = new Point(
                    availableWidth - imageWidth,
                    availableHeight - imageHeight);
                break;
        }

        InvalidateVisual();
    }

    #endregion

    #region 依赖属性回调

    private static void OnImageSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (ZoomableImage)d;
        control.UpdateImagePosition();
    }

    private static void OnZoomChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (ZoomableImage)d;
        control._scaleTransform.ScaleX = control.Zoom;
        control._scaleTransform.ScaleY = control.Zoom;
        control.UpdateImagePosition();
    }

    private static object CoerceZoom(DependencyObject d, object baseValue)
    {
        var control = (ZoomableImage)d;
        double value = (double)baseValue;
        return Math.Max(control.MinZoom, Math.Min(control.MaxZoom, value));
    }

    #endregion

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);

        if (ImageSource == null) return;

        // 绘制背景
        dc.DrawRectangle(Background, null, new Rect(0, 0, ActualWidth, ActualHeight));

        // 应用变换
        dc.PushTransform(_transformGroup);

        // 绘制图像
        dc.DrawImage(ImageSource, new Rect(_imagePosition.X, _imagePosition.Y,
            ImageSource.Width * Zoom, ImageSource.Height * Zoom));

        dc.Pop();
    }
}

#region 枚举类型

/// <summary>
/// 拖动模式
/// </summary>
public enum PanMode
{
    /// <summary>
    /// 从不拖动
    /// </summary>
    Never,

    /// <summary>
    /// 总是可以拖动
    /// </summary>
    Always,

    /// <summary>
    /// 仅在缩放时拖动
    /// </summary>
    WhenZoomed
}

/// <summary>
/// 图像对齐方式
/// </summary>
public enum ImageAlignment
{
    TopLeft,
    TopCenter,
    TopRight,
    CenterLeft,
    Center,
    CenterRight,
    BottomLeft,
    BottomCenter,
    BottomRight
}

#endregion
