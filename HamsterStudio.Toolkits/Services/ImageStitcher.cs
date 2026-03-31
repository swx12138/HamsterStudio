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
        if (mat == null || mat.Empty()) return;

        int rows = mat.Rows;
        int cols = mat.Cols;

        var fastMat = new Mat<Vec3b>(mat);

        // 锁定内存，获取扫描线指针
        var indexer = fastMat.GetIndexer();
        for (int y = 0; y < rows; ++y)
        {
            double t_y = (double)y / (rows - 1);

            for (int x = 0; x < cols; ++x)
            {
                double t_x = (double)x / (cols - 1);

                // 顶部插值
                byte b1 = (byte)((1.0 - t_x) * colorTl[0] + t_x * colorTr[0]);
                byte g1 = (byte)((1.0 - t_x) * colorTl[1] + t_x * colorTr[1]);
                byte r1 = (byte)((1.0 - t_x) * colorTl[2] + t_x * colorTr[2]);

                // 底部插值
                byte b2 = (byte)((1.0 - t_x) * colorBl[0] + t_x * colorBr[0]);
                byte g2 = (byte)((1.0 - t_x) * colorBl[1] + t_x * colorBr[1]);
                byte r2 = (byte)((1.0 - t_x) * colorBl[2] + t_x * colorBr[2]);

                var color = indexer[y, x];
                // 最终垂直插值
                color.Item0 = (byte)((1.0 - t_y) * b1 + t_y * b2); // B
                color.Item1 = (byte)((1.0 - t_y) * g1 + t_y * g2); // G
                color.Item2 = (byte)((1.0 - t_y) * r1 + t_y * r2); // R
                indexer[y, x] = color;
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
