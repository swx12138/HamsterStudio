
#include "./ImageEditor.h"

#include <opencv2/opencv.hpp>

#include "../Tools/Diagnostics.h"

using namespace PixelDataNamespace;
using namespace PixelDataProcessorNamespace;
using namespace ImageEditorNamespace;

class ImageEditor::ImageData {
	cv::Mat image_;
public:
	explicit ImageData(std::string const &filepath)
	{
		image_ = cv::imread(filepath);
		if (!image_.empty()) {
			image_.convertTo(image_, CV_32FC3);
		}
	}

	virtual ~ImageData() noexcept { }

	cv::Mat& GetImage() noexcept {
		return image_;
	}
};

#pragma endregion

ImageEditor::ImageEditor() noexcept
{ }

ImageEditor::~ImageEditor() noexcept
{ }

bool ImageEditor::LoadImage(std::string_view filename) {
	imageData_ = std::make_shared<ImageData>(std::string(filename));
	return !imageData_->GetImage().empty();
}

bool ImageEditor::SaveImage(std::string_view filename)
{
	if (!imageData_) {
		return false;
	}

	cv::Mat &image = imageData_->GetImage();
	if (image.empty()) {
		return false;
	}

	return cv::imwrite(std::string(filename), image);
}

void ImageEditor::SetPixelDataProcessor(PixelDataProcessor::Pointer processor) noexcept
{
	pixelDataProcessor_ = processor;
}

void ImageEditor::ApplyPixelDataProcessor() noexcept
{
	// 这个函数标了 noexcept，未加载图像或未设置处理器时不能靠解空指针来"失败"
	if (!imageData_ || !pixelDataProcessor_) {
		return;
	}

	// 遍历图像的每个像素，并应用PixelDataProcessor
	cv::Mat &image = imageData_->GetImage();
	Diagnostics::Trace::TraceInfo(std::format("Image type: {}", image.type()));
	for (int y = 0; y < image.rows; ++y) {
		for (int x = 0; x < image.cols; ++x) {
			cv::Vec3f &pixel = image.at<cv::Vec3f>(y, x);
			::PixelData pixelData(pixel[2], pixel[1], pixel[0]); // OpenCV使用BGR顺序
			::PixelData processedPixel = pixelDataProcessor_->Process(pixelData);
			pixel[2] = static_cast<float>(processedPixel.Red);
			pixel[1] = static_cast<float>(processedPixel.Green);
			pixel[0] = static_cast<float>(processedPixel.Blue);
		}
	}
}

int ImageEditorNamespace::image_editor_test_main(std::string const& imagePath, std::string const& imageOutPath)
{
	using namespace std::chrono;
	auto start = high_resolution_clock::now();

	ImageEditor editor;
	editor.LoadImage(imagePath);

	auto pixelDataProcessor = std::make_shared<GlobalPixelDataProcessor>(0.5, 0.5, 0.5);
	editor.SetPixelDataProcessor(pixelDataProcessor);
	editor.ApplyPixelDataProcessor();

	editor.SaveImage(imageOutPath);

	auto end = high_resolution_clock::now();
	auto duration = duration_cast<milliseconds>(end - start);
	std::cout << "Processing time: " << duration.count() << " ms" << std::endl;

	return 0;
}
