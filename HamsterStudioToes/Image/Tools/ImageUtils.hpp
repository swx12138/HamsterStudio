
#include <opencv2/opencv.hpp>
#include <filesystem>
#include <fstream>

namespace toes
{
    static constexpr inline bool IsSupportedExtension(const std::filesystem::path& file_path)
    {
        // 定义支持的图片格式
        constexpr std::string_view supported_extensions[] = { ".jpg", ".jpeg", ".png", ".bmp", ".tiff", ".tif", ".webp" };
        auto lbd = [=](std::string_view ext) { return file_path.extension() == ext; };
        return std::any_of(std::begin(supported_extensions), std::end(supported_extensions), lbd);
    }

    // 使用 ifstream + imdecode 替代 cv::imread，以正确处理 Windows 上的 Unicode 路径
    static inline cv::Mat Imread(std::filesystem::path const& file_path, int flags = cv::IMREAD_COLOR)
    {
        // Diagnostics::Trace::TraceInfo(L"正在加载图片: " + file_path.generic_wstring());
        std::ifstream file(file_path, std::ios::binary | std::ios::ate);
        if (!file.is_open())
            return cv::Mat();
        auto size = file.tellg();
        file.seekg(0, std::ios::beg);
        std::vector<uint8_t> buffer(static_cast<size_t>(size));
        if (!file.read(reinterpret_cast<char*>(buffer.data()), size))
            return cv::Mat();
        return cv::imdecode(buffer, flags);
    }

    // 使用 imencode + ofstream 替代 cv::imwrite，以正确处理 Windows 上的 Unicode 路径
    static inline bool Imwrite(std::filesystem::path const& file_path, cv::Mat const& img, std::vector<int> const& params = {})
    {
        // Diagnostics::Trace::TraceInfo(L"正在保存图片: " + file_path.generic_wstring());
        std::vector<uint8_t> buffer;
        auto const ext = file_path.extension().string();
        if (!cv::imencode(ext, img, buffer, params))
            return false;
        std::ofstream file(file_path, std::ios::binary);
        if (!file.is_open())
            return false;
        return static_cast<bool>(file.write(reinterpret_cast<char const*>(buffer.data()), buffer.size()));
    }

}