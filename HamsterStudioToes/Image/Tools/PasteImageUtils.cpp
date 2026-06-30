#include "PasteImageUtils.h"

#include <opencv2/opencv.hpp>

#include "./GradientFiller.h"
#include "../PixelData/PopularColors.hpp"

using namespace GradientFillerNamespace;
using namespace PantoneColorsNamespace;

static void PasteImageUniformToFill(cv::Mat& canvas, const cv::Mat& image)
{
	// 均匀缩放并填充整个画布，超出部分裁剪，居中显示
	double scale_x = static_cast<double>(canvas.cols) / image.cols;
	double scale_y = static_cast<double>(canvas.rows) / image.rows;
	double scale = std::max(scale_x, scale_y);  // 使用 max 确保填满

	cv::Mat scaled_image;
	cv::resize(image, scaled_image, cv::Size{}, scale, scale);

	// 计算裁剪区域（居中）
	int x_offset = (scaled_image.cols - canvas.cols) / 2;
	int y_offset = (scaled_image.rows - canvas.rows) / 2;

	// 确保偏移量不为负
	x_offset = std::max(0, x_offset);
	y_offset = std::max(0, y_offset);

	// 计算有效的裁剪区域
	int crop_width = std::min(canvas.cols, scaled_image.cols - x_offset);
	int crop_height = std::min(canvas.rows, scaled_image.rows - y_offset);

	cv::Rect crop_rect(x_offset, y_offset, crop_width, crop_height);
	cv::Mat cropped_image = scaled_image(crop_rect);

	// 如果裁剪后的尺寸不等于画布尺寸，调整位置
	if (cropped_image.cols != canvas.cols || cropped_image.rows != canvas.rows) {
		int paste_x = (canvas.cols - cropped_image.cols) / 2;
		int paste_y = (canvas.rows - cropped_image.rows) / 2;
		auto roi = canvas(cv::Rect(paste_x, paste_y, cropped_image.cols, cropped_image.rows));

		if (cropped_image.channels() == 1 && canvas.channels() == 3) {
			cv::cvtColor(cropped_image, roi, cv::COLOR_GRAY2BGR);
		}
		else {
			cropped_image.copyTo(roi);
		}
	}
	else {
		if (cropped_image.channels() == 1 && canvas.channels() == 3) {
			cv::cvtColor(cropped_image, canvas, cv::COLOR_GRAY2BGR);
		}
		else {
			cropped_image.copyTo(canvas);
		}
	}
}

static void PasteImageUniform(cv::Mat& canvas, const cv::Mat& image)
{
	double scale_x = static_cast<double>(canvas.cols) / image.cols;
	double scale_y = static_cast<double>(canvas.rows) / image.rows;
	double scale = std::min(scale_x, scale_y);

	cv::Mat scaled_image;
	cv::resize(image, scaled_image, cv::Size{}, scale, scale);

	int x_offset = (canvas.cols - scaled_image.cols) / 2;
	int y_offset = (canvas.rows - scaled_image.rows) / 2;
	auto roi = canvas(cv::Rect(x_offset, y_offset, scaled_image.cols, scaled_image.rows));
	if (scaled_image.channels() == 1 && canvas.channels() == 3) {
		cv::cvtColor(scaled_image, roi, cv::COLOR_GRAY2BGR);
	}
	else {
		scaled_image.copyTo(roi);
	}
}

static void PasteImageFill(cv::Mat& canvas, const cv::Mat& image) {
	cv::Mat stretched_image;

	// 直接拉伸填充
	cv::resize(image, stretched_image, canvas.size());

	if (stretched_image.channels() == 1 && canvas.channels() == 3) {
		cv::cvtColor(stretched_image, canvas, cv::COLOR_GRAY2BGR);
	}
	else if (stretched_image.channels() == 3 && canvas.channels() == 1) {
		cv::cvtColor(stretched_image, canvas, cv::COLOR_BGR2GRAY);
	}
	else {
		stretched_image.copyTo(canvas);
	}
}

static void PasteImageNone(cv::Mat& canvas, const cv::Mat& image) {
	if (canvas.cols >= image.cols) {
		if (canvas.rows >= image.rows) {
			// canvs比image大，直接粘贴
			cv::Rect rc{ (canvas.cols - image.cols) / 2, (canvas.rows - image.rows) / 2, image.cols, image.rows };
			image.copyTo(canvas(rc));
		}
		else {
			// canvas够宽但是不够长，使用canvas的高和image的宽
			cv::Rect canvas_rc{ (canvas.cols - image.cols) / 2, 0, image.cols, canvas.rows };
			cv::Rect image_rc{ 0, (image.rows - canvas.rows) / 2, image.cols, canvas.rows };
			image(image_rc).copyTo(canvas(canvas_rc));
		}
	}
	else {
		if (canvas.rows >= image.rows) {
			// canvas够长但是不够宽，使用canvas的宽和image的高
			cv::Rect canvas_rc{ 0, (canvas.rows - image.rows) / 2, canvas.cols, image.rows };
			cv::Rect image_rc{ (image.cols - canvas.cols) / 2, 0, canvas.cols, image.rows };
			image(image_rc).copyTo(canvas(canvas_rc));
		}
		else {
			// canvas既不够宽也不够长，使用canvas的宽和高
			cv::Rect image_rc{ abs(canvas.cols - image.cols) / 2, abs(canvas.rows - image.rows) / 2, canvas.cols, canvas.rows };
			image(image_rc).copyTo(canvas);
		}
	}
}

void PASTE_IMAGE_UTILS_NAMESPACE::ImagePaster::Paste(void* pCanvasMat, void const* pMat, StretchMode stretchMode)
{
	auto& canvas = *reinterpret_cast<cv::Mat*>(pCanvasMat);
	DrawFilledRoundRect(
		canvas,
		cv::Point(0, 0), canvas.size(),
		std::min(canvas.cols, canvas.rows) / 20,
		PantoneColors::YearColor_2026_CloudDancer);

	auto const& image = *reinterpret_cast<cv::Mat const*>(pMat);
	if (stretchMode == StretchMode::Uniform) {
		PasteImageUniform(canvas, image);
	}
	else if (stretchMode == StretchMode::UniformToFill) {
		PasteImageUniformToFill(canvas, image);
	}
	else if (stretchMode == StretchMode::Fill) {
		PasteImageFill(canvas, image);
	}
	else {
		PasteImageNone(canvas, image);
	}
}
