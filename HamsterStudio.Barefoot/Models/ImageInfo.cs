using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows.Input;

namespace HamsterStudio.Barefoot.Models;

public enum ImageRotateType
{
    R0, R90, R180, R270
}

public partial class ImageInfo(string filename) : ObservableObject
{
    [ObservableProperty]
    private string _filename = filename;

    [ObservableProperty]
    private bool _isSelected = true;

    [ObservableProperty]
    private bool _upDownFlip = false;

    [ObservableProperty]
    private bool _leftRightFlip = false;

    [ObservableProperty]
    private int _repeatCount = 1;

    [ObservableProperty]
    private ImageRotateType _rotateType = ImageRotateType.R0;

    public ImageRotateType[] ImageRotateTypes { get; } = Enum.GetValues<ImageRotateType>();

    public required ICommand RepeatCommand { get; set; }
    public required ICommand DestroyCommand { get; set; }

}
