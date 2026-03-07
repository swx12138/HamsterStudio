#pragma once

#include <opencv2/opencv.hpp>
#include "./Tools/GradientFiller.hpp"

#include <ranges>
#include <filesystem>

class ImageStitcher {
	std::filesystem::path image_folder_path_;
	std::vector<cv::Mat> landscape_images_; // 横图
	std::vector<cv::Mat> portrait_images_;  // 竖图
	constexpr static const int Long = 1920;
	constexpr static const int Short = 1280;
public:

	// 定义支持的图片格式
	const static std::set<std::string> supported_extensions;

	explicit ImageStitcher(std::filesystem::path const &image_folder) : image_folder_path_(image_folder) {
		// 使用 ranges 从路径读取并过滤图片文件
		auto image_files_view = std::filesystem::directory_iterator(image_folder_path_)
			| std::views::filter([] (const std::filesystem::directory_entry &entry) { return entry.is_regular_file()
				&& !entry.path().filename().string().starts_with("result_")
				&& supported_extensions.count(std::filesystem::path(entry).extension().string()); })
			| std::views::transform([] (const std::filesystem::directory_entry &entry) -> std::string { return entry.path().string(); /*转换为完整路径字符串*/ });

		// 将视图内容收集到 vector 中以便排序和后续处理
		std::vector<std::string> all_filenames;
		for (const auto &filename : image_files_view) {
			all_filenames.push_back(filename);
		}
		std::cout << "找到 " << all_filenames.size() << " 张图片，正在加载和分类..." << std::endl;

		// --- 4. 加载图片并按横竖方向分类 ---
		for (const auto &filepath : all_filenames) {
			cv::Mat img = cv::imread(filepath);
			if (img.empty()) {
				std::cerr << "警告: 无法加载图片: " << filepath << std::endl;
				continue;
			}

			if (img.cols > img.rows) {
				landscape_images_.push_back(normalizeImage(img, Long, Short));
			}
			else {
				portrait_images_.push_back(normalizeImage(img, Short, Long));
			}
		}

		std::cout << "加载完成。横图: " << landscape_images_.size() << " 张, 竖图: " << portrait_images_.size() << " 张。" << std::endl;
	}

	/**
	 * @brief 生成拼接结果并保存
	 * @param target_width 每个格子的目标宽度
	 * @param target_height 每个格子的目标高度
	 * @param output_path 输出文件路径
	 */
	cv::Mat generateStitchedImage(std::string const &output_name, bool landscape) {
		std::vector<cv::Mat> all_images = landscape ? landscape_images_ : portrait_images_;
		cv::Mat stitched_image = stitch(all_images, landscape ? Long : Short, landscape ? Short : Long);
		if (!stitched_image.empty()) {
			auto output_path = (image_folder_path_ / output_name);
			cv::imwrite(output_path.string(), stitched_image);
			std::cout << "拼接完成，结果已保存到: " << output_path << std::endl;
		}
		else {
			std::cerr << "拼接失败，未生成有效的图片。" << std::endl;
		}
		return stitched_image;
	}

	/**
	 * @brief 拼接图片的核心算法
	 * @param images 输入的图片向量
	 * @param target_width 每个格子的目标宽度
	 * @param target_height 每个格子的目标高度
	 * @return 拼接后的单张图片
	 */
	static cv::Mat stitch(const std::vector<cv::Mat> &images, int target_width, int target_height) {
		if (images.empty()) {
			std::cerr << "错误: 输入图片列表为空，无法进行拼接。" << std::endl;
			return cv::Mat();
		}

		const int N = static_cast<int>(images.size());

		// 1. 计算最优行列数 (m, n)，使它们最接近
		int rows = static_cast<int>(std::ceil(std::sqrt(static_cast<double>(N))));
		int cols = static_cast<int>(std::ceil(static_cast<double>(N) / rows));
		// 确保 rows <= cols，优先形成横向布局（例如 2x3 而非 3x2）
		if (rows > cols) {
			std::swap(rows, cols);
		}

		// 2. 创建拼接画布
		const int canvas_width = cols * target_width;
		const int canvas_height = rows * target_height;
		cv::Mat canvas = cv::Mat::zeros(canvas_height, canvas_width, CV_8UC3);
		GradientFiller::fillBilinear(canvas,
			PantoneColors::YearColor_2025_MochaMousse,
			PantoneColors::YearColor_2024_PeachFuzz,
			PantoneColors::YearColor_2023_VivaMagenta,
			PantoneColors::YearColor_2022_VeryPeri
		);

		// 3. 循环填充图片
		for (int i = 0; i < static_cast<int>(images.size()); ++i) {
			int row_idx = i / cols;
			int col_idx = i % cols;

			if (row_idx >= rows) {
				std::cout << "警告: 图片数量超过了计算的行列数，剩余图片将被忽略。" << std::endl;
				break; // 防止越界
			}


			// 定义画布上的感兴趣区域 (ROI)
			cv::Mat roi = canvas(cv::Rect(col_idx * target_width, row_idx * target_height, images[i].cols, images[i].rows));

			// 将输入图片复制到 ROI
			// 注意：如果原图是灰度图而画布是彩色图，需要转换
			if (images[i].channels() == 1 && canvas.channels() == 3) {
				cv::cvtColor(images[i], roi, cv::COLOR_GRAY2BGR);
			}
			else {
				//cv::imshow("signle", images[i]);
				//cv::waitKey();

				images[i].copyTo(roi);

				//cv::imshow("canvas", canvas);
				//cv::waitKey();
			}
		}

		return canvas;
	}

	static cv::Mat normalizeImage(const cv::Mat &image, int target_width, int target_height) {
		constexpr auto resized_image_window_name = "resized_image";
		//static std::once_flag init_flag;
		//std::call_once(init_flag, [] () {
		//	cv::namedWindow(resized_image_window_name);
		//	cv::resizeWindow(resized_image_window_name, Long, Long); });

		cv::Mat resized_image;
		auto scale_x = static_cast<double>(target_width) / image.cols;
		auto scale_y = static_cast<double>(target_height) / image.rows;
		auto scale = std::min(scale_x, scale_y);
		cv::resize(image, resized_image, cv::Size {}, scale, scale);

		//cv::imshow(resized_image_window_name, resized_image);
		//cv::waitKey();

		return resized_image;
	}

};

// 定义支持的图片格式
const std::set<std::string> ImageStitcher::supported_extensions = {
	".jpg", ".jpeg", ".png", ".bmp", ".tiff", ".tif", ".webp"
};