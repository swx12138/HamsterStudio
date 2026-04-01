#pragma unmanaged

#include "./GradientFiller.h"

#ifdef _DEBUG
#pragma comment(lib, "opencv_world4100d.lib")
#else
#pragma comment(lib, "opencv_world4100.lib")
#endif // _DEBUG

void GradientFillerNamespace::FillSolidColor(cv::Mat& mat, cv::Scalar color)
{
	if (mat.empty()) return;
	mat.setTo(color);
}

// 颜色插值辅助函数
static cv::Vec3b interpolateColor(const cv::Scalar& c1, const cv::Scalar& c2, double t) {
	cv::Vec3b color;
	for (int i = 0; i < 3; ++i) { // BGR channels
		color[i] = static_cast<uchar>((1.0f - t) * c1[i] + t * c2[i]);
	}
	return color;
}

void GradientFillerNamespace::FillLinear(cv::Mat& mat, cv::Scalar color_start, cv::Scalar color_end, const std::string& direction)
{
	if (mat.empty()) return;

	for (int y = 0; y < mat.rows; ++y) {
		for (int x = 0; x < mat.cols; ++x) {
			double t = 0.0f;
			if (direction == "horizontal") {
				t = static_cast<double>(x) / (mat.cols - 1);
			}
			else if (direction == "vertical") {
				t = static_cast<double>(y) / (mat.rows - 1);
			}
			else if (direction == "diagonal") {
				t = (static_cast<double>(x) + static_cast<double>(y)) / (mat.cols + mat.rows - 2);
				t = std::min(1.0, t); // 确保 t 不超过 1.0
			}

			mat.at<cv::Vec3b>(y, x) = interpolateColor(color_start, color_end, t);
		}
	}
}

void GradientFillerNamespace::FillRadial(cv::Mat& mat, cv::Point center, int radius, cv::Scalar const& color_center, cv::Scalar const& color_edge)
{
	if (mat.empty()) return;

	const double max_radius = static_cast<double>(radius);
	for (int y = 0; y < mat.rows; ++y) {
		for (int x = 0; x < mat.cols; ++x) {
			auto distance = cv::norm(cv::Point(x, y) - center);
			auto t = distance / max_radius;
			t = std::min(1.0, t); // 超出半径的区域保持边缘颜色
			mat.at<cv::Vec3b>(y, x) = interpolateColor(color_center, color_edge, t);
		}
	}
}

void FillBilinearAVX2(cv::Mat& mat, cv::Scalar color_tl, cv::Scalar color_tr, cv::Scalar color_bl, cv::Scalar color_br);

void GradientFillerNamespace::FillBilinear(cv::Mat& mat, cv::Scalar color_tl, cv::Scalar color_tr, cv::Scalar color_bl, cv::Scalar color_br)
{
	if (mat.empty()) return;

	if (!mat.isContinuous()) {
		mat = mat.clone();
	}

	FillBilinearAVX2(mat, color_tl, color_tr, color_bl, color_br);
	return;

	for (int y = 0; y < mat.rows; ++y) {
		for (int x = 0; x < mat.cols; ++x) {
			double t_x = static_cast<double>(x) / (mat.cols - 1);
			double t_y = static_cast<double>(y) / (mat.rows - 1);

			// 先在 x 方向上插值顶部和底部颜色
			cv::Vec3b top_color = interpolateColor(color_tl, color_tr, t_x);
			cv::Vec3b bottom_color = interpolateColor(color_bl, color_br, t_x);

			// 再在 y 方向上插值得到最终颜色
			mat.at<cv::Vec3b>(y, x) = interpolateColor(top_color, bottom_color, t_y);
		}
	}
}

#include <immintrin.h>

void FillBilinearAVX2(cv::Mat& mat, cv::Scalar color_tl, cv::Scalar color_tr, cv::Scalar color_bl, cv::Scalar color_br)
{
	const int width = mat.cols;
	const int height = mat.rows;

	// 预计算颜色值（浮点数组）
	alignas(32) float colors[4][3] = {
		{(float)color_tl[0], (float)color_tl[1], (float)color_tl[2]},
		{(float)color_tr[0], (float)color_tr[1], (float)color_tr[2]},
		{(float)color_bl[0], (float)color_bl[1], (float)color_bl[2]},
		{(float)color_br[0], (float)color_br[1], (float)color_br[2]}
	};

	const float inv_width = 1.0f / (width - 1);
	const float inv_height = 1.0f / (height - 1);

	uchar* data = mat.data;
	const auto step = mat.step;

#pragma omp parallel for
	for (int y = 0; y < height; ++y) {
		const float t_y = y * inv_height;
		const float inv_t_y = 1.0f - t_y;

		// 垂直权重（广播到向量）
		const __m256 vy_bottom = _mm256_set1_ps(t_y);
		const __m256 vy_top = _mm256_set1_ps(inv_t_y);

		//uchar* row_ptr = data + y * step;
		uchar* row_ptr = mat.ptr<uchar>(y);

		// 一次处理8个像素（AVX2可以同时处理8个float）
		int x = 0;
		for (; x <= width - 8; x += 8) {
			// 创建t_x向量 [x, x+1, ..., x+7]
			__m256 tx_base = _mm256_set_ps((x + 7) * inv_width, (x + 6) * inv_width,
				(x + 5) * inv_width, (x + 4) * inv_width,
				(x + 3) * inv_width, (x + 2) * inv_width,
				(x + 1) * inv_width, x * inv_width);
			__m256 inv_tx = _mm256_sub_ps(_mm256_set1_ps(1.0f), tx_base);

			// 处理BGR三个通道
			for (int channel = 0; channel < 3; ++channel) {
				// 加载四个角的颜色值（广播到向量）
				__m256 c_tl = _mm256_set1_ps(colors[0][channel]);
				__m256 c_tr = _mm256_set1_ps(colors[1][channel]);
				__m256 c_bl = _mm256_set1_ps(colors[2][channel]);
				__m256 c_br = _mm256_set1_ps(colors[3][channel]);

				// 水平插值
				__m256 top_h = _mm256_add_ps(_mm256_mul_ps(inv_tx, c_tl),
					_mm256_mul_ps(tx_base, c_tr));
				__m256 bottom_h = _mm256_add_ps(_mm256_mul_ps(inv_tx, c_bl),
					_mm256_mul_ps(tx_base, c_br));

				// 垂直插值
				__m256 final_v = _mm256_add_ps(_mm256_mul_ps(vy_top, top_h),
					_mm256_mul_ps(vy_bottom, bottom_h));

				// 将float转换为uchar（带饱和处理）
				__m256i int_vals = _mm256_cvtps_epi32(final_v);
				__m256i clamped = _mm256_min_epi32(_mm256_max_epi32(int_vals, _mm256_setzero_si256()),
					_mm256_set1_epi32(255));

				// 打包为16位整数
				__m128i low = _mm256_castsi256_si128(clamped);
				__m128i high = _mm256_extractf128_si256(clamped, 1);
				__m128i pack16 = _mm_packs_epi32(low, high);
				__m128i pack8 = _mm_packus_epi16(pack16, pack16);

				// 存储结果
				alignas(16) uchar results[8];
				_mm_storel_epi64((__m128i*)results, pack8);

				// 写入到图像内存
				for (int i = 0; i < 8; ++i) {
					row_ptr[(x + i) * 3 + channel] = results[i];
				}
			}
		}

		// 处理剩余像素
		for (; x < width; ++x) {
			const float t_x = x * inv_width;
			const float inv_t_x = 1.0f - t_x;

			for (int c = 0; c < 3; ++c) {
				float top = inv_t_x * colors[0][c] + t_x * colors[1][c];
				float bottom = inv_t_x * colors[2][c] + t_x * colors[3][c];
				float final_val = inv_t_y * top + t_y * bottom;
				row_ptr[x * 3 + c] = (uchar)std::min(255.0f, std::max(0.0f, final_val));
			}
		}
	}
}
