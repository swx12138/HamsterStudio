#define IMAGEPROCESSOR_EXPORTS

#include "imaging/adjustments.h"
#include <chrono>
#include <algorithm>
#include <numeric>
#include <execution>

// OpenCV包含
#include <opencv2/core.hpp>
#include <opencv2/imgproc.hpp>
#include <opencv2/core/hal/intrin.hpp>  // SIMD内部函数

namespace {
	// 8 位通道的取值范围，LUT 与直方图都用它
	constexpr int kLevels = 256;
}

toes::imaging::ImageProcessor::ImageProcessor() noexcept {
	// 这里原本调用 cv::setNumThreads(0)，而 0 在 OpenCV 里表示「关闭并行」，
	// 与本类声称的并行处理正好相反。不调用才是「交给 OpenCV 自己管理线程」。
}

toes::imaging::ImageProcessor::~ImageProcessor() noexcept = default;

bool toes::imaging::ImageProcessor::SplitToBGR(const cv::Mat &src, cv::Mat &bgr, cv::Mat &alpha)
{
	if (src.empty() || src.depth() != CV_8U) {
		return false;
	}

	if (src.channels() == 3) {
		bgr = src.clone();
		alpha.release();
		return true;
	}

	if (src.channels() != 4) {
		return false;
	}

	std::vector<cv::Mat> channels;
	cv::split(src, channels);
	alpha = channels[3];
	cv::merge(std::vector<cv::Mat>{ channels[0], channels[1], channels[2] }, bgr);
	return true;
}

void toes::imaging::ImageProcessor::MergeFromBGR(cv::Mat &dst, const cv::Mat &bgr, const cv::Mat &alpha)
{
	cv::Mat merged;
	if (alpha.empty()) {
		merged = bgr;
	}
	else {
		std::vector<cv::Mat> channels{ bgr, alpha };
		cv::merge(channels, merged);
	}

	// 只有尺寸和类型都对上才写回原缓冲区；否则 replace 头部（正常路径不会走到）
	if (merged.size() == dst.size() && merged.type() == dst.type()) {
		merged.copyTo(dst);
	}
	else {
		dst = merged;
	}
}

void toes::imaging::ImageProcessor::processBGR(cv::Mat &bgr, const ImageAdjustments &adj) const
{
	// 1. 曝光调整
	if (adj.exposure != 0.0) {
		applyExposure(bgr, adj.exposure);
	}

	// 2. 色温和色调
	if (adj.temperature != 0.0 || adj.tint != 0.0) {
		applyTemperatureTint(bgr, adj.temperature, adj.tint);
	}

	// 3. 饱和度与鲜艳度
	if (adj.saturation != 0.0 || adj.vibrance != 0.0) {
		cv::Mat hsv;
		cv::cvtColor(bgr, hsv, cv::COLOR_BGR2HSV);

		std::vector<cv::Mat> channels;
		cv::split(hsv, channels);
		cv::Mat &s = channels[1];

		if (adj.saturation != 0.0) {
			// 先用独立的 dst 算好再赋值，避免原地读写同一块内存
			cv::Mat scaled;
			s.convertTo(scaled, -1, 1.0 + adj.saturation / 100.0);
			s = scaled;
		}

		if (adj.vibrance != 0.0) {
			cv::Mat const lowSaturationMask = s < 25;
			double const vibranceScale = 1.0 + adj.vibrance / 100.0 * 0.5;

			cv::Mat boosted;
			cv::multiply(s, cv::Scalar::all(vibranceScale), boosted);
			boosted.copyTo(s, lowSaturationMask);
		}

		cv::merge(channels, hsv);
		cv::cvtColor(hsv, bgr, cv::COLOR_HSV2BGR);
	}

	// 4. 亮度范围调整（高光/阴影/白色/黑色）
	if (adj.highlights != 0.0 || adj.shadows != 0.0 ||
		adj.whites != 0.0 || adj.blacks != 0.0) {
		applyToneAdjustments(bgr, adj);
	}
}

void toes::imaging::ImageProcessor::processTileRow(cv::Mat &bgr, const ImageAdjustments &adj, int y, int tileSize) const
{
	int const rows = bgr.rows;
	int const cols = bgr.cols;

	for (int x = 0; x < cols; x += tileSize) {
		cv::Rect const tileRect(x, y, std::min(tileSize, cols - x), std::min(tileSize, rows - y));
		cv::Mat tile = bgr(tileRect);
		processBGR(tile, adj);
	}
}

bool toes::imaging::ImageProcessor::processImage(ImageData &image, const ImageAdjustments &adjustments) {
	if (!image.isValid()) return false;

	auto const startTime = std::chrono::high_resolution_clock::now();

	cv::Mat bgr, alpha;
	if (!SplitToBGR(image.mat, bgr, alpha)) return false;

	processBGR(bgr, adjustments);
	MergeFromBGR(image.mat, bgr, alpha);

	auto const endTime = std::chrono::high_resolution_clock::now();
	std::chrono::duration<double, std::milli> const duration = endTime - startTime;
	updateStats(image.mat, duration.count());

	return true;
}

bool toes::imaging::ImageProcessor::processImageMT(ImageData &image, const ImageAdjustments &adjustments, int threadCount) {
	if (!image.isValid()) return false;

	auto const startTime = std::chrono::high_resolution_clock::now();

	cv::Mat bgr, alpha;
	if (!SplitToBGR(image.mat, bgr, alpha)) return false;

	constexpr int tileSize = 256;
	int const rows = bgr.rows;
	int const cols = bgr.cols;

	// 这里不再调用 cv::setNumThreads(threadCount)：那是改进程级的全局状态，会波及
	// 同一进程里所有其他 OpenCV 使用者。threadCount 现在只作用于下面的分块并行。
	//
	// 各分块写的是互不相交的 ROI，processBGR 又是 const、不碰任何成员，统计信息在
	// 循环结束后统一更新，数据竞争不存在。
	if (threadCount > 0) {
#pragma omp parallel for num_threads(threadCount) if(rows * cols > 10000)
		for (int y = 0; y < rows; y += tileSize) {
			processTileRow(bgr, adjustments, y, tileSize);
		}
	}
	else {
#pragma omp parallel for if(rows * cols > 10000)
		for (int y = 0; y < rows; y += tileSize) {
			processTileRow(bgr, adjustments, y, tileSize);
		}
	}

	MergeFromBGR(image.mat, bgr, alpha);

	auto const endTime = std::chrono::high_resolution_clock::now();
	std::chrono::duration<double, std::milli> const duration = endTime - startTime;
	updateStats(image.mat, duration.count());

	return true;
}

void toes::imaging::ImageProcessor::updateStats(const cv::Mat &mat, double elapsedMs)
{
	stats_.processingTimeMs = elapsedMs;
	stats_.pixelsProcessed = static_cast<int64_t>(mat.total());
	stats_.pixelsPerSecond = stats_.pixelsProcessed / std::max(elapsedMs / 1000.0, 1e-9);
}

void toes::imaging::ImageProcessor::applyExposure(cv::Mat &bgr, double exposure) const {
	double const factor = std::pow(2.0, exposure);

	// LUT 每次按当前曝光值重建。原来把它缓存成成员，第二次换曝光值仍沿用第一次的表。
	cv::Mat lut(1, kLevels, CV_8UC1);
	uchar *p = lut.ptr<uchar>();
	for (int i = 0; i < kLevels; ++i) {
		p[i] = cv::saturate_cast<uchar>(i * factor);
	}

	// 单通道 LUT 会作用到 BGR 三个通道上
	cv::Mat out;
	cv::LUT(bgr, lut, out);
	out.copyTo(bgr);
}

void toes::imaging::ImageProcessor::applyTemperatureTint(cv::Mat &bgr, double temperature, double tint) const {
	// 色温：加红、减蓝；色调：加红、减绿（BGR 顺序为 0=B 1=G 2=R）
	double const deltaBlue = -temperature * 0.5;
	double const deltaGreen = -tint * 0.5;
	double const deltaRed = temperature * 0.25 + tint * 0.25;

	cv::Mat out;
	cv::add(bgr, cv::Scalar(deltaBlue, deltaGreen, deltaRed), out);
	out.copyTo(bgr);
}

void toes::imaging::ImageProcessor::applyToneAdjustments(cv::Mat &bgr, const ImageAdjustments &adj) const {
	// 转到 Lab 空间，只动 L 通道
	cv::Mat lab;
	cv::cvtColor(bgr, lab, cv::COLOR_BGR2Lab);

	std::vector<cv::Mat> labChannels;
	cv::split(lab, labChannels);
	cv::Mat &lightness = labChannels[0];

	if (adj.highlights != 0.0) {
		cv::Mat const mask = lightness > 200;
		cv::Mat adjusted;
		lightness.convertTo(adjusted, -1, 1.0 + adj.highlights / 100.0);
		adjusted.copyTo(lightness, mask);
	}

	if (adj.shadows != 0.0) {
		cv::Mat const mask = lightness < 50;
		cv::Mat adjusted;
		lightness.convertTo(adjusted, -1, 1.0 + adj.shadows / 100.0);
		adjusted.copyTo(lightness, mask);
	}

	if (adj.whites != 0.0) {
		cv::Mat const mask = lightness > 240;
		cv::Mat adjusted;
		cv::add(lightness, cv::Scalar::all(adj.whites / 100.0 * 15.0), adjusted);
		adjusted.copyTo(lightness, mask);
	}

	if (adj.blacks != 0.0) {
		cv::Mat const mask = lightness < 15;
		cv::Mat adjusted;
		cv::subtract(lightness, cv::Scalar::all(adj.blacks / 100.0 * 15.0), adjusted);
		adjusted.copyTo(lightness, mask);
	}

	cv::Mat out;
	cv::merge(labChannels, lab);
	cv::cvtColor(lab, out, cv::COLOR_Lab2BGR);
	out.copyTo(bgr);
}

toes::imaging::HistogramData toes::imaging::ImageProcessor::calculateHistogram(const ImageData &image) {
	if (!image.isValid()) return HistogramData();

	return calculateHistogramOptimized(image.mat);
}

toes::imaging::HistogramData toes::imaging::ImageProcessor::calculateHistogramOptimized(const cv::Mat &image) {
	HistogramData hist;

	if (image.empty() || image.depth() != CV_8U) return hist;
	if (image.channels() != 3 && image.channels() != 4) return hist;

	std::vector<cv::Mat> channels;
	cv::split(image, channels);

	int const histSize[] = { kLevels };
	float const range[] = { 0, 256 };
	float const *histRange[] = { range };
	int const channel0[] = { 0 };

	auto fill = [&](const cv::Mat &plane, int32_t *dst) {
		cv::Mat h;
		cv::calcHist(&plane, 1, channel0, cv::Mat(), h, 1, histSize, histRange);
		for (int i = 0; i < kLevels; ++i) {
			dst[i] = cv::saturate_cast<int32_t>(h.at<float>(i));
		}
	};

	fill(channels[0], hist.blue);
	fill(channels[1], hist.green);
	fill(channels[2], hist.red);

	cv::Mat gray;
	cv::cvtColor(image, gray, image.channels() == 4 ? cv::COLOR_BGRA2GRAY : cv::COLOR_BGR2GRAY);
	fill(gray, hist.luminance);

	return hist;
}

// 工厂函数
std::unique_ptr<toes::imaging::ImageProcessor> toes::imaging::CreateImageProcessor() {
	return std::make_unique<toes::imaging::ImageProcessor>();
}
