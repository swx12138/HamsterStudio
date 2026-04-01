using OpenCvSharp;
using System.Diagnostics;

namespace HamsterStudio.Toolkits.Services;

public class ImageStitcher
{
    static readonly Size Size_Empty = new Size(0, 0);

    public enum StitchSelectionMode
    {
        Both, Landscape, Portrait
    };

    static Size CalculateGridLayout(int image_count, Size cellSize)
    {
        if (image_count < 4)
        {
            if (cellSize.Width > cellSize.Height)
            {
                return new(image_count, 1); // 横屏且图片较少，单列展示
            }
            else
            {
                return new(1, image_count); // 竖屏且图片较少，单行展示
            }
        }
        else
        {
            // 正常计算布局
            int cols = (int)Math.Sqrt(image_count);
            if (cols * cols < image_count)
            {
                cols += 1; // 如果列数的平方小于图片数量，增加一列
            }

            int rows = (image_count + cols - 1) / cols; // 向上取整
            return new(rows, cols);
        }
    }

    public static void FillBilinearFast(Mat mat, Scalar colorTl, Scalar colorTr, Scalar colorBl, Scalar colorBr)
    {
        ArgumentNullException.ThrowIfNull(mat);
        if (!mat.IsContinuous())
        {
            throw new ArgumentException("Mat must be continuous", nameof(mat));
        }

        // 预计算颜色分量
        byte tl_b = (byte)colorTl[0], tl_g = (byte)colorTl[1], tl_r = (byte)colorTl[2];
        byte tr_b = (byte)colorTr[0], tr_g = (byte)colorTr[1], tr_r = (byte)colorTr[2];
        byte bl_b = (byte)colorBl[0], bl_g = (byte)colorBl[1], bl_r = (byte)colorBl[2];
        byte br_b = (byte)colorBr[0], br_g = (byte)colorBr[1], br_r = (byte)colorBr[2];

        double invHeight = 1.0 / (mat.Height - 1);
        double invWidth = 1.0 / (mat.Width - 1);

        Span<Vec3b> span = mat.AsSpan<Vec3b>();
        int width = mat.Width;
        int height = mat.Height;

        // 预计算每行的t_y值
        double[] t_y_cache = new double[height];
        double[] inv_t_y_cache = new double[height];
        for (int y = 0; y < height; y++)
        {
            t_y_cache[y] = y * invHeight;
            inv_t_y_cache[y] = 1.0 - t_y_cache[y];
        }

        // 预计算每列的t_x值
        double[] t_x_cache = new double[width];
        double[] inv_t_x_cache = new double[width];
        for (int x = 0; x < width; x++)
        {
            t_x_cache[x] = x * invWidth;
            inv_t_x_cache[x] = 1.0 - t_x_cache[x];
        }

        for (int y = 0; y < height; y++)
        {
            double t_y = t_y_cache[y];
            double inv_t_y = inv_t_y_cache[y];
            int rowOffset = y * width;

            for (int x = 0; x < width; x++)
            {
                double t_x = t_x_cache[x];
                double inv_t_x = inv_t_x_cache[x];

                // 内联计算，避免重复的浮点运算
                byte b1 = (byte)(inv_t_x * tl_b + t_x * tr_b);
                byte g1 = (byte)(inv_t_x * tl_g + t_x * tr_g);
                byte r1 = (byte)(inv_t_x * tl_r + t_x * tr_r);

                byte b2 = (byte)(inv_t_x * bl_b + t_x * br_b);
                byte g2 = (byte)(inv_t_x * bl_g + t_x * br_g);
                byte r2 = (byte)(inv_t_x * bl_r + t_x * br_r);

                int index = rowOffset + x;
                ref var pixel = ref span[index];
                pixel.Item0 = (byte)(inv_t_y * b1 + t_y * b2);
                pixel.Item1 = (byte)(inv_t_y * g1 + t_y * g2);
                pixel.Item2 = (byte)(inv_t_y * r1 + t_y * r2);
            }
        }
    }

    public static void Stitch(IReadOnlyCollection<string> imagePaths, Size gridSize, int spacing, Size cellSize, StitchSelectionMode stitchSelectionMode, string outFilename)
    {
        var images = imagePaths.Select(imagePath => Cv2.ImRead(imagePath)).Where(image =>
        {
            return stitchSelectionMode switch
            {
                StitchSelectionMode.Portrait => image.Width < image.Height,
                StitchSelectionMode.Landscape => image.Height <= image.Width,
                _ => true
            };
        });
        var preprocessed = images.Select(image =>
        {
            double xScale = cellSize.Width / image.Width;
            double yScale = cellSize.Height / image.Height;
            double scale = Math.Min(xScale, yScale);
            Mat result = new();
            Cv2.Resize(image, result, Size_Empty, scale, scale);
            return result;
        }).ToArray();

        if (preprocessed.Length <= 0)
        {
            return;
        }

        var layout = CalculateGridLayout(preprocessed.Length, cellSize);
        int rows = layout.Width, cols = layout.Height; // CalculateGridLayout 里面是反的

        // 默认边距为格子尺寸的 5%
        if (spacing <= 0)
        {
            spacing = Math.Min(cellSize.Width, cellSize.Height) / 20;
        }

        Size canvasSize = new(
            cellSize.Width * gridSize.Width + spacing * (gridSize.Width + 1),
            cellSize.Height * gridSize.Height + spacing * (gridSize.Height + 1));
        Mat canvas = new(canvasSize, MatType.CV_8UC3, Scalar.WhiteSmoke);
        FillBilinearFast(canvas, Scalar.LightSlateGray, Scalar.PeachPuff, Scalar.Violet, Scalar.Peru);

        for (int i = 0; i < preprocessed.Length; i++)
        {
            int row_idx = i / cols;
            int col_idx = i % cols;

            if (row_idx >= rows)
            {
                Trace.TraceWarning("警告: 图片数量超过了计算的行列数，剩余图片将被忽略。");
                break; // 防止越界
            }

            Mat roi = canvas[
                new Rect(
                spacing + col_idx * (cellSize.Width + spacing),
                spacing + row_idx * (cellSize.Height + spacing),
                cellSize.Width,
                cellSize.Height)];

            //PasteImage(roi, preprocessed[i]);

        }
    }


}
