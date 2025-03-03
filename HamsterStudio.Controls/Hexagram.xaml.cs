using HamsterStudio.Barefeet;
using HamsterStudio.Barefeet.Algorithm;
using HamsterStudio.Barefeet.Models.Coordinate;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace HamsterStudio.Controls
{
    /// <summary>
    /// Hexagram.xaml 的交互逻辑
    /// </summary>
    public partial class Hexagram : UserControl
    {
        #region Brush
        public static readonly PropertyMetadata BrushMetadta = new(default(Brush), BrushMetadta_OnPropertyChanged);
        public static readonly DependencyProperty BrushProperty = DependencyProperty.Register("Brush", typeof(Brush), typeof(Hexagram), BrushMetadta);
        public Brush Brush
        {
            get => (Brush)GetValue(BrushProperty);
            set => SetValue(BrushProperty, value);
        }
        private static void BrushMetadta_OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Hexagram hexagram)
            {
                if (hexagram.ShapeStyle == ShapeStyle.FillOnly)
                {
                    hexagram.mainView.Fill = hexagram.Brush;
                    hexagram.mainView.Stroke = null;
                    hexagram.mainView.StrokeThickness = 0;
                }
                else if (hexagram.ShapeStyle == ShapeStyle.BorderOnly)
                {
                    hexagram.mainView.Fill = null;
                    hexagram.mainView.Stroke = hexagram.Brush;
                    hexagram.mainView.StrokeThickness = 1;
                }
                else
                {
                    hexagram.mainView.Fill = hexagram.Brush;
                    hexagram.mainView.Stroke = hexagram.Brush;
                    hexagram.mainView.StrokeThickness = 1;
                }
            }
        }
        #endregion

        #region Radius
        public static readonly PropertyMetadata RadiusMetadta = new(default(double), RadiusMetadta_OnPropertyChanged);
        public static readonly DependencyProperty RadiusProperty = DependencyProperty.Register("Radius", typeof(double), typeof(Hexagram), RadiusMetadta);
        public double Radius
        {
            get => (double)GetValue(RadiusProperty);
            set => SetValue(RadiusProperty, value);
        }
        private static void RadiusMetadta_OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Hexagram hexagram)
            {
                hexagram.Render();
            }
        }
        #endregion

        #region ShapeStyle
        public static readonly PropertyMetadata ShapeStyleMetadta = new(default(ShapeStyle), ShapeStyleMetadta_OnPropertyChanged);
        public static readonly DependencyProperty ShapeStyleProperty = DependencyProperty.Register("ShapeStyle", typeof(ShapeStyle), typeof(Hexagram), ShapeStyleMetadta);
        public ShapeStyle ShapeStyle
        {
            get => (ShapeStyle)GetValue(ShapeStyleProperty);
            set => SetValue(ShapeStyleProperty, value);
        }
        private static void ShapeStyleMetadta_OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            BrushMetadta_OnPropertyChanged(d, e);
        }
        #endregion

        #region ShapeType
        public static readonly PropertyMetadata ShapeTypeMetadta = new(default(ShapeType), ShapeTypeMetadta_OnPropertyChanged);
        public static readonly DependencyProperty ShapeTypeProperty = DependencyProperty.Register("ShapeType", typeof(ShapeType), typeof(Hexagram), ShapeTypeMetadta);
        public ShapeType ShapeType
        {
            get
            {
                int n = (int)GetValue(ShapeTypeProperty);
                return (ShapeType)(n >= 3 && n <= 17 ? n : 17);
            }
            set => SetValue(ShapeTypeProperty, value);
        }
        private static void ShapeTypeMetadta_OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Hexagram hexagram)
            {
                hexagram.Render();
            }
        }
        #endregion

        public Hexagram()
        {
            InitializeComponent();
        }

        private void Render()
        {
            mainView.Data = GetGeometry();
        }

        public Geometry GetGeometry()
        {
            var center = new CartesianCoor
            {
                X = Radius,
                Y = Radius
            };

            var borderCount = (int)ShapeType;
            var outside = PolarCoor.Uniform(Radius, borderCount).Select(x => x.ToCartesianCoor(center)).ToArray();
            var inside = PolarCoor.Uniform(Radius * (0.5 + 0.02 * borderCount), borderCount, 180 / borderCount).Select(x => x.ToCartesianCoor(center)).ToArray();
            var all_point = ArrayMerger.AlternateMerge(outside, inside).Select(x => x.ToPoint()).ToArray();

            StringBuilder sb = new();
            sb.Append($"M{(int)all_point[0].X},{(int)all_point[0].Y} ");
            for (int i = 1; i < all_point.Length; i++)
            {
                sb.Append($"L{(int)all_point[i].X},{(int)all_point[i].Y} ");
            }
            sb.Append('Z');
            return Geometry.Parse(sb.ToString());
        }

    }
}
