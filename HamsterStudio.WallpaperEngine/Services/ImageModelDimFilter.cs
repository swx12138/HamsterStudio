using CommunityToolkit.Mvvm.ComponentModel;
using HamsterStudio.Barefeet.Logging;
using System.ComponentModel;

namespace HamsterStudio.WallpaperEngine.Services;

public interface IImageModelDimFilter
{
    bool Is4kOnly { get; set; }
    bool IsVerticalOnly { get; set; }
    bool IsHorizontalOnly { get; set; }
    bool IsMarkedOnly { get; set; }
    bool Filter(ImageModelDim item);
}

public partial class ImageModelDimFilter : ObservableObject, IImageModelDimFilter
{
    [ObservableProperty]
    private bool _Is4kOnly = true;

    [ObservableProperty]
    private bool _IsVerticalOnly = false;

    [ObservableProperty]
    private bool _IsHorizontalOnly = false;

    [ObservableProperty]
    private bool _IsMarkedOnly = false;

    public new event EventHandler<PropertyChangedEventArgs> PropertyChanged;

    public bool Filter(ImageModelDim item)
    {
        if (item == null) return false;
        
        if (IsMarkedOnly && !item.Mark)
        {
            return false;
        }

        if (IsVerticalOnly && item.MetaInfo.Width > item.MetaInfo.Height)
        {
            return false;
        }

        if (IsHorizontalOnly && item.MetaInfo.Width < item.MetaInfo.Height)
        {
            return false;
        }

        if (Is4kOnly && !((item.MetaInfo.Width >= 3840 && item.MetaInfo.Height >= 2160) || (item.MetaInfo.Height >= 3840 && item.MetaInfo.Width >= 2160)))
        {
            // ShellApi.SendToRecycleBin(item.Path);
            return false;
        }       

        return true;
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        //_logger.LogDebug($"Property {e.PropertyName} has Changed.");
        PropertyChanged?.Invoke(this, e);
        base.OnPropertyChanged(e);
    }
}
