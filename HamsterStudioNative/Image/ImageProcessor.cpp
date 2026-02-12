#define IMAGEPROCESSOR_EXPORTS

#include "ImageProcessor.h"
#include <chrono>
#include <algorithm>
#include <numeric>
#include <execution>

// OpenCV包含
#include <opencv2/core.hpp>
#include <opencv2/imgproc.hpp>
#include <opencv2/core/hal/intrin.hpp>  // SIMD内部函数

#ifdef _DEBUG
#pragma comment(lib, "opencv_world4100d.lib")
#else
#pragma comment(lib, "opencv_world4100.lib")
#endif // _DEBUG


ImageProcessor::ImageProcessor::ImageProcessor() {
	// 初始化OpenCV相关资源
	cv::setNumThreads(0);  // 默认使用OpenCV内部线程管理
}

ImageProcessor::ImageProcessor::~ImageProcessor() = default;

bool ImageProcessor::ImageProcessor::processImage(ImageData &image, const ImageAdjustments &adjustments) {
	if (!image.isValid()) return false;

	auto startTime = std::chrono::high_resolution_clock::now();

	cv::Mat &mat = image.mat;

	// 1. 曝光调整（使用LUT加速）
	if (adjustments.exposure != 0.0) {
		applyExposure(mat, adjustments.exposure);
	}

	// 2. 色温和色调
	if (adjustments.temperature != 0.0 || adjustments.tint != 0.0) {
		applyTemperatureTint(mat, adjustments.temperature, adjustments.tint);
	}

	// 3. 转换为HSV进行饱和度和鲜艳度调整
	if (adjustments.saturation != 0.0 || adjustments.vibrance != 0.0) {
		cv::Mat hsv;
		cv::cvtColor(mat, hsv, cv::COLOR_BGR2HSV);

		std::vector<cv::Mat> channels;
		cv::split(hsv, channels);

		// 饱和度调整
		if (adjustments.saturation != 0.0) {
			double scale = 1.0 + adjustments.saturation / 100.0;
			channels[1] = channels[1] * scale;
			cv::min(channels[1], 255, channels[1]);
		}

		// 鲜艳度调整（只调整低饱和度区域）
		if (adjustments.vibrance != 0.0) {
			cv::Mat lowSaturationMask = channels[1] < 25;
			double vibranceScale = 1.0 + adjustments.vibrance / 100.0 * 0.5;
			channels[1].setTo(channels[1] * vibranceScale, lowSaturationMask);
			cv::min(channels[1], 255, channels[1]);
		}

		cv::merge(channels, hsv);
		cv::cvtColor(hsv, mat, cv::COLOR_HSV2BGR);
	}

	// 4. 色调调整（高光/阴影等）
	if (adjustments.highlights != 0.0 || adjustments.shadows != 0.0 ||
		adjustments.whites != 0.0 || adjustments.blacks != 0.0) {
		applyToneAdjustments(mat, adjustments);
	}

	auto endTime = std::chrono::high_resolution_clock::now();
	std::chrono::duration<double, std::milli> duration = endTime - startTime;

	// 更新统计
	stats_.processingTimeMs = duration.count();
	stats_.pixelsProcessed = static_cast<int64_t>(mat.total());
	stats_.pixelsPerSecond = stats_.pixelsProcessed / (duration.count() / 1000.0);

	return true;
}

bool ImageProcessor::ImageProcessor::processImageMT(ImageData &image, const ImageAdjustments &adjustments, int threadCount) {
	if (!image.isValid()) return false;

	auto startTime = std::chrono::high_resolution_clock::now();

	cv::Mat &mat = image.mat;

	// 设置OpenCV线程数
	if (threadCount > 0) {
		cv::setNumThreads(threadCount);
	}

	// 分块并行处理
	const int tileSize = 256;
	const int rows = mat.rows;
	const int cols = mat.cols;

#pragma omp parallel for collapse(2) if(rows * cols > 10000)
	for (int y = 0; y < rows; y += tileSize) {
		for (int x = 0; x < cols; x += tileSize) {
			int tileHeight = std::min(tileSize, rows - y);
			int tileWidth = std::min(tileSize, cols - x);

			cv::Rect tileRect(x, y, tileWidth, tileHeight);
			cv::Mat tile = mat(tileRect);

			// 处理当前tile
			ImageData tileData;
			tileData.mat = tile;
			processImage(tileData, adjustments);
		}
	}

	auto endTime = std::chrono::high_resolution_clock::now();
	std::chrono::duration<double, std::milli> duration = endTime - startTime;

	// 更新统计
	stats_.processingTimeMs = duration.count();
	stats_.pixelsProcessed = static_cast<int64_t>(mat.total());
	stats_.pixelsPerSecond = stats_.pixelsProcessed / (duration.count() / 1000.0);

	return true;
}

void ImageProcessor::ImageProcessor::applyExposure(cv::Mat &image, double exposure) {
	double factor = std::pow(2.0, exposure);

	if (exposureLUT_.empty()) {
		exposureLUT_.create(1, 256, CV_8UC3);
		uint8_t *p = exposureLUT_.ptr();
		for (int i = 0; i < 256; ++i) {
			for (int c = 0; c < 3; ++c) {
				p[i * 3 + c] = cv::saturate_cast<uint8_t>(i * factor);
			}
		}
	}

	cv::LUT(image, exposureLUT_, image);
}

void ImageProcessor::ImageProcessor::applyTemperatureTint(cv::Mat &image, double temperature, double tint) {
	// 色温：增加红色，减少蓝色
	// 色调：增加红色，减少绿色
	cv::Mat adjustment = cv::Mat::zeros(1, 3, CV_64F);

	// 色温调整
	adjustment.at<double>(0, 2) += temperature * 0.25;  // 增加红色
	adjustment.at<double>(0, 0) -= temperature * 0.5;   // 减少蓝色

	// 色调调整
	adjustment.at<double>(0, 2) += tint * 0.25;         // 增加红色
	adjustment.at<double>(0, 1) -= tint * 0.5;          // 减少绿色

	// 应用调整
	cv::add(image, cv::Scalar(adjustment.at<double>(0, 0),
		adjustment.at<double>(0, 1),
		adjustment.at<double>(0, 2)), image);
}

void ImageProcessor::ImageProcessor::applyToneAdjustments(cv::Mat &image, const ImageAdjustments &adj) {
	// 转换为Lab颜色空间进行更精确的亮度调整
	cv::Mat lab;
	cv::cvtColor(image, lab, cv::COLOR_BGR2Lab);

	std::vector<cv::Mat> labChannels;
	cv::split(lab, labChannels);
	cv::Mat &lightness = labChannels[0];

	// 高光调整
	if (adj.highlights != 0.0) {
		cv::Mat highlightsMask = lightness > 200;
		double factor = 1.0 + adj.highlights / 100.0;
		lightness.setTo(lightness * factor, highlightsMask);
	}

	// 阴影调整
	if (adj.shadows != 0.0) {
		cv::Mat shadowsMask = lightness < 50;
		double factor = 1.0 + adj.shadows / 100.0;
		lightness.setTo(lightness * factor, shadowsMask);
	}

	// 白色调整
	if (adj.whites != 0.0) {
		cv::Mat whitesMask = lightness > 240;
		double boost = adj.whites / 100.0 * 15.0;  // 最大增加15
		lightness.setTo(cv::min(lightness + boost, 255.0), whitesMask);
	}

	// 黑色调整
	if (adj.blacks != 0.0) {
		cv::Mat blacksMask = lightness < 15;
		double reduction = adj.blacks / 100.0 * 15.0;  // 最大减少15
		lightness.setTo(cv::max(lightness - reduction, 0.0), blacksMask);
	}

	cv::merge(labChannels, lab);
	cv::cvtColor(lab, image, cv::COLOR_Lab2BGR);
}

ImageProcessor::HistogramData ImageProcessor::ImageProcessor::calculateHistogram(const ImageData &image) {
	if (!image.isValid()) return HistogramData();

	return calculateHistogramOptimized(image.mat);
}

ImageProcessor::HistogramData ImageProcessor::ImageProcessor::calculateHistogramOptimized(const cv::Mat &image) {
	HistogramData hist;

	if (image.empty() || image.channels() != 4) return hist;

	// 分离通道
	std::vector<cv::Mat> channels;
	cv::split(image, channels);

	// 计算每个通道的直方图（OpenCV并行优化）
	const int histSize = 256;
	const float range[] = { 0, 256 };
	const float *histRange = { range };

	// 计算RGB直方图
	//cv::calcHist(&channels[0], 1, 0, cv::Mat(), hist.blue, 1, &histSize, &histRange);
	//cv::calcHist(&channels[1], 1, 0, cv::Mat(), hist.green, 1, &histSize, &histRange);
	//cv::calcHist(&channels[2], 1, 0, cv::Mat(), hist.red, 1, &histSize, &histRange);

	// 计算亮度直方图
	cv::Mat gray;
	cv::cvtColor(image, gray, cv::COLOR_BGRA2GRAY);
	//cv::calcHist(&gray, 1, 0, cv::Mat(), hist.luminance, 1, &histSize, &histRange);

	return hist;
}

// 工厂函数
std::unique_ptr<ImageProcessor::ImageProcessor> createImageProcessor() {
	return std::make_unique<ImageProcessor::ImageProcessor>();
}

// C接口实现（保持不变）
extern "C" {
	IMAGEPROCESSOR_API void *image_processor_create() {
		return new ImageProcessor::ImageProcessor();
	}

	IMAGEPROCESSOR_API void image_processor_destroy(void *processor) {
		delete static_cast<ImageProcessor::ImageProcessor *>(processor);
	}

	IMAGEPROCESSOR_API bool image_processor_process(
		void *processor,
		uint8_t *pixels,
		int32_t width,
		int32_t height,
		int32_t stride,
		const ImageProcessor::ImageAdjustments *adjustments) {

		if (!processor || !pixels || !adjustments) return false;

		ImageProcessor::ImageData image(pixels, width, height, stride);
		auto *proc = static_cast<ImageProcessor::ImageProcessor *>(processor);

		return proc->processImageMT(image, *adjustments);
	}

	IMAGEPROCESSOR_API bool image_processor_get_histogram(
		void *processor,
		const uint8_t *pixels,
		int32_t width,
		int32_t height,
		int32_t stride,
		int32_t *redHist,
		int32_t *greenHist,
		int32_t *blueHist,
		int32_t *luminanceHist) {

		if (!processor || !pixels || !redHist || !greenHist || !blueHist || !luminanceHist) {
			return false;
		}

		ImageProcessor::ImageData image(const_cast<uint8_t *>(pixels), width, height, stride);
		auto *proc = static_cast<ImageProcessor::ImageProcessor *>(processor);

		auto hist = proc->calculateHistogram(image);

		// 复制数据
		for (int i = 0; i < 256; ++i) {
			redHist[i] = static_cast<int32_t>(hist.red[i]);
			greenHist[i] = static_cast<int32_t>(hist.green[i]);
			blueHist[i] = static_cast<int32_t>(hist.blue[i]);
			luminanceHist[i] = static_cast<int32_t>(hist.luminance[i]);
		}

		return true;
	}

}
