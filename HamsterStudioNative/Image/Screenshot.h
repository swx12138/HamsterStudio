#pragma once

#include <opencv2/opencv.hpp>
#include <string>

#define ScreenshotNamespace HamsterStudio

namespace ScreenshotNamespace
{
	class Screenshot
	{
		cv::Mat m_data;

	public:
		Screenshot();
		Screenshot(int x, int y, int width, int height);

	public:
		void saveAs(std::string_view filename);
	};

}