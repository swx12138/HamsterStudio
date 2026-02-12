#include "./ImageEditor.h"

#pragma region PixelData Implementation

ImageEditorNamespace::PixelData::PixelData() noexcept
	: Red(1), Green(1), Blue(1)
{ }

ImageEditorNamespace::PixelData::PixelData(DataTye r, DataTye g, DataTye b) noexcept
	: Red(r), Green(g), Blue(b)
{ }

ImageEditorNamespace::PixelData::~PixelData() noexcept
{ }

#pragma endregion

#pragma region ImageEditor Implementation

#include <opencv2/opencv.hpp>

#ifdef _DEBUG
#pragma comment(lib, "opencv_world4100d.lib")
#else
#pragma comment(lib, "opencv_world4100.lib")
#endif // _DEBUG

class ImageEditorNamespace::ImageEditor::ImageData {
	cv::Mat image_;
public:
	explicit ImageData(std::string const &filepath) noexcept
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

ImageEditorNamespace::ImageEditor::ImageEditor() noexcept
{ }

ImageEditorNamespace::ImageEditor::~ImageEditor() noexcept
{ }

bool ImageEditorNamespace::ImageEditor::LoadImage(std::string_view filename) {
	imageData_ = std::make_shared<ImageData>(std::string(filename));
	return !imageData_->GetImage().empty();
}

bool ImageEditorNamespace::ImageEditor::SaveImage(std::string_view filename)
{
	return cv::imwrite(std::string(filename), imageData_->GetImage());
}

void ImageEditorNamespace::ImageEditor::SetPixelDataProcessor(PixelDataProcessor::Pointer processor) noexcept
{
	pixelDataProcessor_ = processor;
}

void ImageEditorNamespace::ImageEditor::ApplyPixelDataProcessor() noexcept
{
	// 遍历图像的每个像素，并应用PixelDataProcessor
	cv::Mat &image = imageData_->GetImage();
	std::cout << "Image type: " << image.type() << std::endl;
	for (int y = 0; y < image.rows; ++y) {
		for (int x = 0; x < image.cols; ++x) {
			cv::Vec3f &pixel = image.at<cv::Vec3f>(y, x);
			PixelData pixelData(pixel[2], pixel[1], pixel[0]); // OpenCV使用BGR顺序
			PixelData processedPixel = pixelDataProcessor_->Process(pixelData);
			pixel[2] = static_cast<float>(processedPixel.Red);
			pixel[1] = static_cast<float>(processedPixel.Green);
			pixel[0] = static_cast<float>(processedPixel.Blue);
		}
	}
}

ImageEditorNamespace::PixelDataProcessor::~PixelDataProcessor() noexcept
{ }

ImageEditorNamespace::PixelData ImageEditorNamespace::PixelDataProcessor::Process(ImageEditorNamespace::PixelData &pixel) const
{
	if (nextProcessor_) {
		return nextProcessor_->Process(pixel);
	}
	else {
		return pixel;
	}
}

ImageEditorNamespace::DirectPixelDataProcessor::DirectPixelDataProcessor(double r, double g, double b) noexcept
	: r_(r), g_(g), b_(b)
{ }

ImageEditorNamespace::DirectPixelDataProcessor::~DirectPixelDataProcessor() noexcept
{ }

ImageEditorNamespace::PixelData ImageEditorNamespace::DirectPixelDataProcessor::Process(ImageEditorNamespace::PixelData &pixel) const
{
	pixel.Red *= r_;
	pixel.Green *= g_;
	pixel.Blue *= b_;
	return PixelDataProcessor::Process(pixel);
}
