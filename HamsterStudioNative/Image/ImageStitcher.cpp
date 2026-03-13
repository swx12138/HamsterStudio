#include "ImageStitcher.h"

#include "./Tools/GradientFiller.hpp"

#include <Windows.h>

// --- Windows API Header ---
#include <shlwapi.h> // 包含 StrCmpLogicalW
#pragma comment(lib, "shlwapi.lib") // 链接 shlwapi.lib 库

// 定义支持的图片格式
const std::set<std::string> ImageStitcher::supported_extensions = {
	".jpg", ".jpeg", ".png", ".bmp", ".tiff", ".tif", ".webp"
};

ImageStitcher::ImageStitcher(std::filesystem::path const &image_folder, bool use_placeholder)
	: image_folder_path_(image_folder),
	use_placeholder_(use_placeholder)
{
	// 将视图内容收集到 vector 中以便排序和后续处理
	std::vector<std::filesystem::path> all_filenames;
	for (auto const &entry : std::filesystem::directory_iterator(image_folder_path_)) {
		if (!entry.is_regular_file()) { continue; }

		auto path = entry.path();
		if (path.filename().wstring().starts_with(L"result_")) { continue; }

		if (supported_extensions.count(path.extension().string())) {
			all_filenames.push_back(path);
		}
	}

	std::sort(all_filenames.begin(), all_filenames.end(),
		[] (const std::filesystem::path &a, const std::filesystem::path &b) {
			return StrCmpLogicalW(a.filename().c_str(), b.filename().c_str()) < 0;
		});

	std::cout << "找到 " << all_filenames.size() << " 张图片，正在加载..." << std::endl;

	if (use_placeholder_) {
		// 第一张图片默认纯色
		auto default_landspce_image = PlaceHolderImage();
		images_.push_back(default_landspce_image);
	}

	// --- 4. 加载图片并按横竖方向分类 ---
	for (const auto &filepath : all_filenames) {
		auto img = LoadImageCV(filepath);
		if (img.empty()) {
			std::cerr << "警告: 无法加载图片: " << filepath << std::endl;
			continue;
		}
		images_.push_back(img);
	}

	std::cout << "加载完成。共计: " << images_.size() - use_placeholder_ << " 张。" << std::endl;
}

cv::Mat ImageStitcher::generateStitchedImage(std::filesystem::path const &output_path, ImageStitcheMode const mode)
{
	std::vector<cv::Mat> all_images_;
	if (use_placeholder_) {
		if (mode == ImageStitcheMode::Portrait) {
			all_images_.push_back(images_[0](cv::Rect(0, 0, Short, Long)));
			std::ranges::copy_if(images_ | std::views::drop(1),
				std::back_inserter(all_images_),
				[] (const cv::Mat &img) { return img.rows > img.cols; });
		}
		else if (mode == ImageStitcheMode::Landscape) {
			all_images_.push_back(images_[0](cv::Rect(0, 0, Long, Short)));
			std::ranges::copy_if(images_ | std::views::drop(1),
				std::back_inserter(all_images_),
				[] (const cv::Mat &img) { return img.cols >= img.rows; });
		}
		else {
			all_images_ = images_;
		}
	}
	else {
		if (mode == ImageStitcheMode::Portrait) {
			std::ranges::copy_if(images_,
				std::back_inserter(all_images_),
				[] (const cv::Mat &img) { return img.rows > img.cols; });
		}
		else if (mode == ImageStitcheMode::Landscape) {
			std::ranges::copy_if(images_,
				std::back_inserter(all_images_),
				[] (const cv::Mat &img) { return img.cols >= img.rows; });
		}
		else {
			all_images_ = images_;
		}
	}

	if (all_images_.size() <= 1) {
		std::cerr << "警告: 没有足够的图片进行拼接，至少需要一张图片。" << std::endl;
		return cv::Mat();
	}

	cv::Mat stitched_image = stitch(all_images_,
		mode != ImageStitcheMode::Portrait ? Long : Short,
		mode != ImageStitcheMode::Landscape ? Long : Short);
	if (!stitched_image.empty()) {
		cv::imwrite(output_path.string(), stitched_image);
		std::cout << "拼接完成，结果已保存到: " << output_path << std::endl;
	}
	else {
		std::cerr << "拼接失败，未生成有效的图片。" << std::endl;
	}
	return stitched_image;
}

#undef min

cv::Mat ImageStitcher::stitch(const std::vector<cv::Mat> &images, int target_width, int target_height, int borderThickness)
{
	if (images.empty()) {
		std::cerr << "错误: 输入图片列表为空，无法进行拼接。" << std::endl;
		return cv::Mat();
	}

	if (borderThickness < 0) {
		borderThickness = std::min(target_width, target_height) / 20; // 默认边距为格子尺寸的 5%
	}

	std::cout << "开始拼接 " << images.size() << " 张图片，每个格子尺寸: " << target_width << "x" << target_height
		<< "，边距: " << borderThickness << std::endl;

	const int N = static_cast<int>(images.size());

	// 1. 计算最优行列数 (m, n)，使它们最接近
	int rows = static_cast<int>(std::ceil(std::sqrt(static_cast<double>(N))));
	int cols = static_cast<int>(std::ceil(static_cast<double>(N) / rows));
	// 确保 rows <= cols，优先形成横向布局（例如 2x3 而非 3x2）
	if (rows > cols) {
		std::swap(rows, cols);
	}

	std::cout << "计算得到的布局: " << rows << " 行 x " << cols << " 列。" << std::endl;

	// 2. 创建拼接画布
	const int canvas_width = cols * target_width + (cols + 1) * borderThickness;
	const int canvas_height = rows * target_height + (rows + 1) * borderThickness;
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
		cv::Mat roi = canvas(cv::Rect(
			borderThickness + col_idx * (target_width + borderThickness),
			borderThickness + row_idx * (target_height + borderThickness),
			target_width,
			target_height
		));
		PasteImage(roi, images[i]);
	}

	return canvas;
}

void ImageStitcher::PasteImage(cv::Mat &canvas, const cv::Mat &image)
{
	double scale_x = static_cast<double>(canvas.cols) / image.cols;
	double scale_y = static_cast<double>(canvas.rows) / image.rows;
	double scale = std::min(scale_x, scale_y);

	cv::Mat scaled_image;
	cv::resize(image, scaled_image, cv::Size {}, scale, scale);

	int x_offset = (canvas.cols - scaled_image.cols) / 2;
	int y_offset = (canvas.rows - scaled_image.rows) / 2;
	auto roi = canvas(cv::Rect(x_offset, y_offset, scaled_image.cols, scaled_image.rows));
	if (scaled_image.channels() == 1 && canvas.channels() == 3) {
		cv::cvtColor(scaled_image, roi, cv::COLOR_GRAY2BGR);
	}
	else {
		scaled_image.copyTo(roi);
	}
}

cv::Mat ImageStitcher::PlaceHolderImage() {
	static cv::Mat placeholder = cv::Mat(Long, Long, CV_8UC3);
	GradientFiller::fillSolidColor(placeholder, PantoneColors::YearColor_2026_CloudDancer);
	//auto size = cv::Size(Long / 4, Short / 4);
	//size.width = size.height = std::min(size.width, Long / 2 - 20);
	//cv::ellipse(placeholder, cv::Point(Long / 2, Short / 2), size, 0, 0, 360, PantoneColors::YearColor_2020_ClassicBlue, -1);
	return placeholder;
}

cv::Mat ImageStitcher::LoadImageCV(const std::filesystem::path &filepath) {
	try {
		cv::Mat img = cv::imread(filepath.string());
		return img;
	}
	catch (const std::exception &e) {
		std::wcerr << L"错误: 无法加载图片 " << filepath
			<< L"，异常信息: " << e.what()
			<< L"，准备重试。。。"
			<< std::endl;
	}
	try {
		auto temp_filename = filepath.parent_path()
			.append("temp_image" + std::filesystem::path(filepath).extension().string());
		std::filesystem::copy_file(filepath, temp_filename, std::filesystem::copy_options::overwrite_existing);
		cv::Mat img = cv::imread(temp_filename.string());

		std::wcerr << L"重试成功。" << std::endl;
		std::filesystem::remove(temp_filename);
		return img;
	}
	catch (const std::exception &e) {
		std::wcerr << L"错误: 无法加载图片 " << filepath
			<< L"，异常信息: " << e.what()
			<< std::endl;
	}
	return cv::Mat();
}
