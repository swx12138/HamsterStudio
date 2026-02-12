#pragma once

#include <opencv2/opencv.hpp>

#include <cstdint>
#include <vector>
#include <memory>

#ifdef IMAGEPROCESSOR_EXPORTS
#define IMAGEPROCESSOR_API __declspec(dllexport)
#else
#define IMAGEPROCESSOR_API __declspec(dllimport)
#endif

namespace ImageProcessor
{
	struct ImageAdjustments {
		double exposure = 0.0;
		double temperature = 0.0;
		double tint = 0.0;
		double highlights = 0.0;
		double shadows = 0.0;
		double whites = 0.0;
		double blacks = 0.0;
		double saturation = 0.0;
		double vibrance = 0.0;
	};

	// OpenCV图像包装器
	struct ImageData {
		cv::Mat mat;  // BGRA格式的OpenCV Mat

		ImageData() = default;
		ImageData(uint8_t *pixels, int32_t width, int32_t height, int32_t stride)
			: mat(cv::Mat(height, width, CV_8UC4, pixels, stride)) { }

		bool isValid() const { return !mat.empty(); }
	};

	struct HistogramData {
		int32_t red[256] = { 0 };
		int32_t green[256] = { 0 };
		int32_t blue[256] = { 0 };
		int32_t luminance[256] = { 0 };
	};

	class IMAGEPROCESSOR_API ImageProcessor {
	public:
		ImageProcessor();
		~ImageProcessor();

		bool processImage(ImageData &image, const ImageAdjustments &adjustments);
		bool processImageMT(ImageData &image, const ImageAdjustments &adjustments, int threadCount = 0);
		HistogramData calculateHistogram(const ImageData &image);

		// 性能统计
		struct PerformanceStats {
			double processingTimeMs = 0.0;
			double pixelsPerSecond = 0.0;
			int64_t pixelsProcessed = 0;
		};

		PerformanceStats getStats() const { return stats_; }

	private:
		// OpenCV核心处理函数
		void applyExposure(cv::Mat &image, double exposure);
		void applySaturation(cv::Mat &image, double saturation);
		void applyVibrance(cv::Mat &image, double vibrance);
		void applyTemperatureTint(cv::Mat &image, double temperature, double tint);
		void applyToneAdjustments(cv::Mat &image, const ImageAdjustments &adj);

		// 直方图计算优化版本
		HistogramData calculateHistogramOptimized(const cv::Mat &image);

		// 并行处理辅助函数
		void processChunk(cv::Mat &chunk, const ImageAdjustments &adj);

		PerformanceStats stats_;

		// 查找表（可选保留）
		cv::Mat exposureLUT_;
		cv::Mat temperatureLUT_;

		ImageProcessor(const ImageProcessor &) = delete;
		ImageProcessor &operator=(const ImageProcessor &) = delete;
	};

	IMAGEPROCESSOR_API std::unique_ptr<ImageProcessor> createImageProcessor();

	extern "C" {
		// C接口保持不变
		IMAGEPROCESSOR_API void *image_processor_create();
		IMAGEPROCESSOR_API void image_processor_destroy(void *processor);
		IMAGEPROCESSOR_API bool image_processor_process(
			void *processor,
			uint8_t *pixels,
			int32_t width,
			int32_t height,
			int32_t stride,
			const ImageAdjustments *adjustments);

		IMAGEPROCESSOR_API bool image_processor_get_histogram(
			void *processor,
			const uint8_t *pixels,
			int32_t width,
			int32_t height,
			int32_t stride,
			int32_t *redHist,
			int32_t *greenHist,
			int32_t *blueHist,
			int32_t *luminanceHist);
	}
}