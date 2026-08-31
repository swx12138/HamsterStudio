
using HamsterStudio.Photogrammetry.CameraLensMath;
using static HamsterStudio.Photogrammetry.CameraLensMath.DepthOfFieldCalculator;

namespace HamsterStudio.Photogrammetry.Services;

public class LensCalculateService
{
    public double GetAoV(double focalLength, Orientation orientation = Orientation.Horizontal)
    {
        return orientation switch
        {
            Orientation.Diagonal => LensCalculator.GetDiagonalAngleOfView(focalLength),
            Orientation.Horizontal => LensCalculator.GetHorizontalAngleOfView(focalLength),
            Orientation.Vertical => LensCalculator.GetVerticalAngleOfView(focalLength),
            _ => LensCalculator.GetHorizontalAngleOfView(focalLength)
        };
    }

    public DoFResult GetDoF(double focalLength, double fStop, double focusDistance, CameraFormat format = CameraFormat.FullFrame)
    {
        return DepthOfFieldCalculator.Calculate(focalLength, fStop, focusDistance, format);
    }

}