#include "PasteImageUtils.h"

#include <algorithm>

#include <opencv2/opencv.hpp>

#include "./GradientFiller.h"
#include "../PixelData/PopularColors.hpp"

using namespace GradientFillerNamespace;
using namespace PantoneColorsNamespace;

// 通道对齐：把 image 转成与目标相同的通道数。
// 关键点是「必须在创建 ROI 之前」完成转换——直接对 ROI 调 cvtColor，目标类型
// 一旦不匹配就会重新分配 Mat，那块 ROI 便与 canvas 脱钩，粘贴结果静默丢失。
static cv::Mat AlignChannels(const cv::Mat& image, int targetChannels)
{
	if (image.channels() == targetChannels) {
		return image;
	}

	cv::Mat converted;
	if (image.channels() == 1 && targetChannels == 3) {
		cv::cvtColor(image, converted, cv::COLOR_GRAY2BGR);
	}
	else if (image.channels() == 1 && targetChannels == 4) {
		cv::cvtColor(image, converted, cv::COLOR_GRAY2BGRA);
	}
	else if (image.channels() == 3 && targetChannels == 4) {
		cv::cvtColor(image, converted, cv::COLOR_BGR2BGRA);
	}
	else if (image.channels() == 4 && targetChannels == 3) {
		cv::cvtColor(image, converted, cv::COLOR_BGRA2BGR);
	}
	else if (image.channels() == 4 && targetChannels == 1) {
		cv::cvtColor(image, converted, cv::COLOR_BGRA2GRAY);
	}
	else if (image.channels() == 3 && targetChannels == 1) {
		cv::cvtColor(image, converted, cv::COLOR_BGR2GRAY);
	}
	else {
		return image;
	}
	return converted;
}

// 贴之前保证源与目标尺寸完全一致，copyTo 才会写进 canvas 原有的缓冲区
static void PasteScaled(cv::Mat& canvas, const cv::Mat& image)
{
	cv::Mat fitted = AlignChannels(image, canvas.channels());
	if (fitted.size() != canvas.size()) {
		cv::resize(fitted, fitted, canvas.size(), 0, 0, cv::INTER_AREA);
	}
	fitted.copyTo(canvas);
}

// 源比目标大时取中心区域裁切，再居中贴上去（不缩放）
static void PasteCroppedCentered(cv::Mat& canvas, const cv::Mat& image)
{
	cv::Mat aligned = AlignChannels(image, canvas.channels());

	int const w = std::min(canvas.cols, aligned.cols);
	int const h = std::min(canvas.rows, aligned.rows);

	cv::Rect const srcRc((aligned.cols - w) / 2, (aligned.rows - h) / 2, w, h);
	cv::Rect const dstRc((canvas.cols - w) / 2, (canvas.rows - h) / 2, w, h);

	aligned(srcRc).copyTo(canvas(dstRc));
}

static void PasteImageUniformToFill(cv::Mat& canvas, const cv::Mat& image)
{
	// 等比放大到刚好铺满画布，多出来的部分居中裁掉
	double const scale = std::max(
		static_cast<double>(canvas.cols) / image.cols,
		static_cast<double>(canvas.rows) / image.rows);

	cv::Mat scaled;
	cv::resize(image, scaled, cv::Size(), scale, scale, cv::INTER_AREA);

	// 缩放到「至少覆盖画布」之后，这里只剩居中裁剪
	PasteCroppedCentered(canvas, scaled);
}

static void PasteImageUniform(cv::Mat& canvas, const cv::Mat& image)
{
	// 等比缩小到能完整放进画布，四周留边
	double const scale = std::min(
		static_cast<double>(canvas.cols) / image.cols,
		static_cast<double>(canvas.rows) / image.rows);

	cv::Mat scaled;
	cv::resize(image, scaled, cv::Size(), scale, scale, cv::INTER_AREA);

	// scaled 不会大于画布，这里只是把它居中放进去
	PasteCroppedCentered(canvas, scaled);
}

static void PasteImageFill(cv::Mat& canvas, const cv::Mat& image)
{
	// 直接拉伸到画布尺寸
	cv::Mat stretched;
	cv::resize(image, stretched, canvas.size(), 0, 0, cv::INTER_AREA);
	PasteScaled(canvas, stretched);
}

static void PasteImageNone(cv::Mat& canvas, const cv::Mat& image)
{
	// 不缩放，居中放置；任一边超出就居中裁剪
	PasteCroppedCentered(canvas, image);
}

void PASTE_IMAGE_UTILS_NAMESPACE::ImagePaster::Paste(void* pCanvasMat, void const* pMat, StretchMode stretchMode)
{
	auto& canvas = *reinterpret_cast<cv::Mat*>(pCanvasMat);
	if (canvas.empty()) {
		return;
	}

	DrawFilledRoundRect(
		canvas,
		cv::Point(0, 0), canvas.size(),
		std::min(canvas.cols, canvas.rows) / 20,
		PantoneColors::YearColor_2026_CloudDancer);

	auto const& image = *reinterpret_cast<cv::Mat const*>(pMat);
	if (image.empty()) {
		return;
	}

	switch (stretchMode) {
	case StretchMode::Uniform:
		PasteImageUniform(canvas, image);
		break;
	case StretchMode::UniformToFill:
		PasteImageUniformToFill(canvas, image);
		break;
	case StretchMode::Fill:
		PasteImageFill(canvas, image);
		break;
	default:
		PasteImageNone(canvas, image);
		break;
	}
}
