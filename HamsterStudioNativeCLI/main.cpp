
#include "../HamsterStudioNative/Image/ImageEditor.h"
#include "../HamsterStudioNative/Image/ImageProcessor.h"

#include <chrono>
#include <iostream>

const std::string imagePath = "E:/HamsterStudioHome/xiaohongshu/艾西Aiwest/长萌~大家新年快乐呀_1_xhs_艾西Aiwest_1040g3k031sqfb1cdlk0g5nt3psrg8e7n8c09eko.png";
const std::string imageOutPath = "D:/.memory/长萌~大家新年快乐呀_1_xhs_艾西Aiwest_1040g3k031sqfb1cdlk0g5nt3psrg8e7n8c09eko.png";

#include <opencv2/opencv.hpp>

int cuda_check_main() {
	std::cout << "OpenCV Version: " << CV_VERSION << std::endl;

	// 打印构建信息，查找 CUDA 相关内容
	std::cout << "OpenCV Build Information:" << std::endl;
	std::cout << cv::getBuildInformation() << std::endl;

	// 检查是否有可用的 CUDA 设备
	int cuda_devices = cv::cuda::getCudaEnabledDeviceCount();
	std::cout << "Number of CUDA devices available: " << cuda_devices << std::endl;

	if (cuda_devices > 0) {
		std::cout << "CUDA support is compiled and working!" << std::endl;
		// 你可以在这里尝试使用 CUDA 功能，例如 cv::cuda::cvtColor
	}
	else {
		std::cout << "No CUDA devices found or CUDA support not compiled correctly." << std::endl;
	}

	return 0;
}

int image_format_convert(std::string const &infile, std::string const &outfile) {
	cv::Mat image = cv::imread(infile);
	if (image.empty()) {
		std::cerr << "Failed to read image: " << infile << std::endl;
		return 1;
	}
	// 保存为 PNG 格式
	if (!cv::imwrite(outfile, image)) {
		std::cerr << "Failed to write image: " << outfile << std::endl;
		return 2;
	}

	return 0;
}

int main0()
{
	return image_format_convert(
		"E:\\HamsterStudioHome\\xiaohongshu\\是阿珞子\\缓慢更新库存一则_1_xhs_是阿珞子_1040g00831sg3li8c5kkg5ntlqomgbnsadk0cdl0.png",
		"E:\\HamsterStudioHome\\xiaohongshu\\是阿珞子\\缓慢更新库存一则_1_xhs_是阿珞子_1040g00831sg3li8c5kkg5ntlqomgbnsadk0cdl0.jpg");
	return cuda_check_main();

	using namespace std::chrono;
	auto start = high_resolution_clock::now();

	ImageEditorNamespace::ImageEditor editor;
	editor.LoadImage(imagePath);

	auto pixelDataProcessor = std::make_shared<PixelDataProcessorNamespace::GlobalPixelDataProcessor>(0.5, 0.5, 0.5);
	editor.SetPixelDataProcessor(pixelDataProcessor);
	editor.ApplyPixelDataProcessor();

	editor.SaveImage(imageOutPath);

	auto end = high_resolution_clock::now();
	auto duration = duration_cast<milliseconds>(end - start);
	std::cout << "Processing time: " << duration.count() << " ms" << std::endl;

	return 0;
}

#include "../HamsterStudioNative/Image/ImageStitcher.hpp"
#include "../HamsterStudioNative/Framework/StopWatch.hpp"

int main_stitch(std::vector<std::string> const &args)
{
	namespace fs = std::filesystem;
	StopWatch watch;
#ifdef _DEBUG
#else
	try {
#endif // _DEBUG
		fs::path image_folder_path = fs::current_path();
		ImageStitcher stitcher { image_folder_path };

		if (args.size() != 2) {
			std::cout << "No mode specified, generating all results in parallel..." << std::endl;
			std::jthread th_portrait([&] { stitcher.generateStitchedImage("result_portrait.jpg", ImageStitcheMode::Portrait); });
			std::jthread th_landscape([&] { stitcher.generateStitchedImage("result_landscape.jpg", ImageStitcheMode::Landscape); });
			th_landscape.join();
			th_portrait.join();
		}
		else if (args[1] == "portrait" || args[1] == "p") {
			std::cout << "Generating portrait stitched image..." << std::endl;
			stitcher.generateStitchedImage("result_portrait.jpg", ImageStitcheMode::Portrait);
		}
		else if (args[1] == "landscape" || args[1] == "l") {
			std::cout << "Generating landscape stitched image..." << std::endl;
			stitcher.generateStitchedImage("result_landscape.jpg", ImageStitcheMode::Landscape);
		}
		else if (args[1] == "all" || args[1] == "a") {
			std::cout << "Generating all stitched images..." << std::endl;
			stitcher.generateStitchedImage("result_all.jpg", ImageStitcheMode::None);
		}
		else {
			std::cerr << "Invalid argument: " << args[1] << std::endl;
			std::cerr << "Usage: " << args[0] << " [portrait|landscape|all]" << std::endl;
			return 1;
		}

#ifdef _DEBUG
#else
	}
	catch (const std::exception &e) {
		std::cerr << "Error: " << e.what() << std::endl;
		system("pause");
	}
#endif // _DEBUG
	std::cout << "Total processing time: " << std::chrono::duration_cast<std::chrono::milliseconds>(watch.elapsed()).count() << " ms" << std::endl;
	return 0;
}

#include "../HamsterStudioNative/Image/Effects/BlackSoftEffect.hpp"

int main(int argc, char **argv) {
	std::vector<std::string> args(argv, argv + argc);
	return main_stitch(args);

	using namespace ImageEffectsNamespace;

	cv::Mat image = cv::imread("C:/Users/nv/Downloads/20260222_00193.jpg");

	// 应用黑柔效果
	// strength: 0.7 会让暗角非常显著
	// vignette_size: 0.5 控制暗角从中心扩散的快慢
	cv::Mat result = applyBlackVelvetEffect(image);

	cv::imwrite("C:/Users/nv/Downloads/20260222_00193_blacksoft.jpg", result);

	return 0;
}
