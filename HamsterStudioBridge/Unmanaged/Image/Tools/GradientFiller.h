#pragma once

#include <opencv2/core/mat.hpp>

#define GradientFillerNamespace Image::Tools::GradientFiller

namespace GradientFillerNamespace {
	void FillSolidColor(cv::Mat& mat, cv::Scalar color);

	/**
	 * @brief 线性渐变
	 * @param mat 输出的 Mat，必须预先设置好尺寸和类型
	 * @param color_start 起始颜色 (BGR)
	 * @param color_end 结束颜色 (BGR)
	 * @param direction 渐变方向 ("horizontal", "vertical", "diagonal")
	 */
	void FillLinear(cv::Mat& mat, cv::Scalar color_start, cv::Scalar color_end, const std::string& direction = "horizontal");

	/**
	 * @brief 径向渐变
	 * @param mat 输出的 Mat，必须预先设置好尺寸和类型
	 * @param center 渐变中心点坐标
	 * @param radius 渐变半径
	 * @param color_center 中心颜色 (BGR)
	 * @param color_edge 边缘颜色 (BGR)
	 */
	void FillRadial(cv::Mat& mat, cv::Point center, int radius, cv::Scalar const& color_center, cv::Scalar const& color_edge);

	/**
	 * @brief 双线性渐变 (从四个角混合颜色)
	 * @param mat 输出的 Mat，必须预先设置好尺寸和类型
	 * @param color_tl 左上角颜色 (BGR)
	 * @param color_tr 右上角颜色 (BGR)
	 * @param color_bl 左下角颜色 (BGR)
	 * @param color_br 右下角颜色 (BGR)
	 */
	void FillBilinear(cv::Mat& mat, cv::Scalar color_tl, cv::Scalar color_tr, cv::Scalar color_bl, cv::Scalar color_br);
};
