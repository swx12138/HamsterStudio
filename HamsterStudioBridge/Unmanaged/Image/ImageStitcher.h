#pragma once

#define ImageStitcherNamespace Image

#include <opencv2/core/mat.hpp>

#include <filesystem>
#include <set>

namespace ImageStitcherNamespace {

	enum ImageStitcheMode {
		None,
		Portrait,
		Landscape,
		All
	};

	class ImageStitcher {
		std::filesystem::path image_folder_path_;
		std::vector<cv::Mat> images_;
		constexpr static const int Long = 1920;
		constexpr static const int Short = 1280;
		bool use_placeholder_ = true;
	public:

		// 定义支持的图片格式
		const static std::set<std::string> supported_extensions;

		explicit ImageStitcher(std::filesystem::path const& image_folder, bool use_placeholder);

		/**
		 * @brief 生成拼接结果并保存
		 * @param target_width 每个格子的目标宽度
		 * @param target_height 每个格子的目标高度
		 * @param output_path 输出文件路径
		 */
		cv::Mat generateStitchedImage(std::filesystem::path const& output_path, ImageStitcheMode const mode);

		/**
		 * @brief 拼接图片的核心算法
		 * @param images 输入的图片向量
		 * @param target_width 每个格子的目标宽度
		 * @param target_height 每个格子的目标高度
		 * @return 拼接后的单张图片
		 */
		static cv::Mat stitch(const std::vector<cv::Mat>& images, int target_width, int target_height, int borderThickness = -1);

		static cv::Mat stitch(std::vector<cv::Mat const*> const& images, int target_width, int target_height, int borderThickness = -1);

		static void PasteImage(cv::Mat& canvas, const cv::Mat& image);
		static cv::Mat PlaceHolderImage();
		static cv::Mat LoadImageCV(const std::filesystem::path& filepath);
	};
}