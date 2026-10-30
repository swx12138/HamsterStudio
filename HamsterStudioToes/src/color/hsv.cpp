#include <hamster/toes/color.h>

#include <opencv2/opencv.hpp>

// ==================== 肤色区间（可调常量）：窄范围黄种人肤色 ====================
// 角度约定：顶部 0°（红），顺时针增加；S 沿半径 0（中心）~1（边缘）
constexpr double SKIN_H_START = 0.0;   // 起始色相（度）
constexpr double SKIN_H_END = 20.0;  // 结束色相（度），注意：边框绘制暂不支持跨 0° 区间
constexpr double SKIN_S_MIN = 0.1;   // 最小饱和度（对应内半径）
constexpr double SKIN_S_MAX = 0.5;   // 最大饱和度（对应外半径）

// 在方形色轮图上叠加肤色区间标注：半透明提亮 + 白色边框（wheel 需为 CV_8UC3）
void drawSkinZone(cv::Mat& wheel, double hStart = SKIN_H_START, double hEnd = SKIN_H_END, double sMin = SKIN_S_MIN, double sMax = SKIN_S_MAX);

cv::Mat toes::colors::HueSaturationValue::CreateColorCircle(bool withSkinZone, int radius, int padding)
{
	const int cols = (radius + padding) * 2;
	const int rows = (radius + padding) * 2;

	cv::Mat colorCircle(rows, cols, CV_32FC3, cv::Scalar(0, 0, 1));
	for (int i = 0; i < cols; ++i)
	{
		for (int j = 0; j < rows; ++j)
		{
			auto yy = i - cols / 2,
				xx = j - rows / 2;
			double dist = std::sqrt(xx * xx + yy * yy);
			if (dist <= radius) {
				// double theta = ::atan2(i - cols / 2, rows / 2 - j);
				// double hue = (theta + CV_PI) / (2 * CV_PI) * 360.0; // 0~360
				double theta = ::atan2(i - cols / 2, rows / 2 - j);
				double hue = std::fmod(theta, 2 * CV_PI);
				if (hue < 0) hue += 2 * CV_PI;
				hue = hue / (2 * CV_PI) * 360.0; // 顶部=0°（红），顺时针 红→黄→绿→青→蓝→品红
				double saturation = dist / radius; // 0~1
				double value = 1.0; // 固定亮度为 1.0
				cv::Vec3d hsv(hue, saturation, value);
				colorCircle.at<cv::Vec3f>(j, i) = hsv;
			}
		}
	}

	cv::cvtColor(colorCircle, colorCircle, cv::COLOR_HSV2BGR);
	colorCircle.convertTo(colorCircle, CV_8UC3, 255.0);

	// ===== 叠加肤色区间标注 =====
	if (withSkinZone) {
		drawSkinZone(colorCircle);
	}

	return colorCircle;
}

cv::Mat toes::colors::HueSaturationValue::CreateHistogram(const cv::Mat& bgr, bool withSkinZone, int radius, int padding)
{
	const int H_BINS = 180;   // OpenCV 8U HSV：H ∈ [0,179]
	const int S_BINS = 256;   // S ∈ [0,255]

	// 1. H-S 二维直方图，归一化到 0~1
	cv::Mat hsv;
	cv::cvtColor(bgr, hsv, cv::COLOR_BGR2HSV);
	int channels[] = { 0, 1 };
	int histSize[] = { H_BINS, S_BINS };
	float hRange[] = { 0, 180 }, sRange[] = { 0, 256 };
	const float* ranges[] = { hRange, sRange };
	cv::Mat hist;
	cv::calcHist(&hsv, 1, channels, cv::Mat(), hist, 2, histSize, ranges);
	cv::GaussianBlur(hist, hist, cv::Size(3, 3), 0);   // 平滑毛刺
	cv::normalize(hist, hist, 0, 1, cv::NORM_MINMAX);

	// 2. 预计算 (H,S) → BGR 颜色查找表（整表一次转换，避免逐像素 cvtColor）
	cv::Mat lut(H_BINS, S_BINS, CV_32FC3);
	for (int h = 0; h < H_BINS; ++h)
		for (int s = 0; s < S_BINS; ++s)
			lut.at<cv::Vec3f>(h, s) = cv::Vec3f((float)h, (float)s / S_BINS, 1.f);
	cv::cvtColor(lut, lut, cv::COLOR_HSV2BGR);

	int wheelSize = radius + padding;

	// 3. 渲染色轮：角度定义与 tasteColorCircle 一致（顶部 H=0 红，顺时针增加）
	const double R = wheelSize / 2.0;
	cv::Mat wheel(wheelSize, wheelSize, CV_32FC3, cv::Scalar(1, 1, 1)); // 圆外白色
	for (int y = 0; y < wheelSize; ++y) {
		for (int x = 0; x < wheelSize; ++x) {
			double dx = x + 0.5 - R, dy = R - (y + 0.5);   // 顶部为正
			double dist = std::sqrt(dx * dx + dy * dy);
			if (dist > R) continue;

			double theta = std::fmod(::atan2(dx, dy), 2 * CV_PI);
			if (theta < 0) theta += 2 * CV_PI;
			double hue = theta / (2 * CV_PI) * 180.0;      // 0~180

			int hb = cvRound(hue * (H_BINS - 1) / 180.0);
			int sb = cvRound(dist / R * (S_BINS - 1));
			double cnt = std::sqrt(hist.at<float>(hb, sb)); // 幂压缩，防峰值一片白

			wheel.at<cv::Vec3f>(y, x) = lut.at<cv::Vec3f>(hb, sb) * (float)cnt;
		}
	}

	cv::Mat out;
	wheel.convertTo(out, CV_8U, 255.0);

	// ===== 叠加肤色区间标注 =====
	if (withSkinZone) {
		drawSkinZone(out);
	}

	return out;
}

// 在方形色轮图上叠加肤色区间标注：半透明提亮 + 白色边框（wheel 需为 CV_8UC3）
void drawSkinZone(cv::Mat& wheel, double hStart, double hEnd, double sMin, double sMax)
{
	const int cols = wheel.cols, rows = wheel.rows;
	const cv::Point2i center(cols / 2, rows / 2);
	const double radius = std::min(cols, rows) / 2.0;
	const int rIn = cvRound(radius * sMin);   // 内半径（S 下界）
	const int rOut = cvRound(radius * sMax);   // 外半径（S 上界）

	// 1. 生成肤色扇区掩码（角度定义与生成色轮一致：顶部 0°，顺时针增加）
	cv::Mat skinMask(rows, cols, CV_8UC1, cv::Scalar(0));
	for (int j = 0; j < rows; ++j) {
		for (int i = 0; i < cols; ++i) {
			double dx = (double)(i - center.x);
			double dy = (double)(center.y - j);   // 屏幕 y 向下，取反使顶部为正
			double dist = std::sqrt(dx * dx + dy * dy);
			if (dist < rIn || dist > rOut) continue;

			double hue = std::fmod(::atan2(dx, dy), 2 * CV_PI);
			if (hue < 0) hue += 2 * CV_PI;
			hue = hue / (2 * CV_PI) * 360.0;

			bool inHue = (hStart <= hEnd)
				? (hue >= hStart && hue <= hEnd)
				: (hue >= hStart || hue <= hEnd);
			if (inHue) skinMask.at<uchar>(j, i) = 255;
		}
	}

	// 2. 半透明提亮肤色区域（与白色 1:1 混合）
	cv::Mat blended;
	cv::addWeighted(wheel, 0.5,
		cv::Mat(rows, cols, CV_8UC3, cv::Scalar(255, 255, 255)), 0.5,
		0.0, blended);
	blended.copyTo(wheel, skinMask);

	// 3. 白色边框：两条弧线 + 两条径向边界线
	//    OpenCV 椭圆角：0° 在正右方、沿屏幕顺时针增加 → 色相角需 -90° 偏移
	const cv::Scalar borderColor(255, 255, 255);
	const int borderThickness = 1;
	const double a0 = hStart - 90.0;   // hue=0 → 顶部
	const double a1 = hEnd - 90.0;     // hue=20 → 顶部偏右
	cv::ellipse(wheel, center, cv::Size(rIn, rIn), 0.0, a0, a1, borderColor, borderThickness);
	cv::ellipse(wheel, center, cv::Size(rOut, rOut), 0.0, a0, a1, borderColor, borderThickness);
	for (double a : { a0, a1 }) {
		double rad = a * CV_PI / 180.0;
		cv::Point2i pIn(cvRound(center.x + rIn * std::cos(rad)), cvRound(center.y + rIn * std::sin(rad)));
		cv::Point2i pOut(cvRound(center.x + rOut * std::cos(rad)), cvRound(center.y + rOut * std::sin(rad)));
		cv::line(wheel, pIn, pOut, borderColor, borderThickness);
	}
}
