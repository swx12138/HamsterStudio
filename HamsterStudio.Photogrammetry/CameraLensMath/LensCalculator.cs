
namespace HamsterStudio.Photogrammetry.CameraLensMath;

public enum Orientation { Horizontal, Vertical, Diagonal };

public static class LensCalculator
{
    // 全画幅传感器标准尺寸（单位：毫米）
    private const double FullFrameWidth = 36.0;
    private const double FullFrameHeight = 24.0;

    /// <summary>
    /// 计算全画幅相机的对角线视角（度）
    /// </summary>
    /// <param name="focalLength">镜头焦距（毫米）</param>
    /// <returns>对角线视角（度）</returns>
    public static double GetDiagonalAngleOfView(double focalLength)
    {
        return GetDiagonalAngleOfView(focalLength, FullFrameWidth, FullFrameHeight);
    }

    /// <summary>
    /// 计算自定义传感器尺寸的对角线视角（度）
    /// </summary>
    /// <param name="focalLength">镜头焦距（毫米）</param>
    /// <param name="sensorWidth">传感器宽度（毫米）</param>
    /// <param name="sensorHeight">传感器高度（毫米）</param>
    /// <returns>对角线视角（度）</returns>
    public static double GetDiagonalAngleOfView(double focalLength, double sensorWidth, double sensorHeight)
    {
        if (!double.IsFinite(focalLength) || focalLength <= 0)
            throw new ArgumentOutOfRangeException(nameof(focalLength), "焦距必须大于0。");
        if (!double.IsFinite(sensorWidth) || sensorWidth <= 0)
            throw new ArgumentOutOfRangeException(nameof(sensorWidth), "传感器宽度必须为正数。");
        if (!double.IsFinite(sensorHeight) || sensorHeight <= 0)
            throw new ArgumentOutOfRangeException(nameof(sensorHeight), "传感器高度必须为正数。");

        double diagonal = Math.Sqrt(sensorWidth * sensorWidth + sensorHeight * sensorHeight);
        double angleRad = 2 * Math.Atan(diagonal / (2 * focalLength));
        return angleRad * (180.0 / Math.PI);
    }

    /// <summary>
    /// 计算水平视角（度）- 全画幅
    /// </summary>
    public static double GetHorizontalAngleOfView(double focalLength)
    {
        return GetHorizontalAngleOfView(focalLength, FullFrameWidth);
    }

    /// <summary>
    /// 计算水平视角（度）- 自定义宽度
    /// </summary>
    public static double GetHorizontalAngleOfView(double focalLength, double sensorWidth)
    {
        if (!double.IsFinite(focalLength) || focalLength <= 0)
            throw new ArgumentOutOfRangeException(nameof(focalLength), "焦距必须大于0。");
        if (!double.IsFinite(sensorWidth) || sensorWidth <= 0)
            throw new ArgumentOutOfRangeException(nameof(sensorWidth), "传感器宽度必须为正数。");
        double angleRad = 2 * Math.Atan(sensorWidth / (2 * focalLength));
        return angleRad * (180.0 / Math.PI);
    }

    /// <summary>
    /// 计算垂直视角（度）- 全画幅
    /// </summary>
    public static double GetVerticalAngleOfView(double focalLength)
    {
        return GetVerticalAngleOfView(focalLength, FullFrameHeight);
    }

    /// <summary>
    /// 计算垂直视角（度）- 自定义高度
    /// </summary>
    public static double GetVerticalAngleOfView(double focalLength, double sensorHeight)
    {
        if (!double.IsFinite(focalLength) || focalLength <= 0)
            throw new ArgumentOutOfRangeException(nameof(focalLength), "焦距必须大于0。");
        if (!double.IsFinite(sensorHeight) || sensorHeight <= 0)
            throw new ArgumentOutOfRangeException(nameof(sensorHeight), "传感器高度必须为正数。");
        double angleRad = 2 * Math.Atan(sensorHeight / (2 * focalLength));
        return angleRad * (180.0 / Math.PI);
    }
}