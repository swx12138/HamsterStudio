using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace HamsterStudio.Photogrammetry.Services;

/// <summary>
/// 景深计算器
/// </summary>
public class DepthOfFieldCalculator
{
    // 定义一个枚举来表示常见的相机画幅，方便用户选择
    public enum CameraFormat
    {
        FullFrame,      // 全画幅 35mm (弥散圆 ~0.029mm)
        APS_C,          // APS-C (弥散圆 ~0.019mm, 以佳能为例)
        MicroFourThirds // M4/3 (弥散圆 ~0.015mm)
    }

    private const double FULL_FRAME_COC = 0.029; // 全画幅默认弥散圆直径 (mm)
    private const double APS_C_COC = 0.019;     // APS-C 默认弥散圆直径 (mm)
    private const double M43_COC = 0.015;       // M4/3 默认弥散圆直径 (mm)

    /// <summary>
    /// 计算景深相关参数
    /// </summary>
    /// <param name="focalLength">镜头焦距 (毫米)</param>
    /// <param name="fStop">光圈值 (f/2.8, f/4 等)</param>
    /// <param name="focusDistance">对焦距离 (米)</param>
    /// <param name="format">相机画幅</param>
    /// <returns>包含景深信息的结果对象</returns>
    public static DoFResult Calculate(double focalLength, double fStop, double focusDistance, CameraFormat format = CameraFormat.FullFrame)
    {
        if (focalLength <= 0 || fStop <= 0 || focusDistance <= 0)
        {
            throw new ArgumentException("焦距、光圈和对焦距离必须大于零。");
        }

        double coc;
        switch (format)
        {
            case CameraFormat.APS_C:
                coc = APS_C_COC;
                break;
            case CameraFormat.MicroFourThirds:
                coc = M43_COC;
                break;
            case CameraFormat.FullFrame:
            default:
                coc = FULL_FRAME_COC;
                break;
        }

        // 将对焦距离从米转换为毫米，以便单位统一
        double u = focusDistance * 1000;
        double f = focalLength;
        double N = fStop;

        // 1. 计算超焦距 (H)
        // H = (f^2) / (N * c)
        double h = (f * f) / (N * coc);

        // 2. 计算最近清晰点 (Dn)
        // Dn = (H * u) / (H + u)
        // 这里使用了近似公式 H >> f，所以省略了(u - f)项，适用于大部分情况
        double dNear = (h * u) / (h + u);

        // 3. 计算最远清晰点 (Df)
        // Df = (H * u) / (H - u)
        // 同样使用近似公式
        double numerator = h * u;
        double denominator = h - u;

        // 如果对焦距离等于或超过超焦距，则最远清晰点为无穷远
        double dFar = denominator > 0 ? numerator / denominator : double.PositiveInfinity;

        // 4. 计算总景深
        double dof = dFar - dNear;

        // 将结果从毫米转换回米
        return new DoFResult(
            hyperfocalDistanceMeters: h / 1000,
            nearLimitMeters: dNear / 1000,
            farLimitMeters: dFar == double.PositiveInfinity ? double.PositiveInfinity : dFar / 1000,
            totalDoFMeters: dof / 1000,
            circleOfConfusionMillimeters: coc
        );
    }
}

/// <summary>
/// 存储景深计算结果的数据类
/// </summary>
public class DoFResult(double hyperfocalDistanceMeters, double nearLimitMeters, double farLimitMeters, double totalDoFMeters, double circleOfConfusionMillimeters)
{
    const string CategroyName = "景深计算结果";

    [Category(CategroyName)]
    [DisplayName("超焦距(m)")]
    [Editable(false)]
    public double HyperfocalDistanceMeters { get; } = hyperfocalDistanceMeters;

    [Category(CategroyName)]
    [DisplayName("最近清晰点(m)")]
    [Editable(false)]
    public double NearLimitMeters { get; } = nearLimitMeters;

    [Category(CategroyName)]
    [DisplayName("最远清晰点(m)")]
    [Editable(false)]
    public double FarLimitMeters { get; } = farLimitMeters;

    [Category(CategroyName)]
    [DisplayName("总景深(m)")]
    [Editable(false)]
    public double TotalDoFMeters { get; } = totalDoFMeters;

    [Category(CategroyName)]
    [DisplayName("参考弥散圆直径(mm)")]
    [Editable(false)]
    public double CircleOfConfusionMillimeters { get; } = circleOfConfusionMillimeters;

    public override string ToString()
    {
        var farStr = double.IsPositiveInfinity(FarLimitMeters) ? "无穷远" : $"{FarLimitMeters:F2}m";
        return $"超焦距: {HyperfocalDistanceMeters:F2}m\n" +
               $"最近清晰点: {NearLimitMeters:F2}m\n" +
               $"最远清晰点: {farStr}\n" +
               $"总景深: {TotalDoFMeters:F2}m\n" +
               $"参考弥散圆直径：{CircleOfConfusionMillimeters}mm";
    }
}
