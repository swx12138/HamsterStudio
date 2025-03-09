using System.Drawing;
using System.Windows;

namespace HamsterStudio.Barefeet.Models.Coordinate
{
    public class CartesianCoor
    {
        public double X { get; set; }

        public double Y { get; set; }

        public PolarCoor ToPolarCoor()
        {
            PolarCoor polar = new();
            polar.Radius = Math.Sqrt(X * X + Y * Y);
            polar.Radian = Math.Asin(X / polar.Radius);
            return polar;
        }

        public Point ToPoint()
        {
            return new Point((int)X, (int)Y);
        }
    }
}
