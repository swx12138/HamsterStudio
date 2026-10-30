#pragma once

#include <opencv2/core/mat.hpp>

#include <cstdint>
#include <vector>
#include <memory>

namespace toes::imaging
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

	class ImageProcessor {
	public:
		explicit ImageProcessor() noexcept;
		~ImageProcessor() noexcept;

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
		// 把 8UC4 / 8UC3 拆成「3 通道 BGR + 可选 alpha」，处理完再合并回 image.mat。
		// 之所以绕这一道：cvtColor 直接写进 4 通道 Mat 时会把 Mat 重新分配，
		// 于是调用方传进来的像素内存一个字节都不会被写入——那正是 ImageData 的契约。
		static bool SplitToBGR(const cv::Mat &src, cv::Mat &bgr, cv::Mat &alpha);
		static void MergeFromBGR(cv::Mat &dst, const cv::Mat &bgr, const cv::Mat &alpha);

		// 纯像素运算，不读写任何成员，因此可以安全地在分块并行里并发调用
		void processBGR(cv::Mat &bgr, const ImageAdjustments &adj) const;

		// 处理一整行分块。抽出来是为了让「指定线程数 / 不指定线程数」两个 pragma
		// 共用同一份循环体（OpenMP 的 num_threads(0) 是非法值）
		void processTileRow(cv::Mat &bgr, const ImageAdjustments &adj, int y, int tileSize) const;

		void applyExposure(cv::Mat &bgr, double exposure) const;
		void applyTemperatureTint(cv::Mat &bgr, double temperature, double tint) const;
		void applyToneAdjustments(cv::Mat &bgr, const ImageAdjustments &adj) const;

		HistogramData calculateHistogramOptimized(const cv::Mat &image);

		void updateStats(const cv::Mat &mat, double elapsedMs);

		PerformanceStats stats_;

		ImageProcessor(const ImageProcessor &) = delete;
		ImageProcessor &operator=(const ImageProcessor &) = delete;
	};

	std::unique_ptr<ImageProcessor> CreateImageProcessor();

}
