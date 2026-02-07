using CommunityToolkit.Mvvm.ComponentModel;

namespace HamsterStudio.ImageTool.Exposure;

public partial class ImageAdjustments : ObservableObject
{
    [ObservableProperty]
    private double _exposure = 0.0; // 曝光度 (-4 to 4)

    [ObservableProperty]
    private double _temperature = 0.0; // 色温 (-100 to 100)

    [ObservableProperty]
    private double _tint = 0.0; // 色调 (-100 to 100)

    [ObservableProperty]
    private double _highlights = 0.0; // 高光 (-100 to 100)

    [ObservableProperty]
    private double _shadows = 0.0; // 阴影 (-100 to 100)

    [ObservableProperty]
    private double _whites = 0.0; // 白色色阶 (-100 to 100)

    [ObservableProperty]
    private double _blacks = 0.0; // 黑色色阶 (-100 to 100)

    [ObservableProperty]
    private double _saturation = 0.0; // 饱和度 (-100 to 100)

    [ObservableProperty]
    private double _vibrance = 0.0; // 鲜艳度 (-100 to 100)

    public ImageAdjustments Clone()
    {
        return new ImageAdjustments()
        {
            Exposure = Exposure,
            Temperature = Temperature,
            Tint = Tint,
            Highlights = Highlights,
            Shadows = Shadows,
            Whites = Whites,
            Blacks = Blacks,
            Saturation = Saturation,
            Vibrance = Vibrance
        };
    }

    public void Reset()
    {
        Exposure = 0.0;
        Temperature = 0.0;
        Tint = 0.0;
        Highlights = 0.0;
        Shadows = 0.0;
        Whites = 0.0;
        Blacks = 0.0;
        Saturation = 0.0;
        Vibrance = 0.0;
    }

}
