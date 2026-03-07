#pragma once

#include "../PixelData/PopularColors.hpp"

class GradientFiller {
public:
	static void fillSolidColor(cv::Mat &mat, cv::Scalar color) {
		if (mat.empty()) return;
		mat.setTo(color);
	}

	/**
	 * @brief 线性渐变
	 * @param mat 输出的 Mat，必须预先设置好尺寸和类型
	 * @param color_start 起始颜色 (BGR)
	 * @param color_end 结束颜色 (BGR)
	 * @param direction 渐变方向 ("horizontal", "vertical", "diagonal")
	 */
	static void fillLinear(cv::Mat &mat, cv::Scalar color_start, cv::Scalar color_end, const std::string &direction = "horizontal") {
		if (mat.empty()) return;

		for (int y = 0; y < mat.rows; ++y) {
			for (int x = 0; x < mat.cols; ++x) {
				float t = 0.0f;
				if (direction == "horizontal") {
					t = static_cast<float>(x) / (mat.cols - 1);
				}
				else if (direction == "vertical") {
					t = static_cast<float>(y) / (mat.rows - 1);
				}
				else if (direction == "diagonal") {
					t = (static_cast<float>(x) + static_cast<float>(y)) / (mat.cols + mat.rows - 2);
					t = std::min(1.0f, t); // 确保 t 不超过 1.0
				}

				mat.at<cv::Vec3b>(y, x) = interpolateColor(color_start, color_end, t);
			}
		}
	}

	/**
	 * @brief 径向渐变
	 * @param mat 输出的 Mat，必须预先设置好尺寸和类型
	 * @param center 渐变中心点坐标
	 * @param radius 渐变半径
	 * @param color_center 中心颜色 (BGR)
	 * @param color_edge 边缘颜色 (BGR)
	 */
	static void fillRadial(cv::Mat &mat, cv::Point center, int radius, cv::Scalar const &color_center, cv::Scalar const &color_edge) {
		if (mat.empty()) return;

		const float max_radius = static_cast<float>(radius);
		for (int y = 0; y < mat.rows; ++y) {
			for (int x = 0; x < mat.cols; ++x) {
				float distance = cv::norm(cv::Point(x, y) - center);
				float t = distance / max_radius;
				t = std::min(1.0f, t); // 超出半径的区域保持边缘颜色

				mat.at<cv::Vec3b>(y, x) = interpolateColor(color_center, color_edge, t);
			}
		}
	}

	/**
	 * @brief 双线性渐变 (从四个角混合颜色)
	 * @param mat 输出的 Mat，必须预先设置好尺寸和类型
	 * @param color_tl 左上角颜色 (BGR)
	 * @param color_tr 右上角颜色 (BGR)
	 * @param color_bl 左下角颜色 (BGR)
	 * @param color_br 右下角颜色 (BGR)
	 */
	static void fillBilinear(cv::Mat &mat, cv::Scalar color_tl, cv::Scalar color_tr, cv::Scalar color_bl, cv::Scalar color_br) {
		if (mat.empty()) return;

		for (int y = 0; y < mat.rows; ++y) {
			for (int x = 0; x < mat.cols; ++x) {
				float t_x = static_cast<float>(x) / (mat.cols - 1);
				float t_y = static_cast<float>(y) / (mat.rows - 1);

				// 先在 x 方向上插值顶部和底部颜色
				cv::Vec3b top_color = interpolateColor(color_tl, color_tr, t_x);
				cv::Vec3b bottom_color = interpolateColor(color_bl, color_br, t_x);

				// 再在 y 方向上插值得到最终颜色
				mat.at<cv::Vec3b>(y, x) = interpolateColor(top_color, bottom_color, t_y);
			}
		}
	}

private:
	// 颜色插值辅助函数
	static cv::Vec3b interpolateColor(const cv::Scalar &c1, const cv::Scalar &c2, float t) {
		cv::Vec3b color;
		for (int i = 0; i < 3; ++i) { // BGR channels
			color[i] = static_cast<uchar>((1.0f - t) * c1[i] + t * c2[i]);
		}
		return color;
	}
};
