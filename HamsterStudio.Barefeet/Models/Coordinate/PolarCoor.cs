
namespace HamsterStudio.Barefeet.Models.Coordinate;

public class PolarCoor
{
    public double Radian { get; set; }

    public double Radius { get; set; }

    public CartesianCoor ToCartesianCoor(CartesianCoor? center = null)
    {
        var cc = new CartesianCoor()
        {
            X = (center?.X ?? 0) + Radius * Math.Cos(Radian), // 修改为Cos
            Y = (center?.Y ?? 0) + Radius * Math.Sin(Radian)  // 修改为Sin
        };
        return cc;
    }

    public static IEnumerable<PolarCoor> Uniform(double radius, int size, double startAngle = 0.0)
    {

        double totalRadian = 2 * Math.PI; // 总角度为360度，转换为弧度
        double startRadian = startAngle / 360 * totalRadian;
        double radianIncrement = totalRadian / size; // 每个点之间的角度增量
        return Enumerable.Range(0, size)
            .Select(i => new PolarCoor() { Radius = radius, Radian = startRadian + radianIncrement * i });
    }
}
