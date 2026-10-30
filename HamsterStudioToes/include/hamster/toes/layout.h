#pragma once

#define ImageUtilsNamespace HamsterStudioToes::ImageTools

#include <utility>

#include <hamster/toes/export.h>

namespace ImageUtilsNamespace
{

	struct TOES_API ImageLayout { int rows, cols; };

	class TOES_API LayoutHelper
	{
	public:
		static ImageLayout CalculateGridLayout(int image_count, int cell_width, int cell_height);
	};
}
