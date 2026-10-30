#include <hamster/toes/image.h>

#include <opencv2/opencv.hpp>

#ifdef _CUDA_OPENCV
#include <opencv2/cudawarping.hpp>
#endif // _CUDA_OPENCV

#ifdef _DEBUG
#pragma comment(lib, "opencv_world4120d.lib")
#else
#pragma comment(lib, "opencv_world4120.lib")
#endif // _DEBUG

#include "core/diagnostics.h"
using namespace HamsterStudioToes;

#include <fstream>

cv::Mat loadMatFromStream(std::istream& is, int flags = cv::IMREAD_COLOR);

struct ImageNamespace::Image::Impl
{
	cv::Mat mat;
};

// 默认构造
ImageNamespace::Image::Image()
{
	pImpl = std::make_shared<Impl>();
}

// 从文件读取
ImageNamespace::Image::Image(fs::path const& path, int opencvReadFlag)
	:Image()
{
	//pImpl->mat = LoadImageCV(path);
	std::ifstream ifs(path, std::ios::binary);
	pImpl->mat = loadMatFromStream(ifs, opencvReadFlag);
}

// 从流读取
ImageNamespace::Image::Image(std::istream &is, int opencvReadFlag)
	:Image()
{
	pImpl->mat = loadMatFromStream(is, opencvReadFlag);
}

// 复制构造函数
ImageNamespace::Image::Image(Image const &image)
	:Image()
{
	pImpl->mat = image.pImpl->mat.clone();
}

// 移动构造函数
ImageNamespace::Image::Image(Image &&image) noexcept
{
	pImpl = image.pImpl;
	image.pImpl = nullptr;
}

// 复制赋值
ImageNamespace::Image& ImageNamespace::Image::operator=(Image const &image)
{
	if (this != &image) {
		if (!pImpl) {
			pImpl = std::make_shared<Impl>();
		}
		pImpl->mat = image.pImpl->mat.clone();
	}
	return *this;
}

// 移动赋值
ImageNamespace::Image& ImageNamespace::Image::operator=(Image &&image) noexcept
{
	if (this != &image) {  // 防止自赋值
		pImpl = std::move(image.pImpl);  // 或者 pImpl.swap(image.pImpl)
		// image.pImpl 现在为空，其原有资源已被转移
	}
	return *this;
}

ImageNamespace::Image ImageNamespace::Image::operator()(Shapes::Rectangle const& rect) const
{
	cv::Rect rc(rect.left, rect.top, rect.width, rect.height);
	Image roi;
	roi.pImpl->mat = pImpl->mat(rc);
	return roi;
}

bool ImageNamespace::Image::Valid() const
{
	if (pImpl == nullptr) {
		return false;
	}
	return !pImpl->mat.empty();
}

void* ImageNamespace::Image::NativeMatPointer() const
{
	return &pImpl->mat;
}

ImageNamespace::Image ImageNamespace::Image::FromNativeMatPointer(void* pointer)
{
	Image img;
	img.pImpl->mat = *reinterpret_cast<cv::Mat*>(pointer);
	return img;
}

ImageNamespace::Image ImageNamespace::Image::FromByteArray(std::vector<uint8_t> const& data, int flags)
{
	Image img;
	img.pImpl->mat = cv::imdecode(data, flags);
	return img;
}

ImageNamespace::Image ImageNamespace::Image::FromByteArray(uint8_t* data, size_t dataLen, int flags)
{
	return FromByteArray(std::vector<uint8_t>(data, data + dataLen), flags);
}

int ImageNamespace::Image::Width() const
{
	return pImpl->mat.cols;
}

int ImageNamespace::Image::Height() const
{
	return pImpl->mat.rows;
}

ImageNamespace::ImageShape ImageNamespace::Image::GetShape() const
{
	int width = pImpl->mat.cols;
	int height = pImpl->mat.rows;
	int channels = pImpl->mat.channels();
	return { width, height, channels };
}

void ImageNamespace::Image::ImShow(int width) const
{
	cv::Mat resized;
	cv::resize(pImpl->mat, resized, cv::Size(width, width * pImpl->mat.rows / pImpl->mat.cols));
	cv::imshow("Image", resized);
	cv::waitKey();
}

void ImageNamespace::Image::Save(fs::path const& filename, int quality) const
{
	cv::imwrite(filename.generic_string(), pImpl->mat, { cv::IMWRITE_JPEG_QUALITY,quality });
}

void ImageNamespace::Image::AdaptiveResize(int shortSide) const
{
	if (Width() <= shortSide || Height() <= shortSide) {
		return;
	}

	cv::Size2d size;
	if (Width() > Height()) {
		size.height = shortSide;
		size.width = size.height * Width() / Height();
	}
	else {
		size.width = shortSide;
		size.height = size.width * Height() / Width();
	}

#ifdef _CUDA_OPENCV
	cv::cuda::GpuMat gpu_src, gpu_dst;
	gpu_src.upload(pImpl->mat);
	cv::cuda::resize(gpu_src, gpu_dst, size);
	gpu_dst.download(pImpl->mat);
#else
	cv::resize(pImpl->mat, pImpl->mat, size);
#endif // _CUDA_OPENCV
}

void ImageNamespace::Image::CheckCuda()
{
	std::ostringstream oss;
	oss << "OpenCV Version: " << CV_VERSION << std::endl;

	// 打印构建信息，查找 CUDA 相关内容
	oss << "OpenCV Build Information:" << std::endl;
	oss << cv::getBuildInformation() << std::endl;

	// 检查是否有可用的 CUDA 设备
	int cuda_devices = cv::cuda::getCudaEnabledDeviceCount();
	oss << "Number of CUDA devices available: " << cuda_devices << std::endl;

	if (cuda_devices > 0) {
		oss << "CUDA support is compiled and working!" << std::endl;
		// 你可以在这里尝试使用 CUDA 功能，例如 cv::cuda::cvtColor
	}
	else {
		oss << "No CUDA devices found or CUDA support not compiled correctly." << std::endl;
	}
	Diagnostics::Trace::TraceInfo(oss.str());
}

/**
 * 从 std::istream 加载图像到 cv::Mat
 * @param is 输入流（如 std::ifstream, std::stringstream, 网络流等）
 * @param flags 读取模式，同 imread (默认 IMREAD_COLOR)
 * @return cv::Mat 解码后的图像，失败返回空矩阵
 */
cv::Mat loadMatFromStream(std::istream &is, int flags)
{
	// 1. 读取整个流到内存缓冲区
	std::vector<unsigned char> buffer {
		std::istreambuf_iterator<char>(is),
		std::istreambuf_iterator<char>()
	};

	// 2. 如果流为空，返回空 Mat
	if (buffer.empty()) {
		return cv::Mat();
	}

	// 3. 使用 imdecode 解码
	return cv::imdecode(buffer, flags);
}
