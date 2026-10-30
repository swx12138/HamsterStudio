#pragma unmanaged

#include <hamster/toes/layout.h>

using namespace toes::image;

#include <cmath>

ImageLayout LayoutHelper::CalculateGridLayout(int image_count, int cell_width, int cell_height)
{
	if (image_count < 4) {
		if (cell_width > cell_height) {
			return { image_count, 1 }; // 横屏且图片较少，单列展示
		}
		else {
			return { 1, image_count }; // 竖屏且图片较少，单行展示
		}
	}
	else {
		// 正常计算布局
		int cols = static_cast<int>(sqrt(image_count));
		if (cols * cols < image_count) {
			cols += 1; // 如果列数的平方小于图片数量，增加一列
		}

		int rows = (image_count + cols - 1) / cols; // 向上取整
		return { rows,cols };
	}
}
