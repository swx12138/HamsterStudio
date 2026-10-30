#pragma once

#include <opencv2/core.hpp>

#include <hamster/toes/export.h>

namespace toes::colors
{
	class TOES_API HueSaturationValue
	{

	public:
		static cv::Mat CreateColorCircle(bool withSkinZone = true, int radius = 500, int padding = 12);
		static cv::Mat CreateHistogram(const cv::Mat& bgr, bool withSkinZone = true, int radius = 500, int padding = 12);


	};

}
