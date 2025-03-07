using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace HamsterStudio.Controls.Smarter;

public class DynamicUniformGrid : UniformGrid
{
    protected override Size MeasureOverride(Size constraint)
    {
        // 父容器尺寸变化时自动计算列数
        UpdateColumns(constraint.Width);
        return base.MeasureOverride(constraint);
    }

    private void UpdateColumns(double availableWidth)
    {
        if (availableWidth <= 0 || Children.Count == 0)
            return;

        double itemWidth = Children[0].DesiredSize.Width;
        if (itemWidth <= 0)
            return;

        // 计算最大列数（确保不超出可用宽度）
        int newColumns = Math.Max(1, (int)(availableWidth / itemWidth));
        Columns = newColumns;
    }
}
