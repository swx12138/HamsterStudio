using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HamsterStudio.Barefeet.MVVM;
using HamsterStudio.ImageTool.Exposure;
using HamsterStudio.ImageTool.Exposure.ImageProcessors;
using Microsoft.Extensions.Logging;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace HamsterStudio.ImageTool.ViewModels;

public partial class ExposureAdjustmentViewModel : KnownViewModel
{
    private readonly ILogger<ExposureAdjustmentViewModel>? _logger;
    private readonly Lazy<IImageProcessor> _imageProcessor = new(() => new ThrottleImageProcessor(
        new FallbackImageProcessor(
            new CppImageProcessor(),
            new FallbackImageProcessor(
                new SoftwareImageProcessor()))
        )
    );

    public ExposureAdjustmentViewModel(ILogger<ExposureAdjustmentViewModel> logger) : base(logger)
    {
        // 初始化命令
        LoadImageCommand = new RelayCommand<string>(LoadImageFromPath);
        LoadNewImageCommand = new RelayCommand(OpenLoadImageDialog);
        ResetAdjustmentsCommand = new RelayCommand(ImageAdjustments.Reset);
        ResetScaleCommand = new RelayCommand(() => { ImageScale = 1.0; ImagePositionX = ImagePositionY = 0; AutoFitImage(); });
        AutoFitImageCommand = new RelayCommand(AutoFitImage);
        SaveImageCommand = new RelayCommand(SaveImage);

        ImageAdjustments.PropertyChanged += ImageAdjustments_PropertyChanged;
        DisplayName = "图片编辑器";
    }

    private void ImageAdjustments_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (OriginalImage == null) return;
        var adjusted = _imageProcessor.Value.ApplyAdjustments(OriginalImage, ImageAdjustments.Clone());
        AdjustedImage = adjusted;
    }

    #region 属性

    [ObservableProperty]
    private BitmapSource? _originalImage;

    [ObservableProperty]
    private BitmapSource? _adjustedImage;

    [ObservableProperty]
    private bool _isImageLoaded;

    [ObservableProperty]
    private string _imagePath = string.Empty;

    [ObservableProperty]
    private string _imageInfo = string.Empty;

    [ObservableProperty]
    private Visibility _dragDropTipsVisibility = Visibility.Visible;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(AdjustedImage))]
    private ImageAdjustments _imageAdjustments = new();

    [ObservableProperty]
    private double _imageCanvasReadonlyWidth = 0;

    [ObservableProperty]
    private double _imageCanvasReadonlyHeight = 0;

    [ObservableProperty]
    private double _imageScale = 1.0;

    [ObservableProperty]
    private double _imagePositionX = 0;

    [ObservableProperty]
    private double _imagePositionY = 0;

    [ObservableProperty]
    private double _imagePositionWidth = 0;

    [ObservableProperty]
    private double _imagePositionHeight = 0;

    [ObservableProperty]
    private Visibility _imageCenterPointVisibility = Visibility.Visible;

    [ObservableProperty]
    private double _mousePositionX = 0;

    [ObservableProperty]
    private double _mousePositionY = 0;

    [ObservableProperty]
    private double _lookRectangleAboveOriginalImageX = 0;
    [ObservableProperty]
    private double _lookRectangleAboveOriginalImageY = 0;
    [ObservableProperty]
    private double _lookRectangleAboveOriginalImageWidth = 0;
    [ObservableProperty]
    private double _lookRectangleAboveOriginalImageHeight = 0;

    #endregion

    #region 命令

    public IRelayCommand<string> LoadImageCommand { get; }
    public IRelayCommand LoadNewImageCommand { get; }
    public IRelayCommand ResetAdjustmentsCommand { get; }
    public ICommand ResetScaleCommand { get; }
    public ICommand AutoFitImageCommand { get; }
    public IRelayCommand SaveImageCommand { get; }

    #endregion

    #region 方法

    public void UpdateMousePosition(Point pt)
    {
        MousePositionX = pt.X - 10;
        MousePositionY = pt.Y - 10;
    }

    public void AutoFitImage()
    {
        if (AdjustedImage == null) return;
        double scaleX = ImageCanvasReadonlyWidth / AdjustedImage.Width;
        double scaleY = ImageCanvasReadonlyHeight / AdjustedImage.Height;
        ImageScale = Math.Min(scaleX, scaleY);
        UpdateImagePosition(false);
    }

    /// <summary>
    /// 更新图片位置
    /// </summary>
    /// <param name="mouseWheelEventDelta">鼠标滚轮delta</param>
    /// <param name="focus">是否以鼠标为中心缩放</param>
    public void UpdateImagePosition(int mouseWheelEventDelta, bool focus = true)
    {
        double oldScale = ImageScale;
        if (mouseWheelEventDelta > 0)
        {
            ImageScale *= 1.1;
        }
        else
        {
            ImageScale /= 1.1;
        }

        UpdateImagePosition(focus, oldScale);
    }

    public void UpdateImagePosition(bool focus = true, double? oldScaleNullable = null)
    {
        double oldScale = oldScaleNullable ?? ImageScale; // 如果没提供，假设本次没变化（可能是不需要缩放的情况）

        ImagePositionWidth = (AdjustedImage?.Width ?? 0) * ImageScale;
        ImagePositionHeight = (AdjustedImage?.Height ?? 0) * ImageScale;

        if (focus)
        {
            // 使用增量缩放比例
            double scaleFactor = ImageScale / oldScale;
            ImagePositionX = MousePositionX + (ImagePositionX - MousePositionX) * scaleFactor;
            ImagePositionY = MousePositionY + (ImagePositionY - MousePositionY) * scaleFactor;
        }
        else
        {
            ImagePositionX = (ImageCanvasReadonlyWidth - ImagePositionWidth) / 2;
            ImagePositionY = (ImageCanvasReadonlyHeight - ImagePositionHeight) / 2;
        }
    }

    public void OpenLoadImageDialog()
    {
        var openDialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "图片文件|*.png;*.jpg;*.jpeg;*.bmp|所有文件|*.*"
        };
        if (openDialog.ShowDialog() == true)
        {
            LoadImageFromPath(openDialog.FileName);
        }
    }

    partial void OnOriginalImageChanged(BitmapSource? value)
    {
        IsImageLoaded = value != null;
        if (value != null)
        {
            AdjustedImage = value.Clone();
        }
    }

    private void LoadImageFromPath(string? filePath)
    {
        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            return;

        try
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(filePath);
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();

            OriginalImage = bitmap;
            ImagePath = filePath;
            ImageInfo = $"尺寸: {bitmap.PixelWidth}×{bitmap.PixelHeight}\n" +
                                    $"格式: {Path.GetExtension(filePath)}\n" +
                                    $"路径: {filePath}";
            DragDropTipsVisibility = Visibility.Hidden;

            // 更新直方图
            //UpdateHistogram();
            ImageAdjustments.Reset();

            _logger?.LogInformation($"图片加载成功: {filePath}");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, $"图片加载失败: {filePath}");
            MessageBox.Show($"无法加载图片: {ex.Message}", "错误",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void SaveImage()
    {
        if (AdjustedImage == null) return;

        try
        {
            var saveDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "PNG 图片|*.png|JPEG 图片|*.jpg|BMP 图片|*.bmp",
                DefaultExt = ".png",
                FileName = $"adjusted_{Path.GetFileName(ImagePath)}"
            };

            if (saveDialog.ShowDialog() == true)
            {
                // 这里添加保存图像的逻辑
                // 可以使用PngBitmapEncoder、JpegBitmapEncoder等3
                BitmapEncoder encoder = saveDialog.FileName.ToLower() switch
                {
                    string s when s.EndsWith(".jpg") || s.EndsWith(".jpeg") => new JpegBitmapEncoder(),
                    string s when s.EndsWith(".bmp") => new BmpBitmapEncoder(),
                    _ => new PngBitmapEncoder()
                };

                encoder.Frames.Add(BitmapFrame.Create(AdjustedImage));

                using (FileStream stream = new FileStream(saveDialog.FileName, FileMode.Create))
                {
                    encoder.Save(stream);
                }

                MessageBox.Show($"图片已保存到: {saveDialog.FileName}", "保存成功",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "保存图片失败");
            MessageBox.Show($"保存失败: {ex.Message}", "错误",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    #endregion
}
