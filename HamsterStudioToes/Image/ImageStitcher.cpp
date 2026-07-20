#pragma unmanaged

#include "ImageStitcher.h"

#include <opencv2/opencv.hpp>
#ifdef _CUDA_OPENCV
#include <opencv2/cudawarping.hpp>
#include <opencv2/cudaarithm.hpp>
#endif // _CUDA_OPENCV

#include "./PixelData/PopularColors.hpp"
#include "./Tools/GradientFiller.h"
#include "./Tools/LayoutHelper.h"

#include "../Tools/StringUtils.h"
#include "../Tools/Diagnostics.h"
#include "../Tools/StopWatch.hpp"
#include "../Windows/Win32Utils.h"

#include <ranges>
#include <iostream>
#include <fstream>
#include <io.h>    // _setmode
#include <fcntl.h> // _O_U16TEXT

using namespace ImageNamespace;
using namespace ImageUtilsNamespace;
using namespace HamsterStudioToes;

static constexpr inline bool IsSupportedExtension(const std::filesystem::path &file_path)
{
    // 定义支持的图片格式
    constexpr std::string_view supported_extensions[] = {".jpg", ".jpeg", ".png", ".bmp", ".tiff", ".tif", ".webp"};
    return std::any_of(std::begin(supported_extensions), std::end(supported_extensions), [=](std::string_view ext)
                       { return file_path.extension() == ext; });
}

// 使用 ifstream + imdecode 替代 cv::imread，以正确处理 Windows 上的 Unicode 路径
static cv::Mat Imread(std::filesystem::path const &file_path, int flags = cv::IMREAD_COLOR)
{
    // Diagnostics::Trace::TraceInfo(L"正在加载图片: " + file_path.generic_wstring());
    std::ifstream file(file_path, std::ios::binary | std::ios::ate);
    if (!file.is_open())
        return cv::Mat();
    auto size = file.tellg();
    file.seekg(0, std::ios::beg);
    std::vector<uint8_t> buffer(static_cast<size_t>(size));
    if (!file.read(reinterpret_cast<char *>(buffer.data()), size))
        return cv::Mat();
    return cv::imdecode(buffer, flags);
}

// 使用 imencode + ofstream 替代 cv::imwrite，以正确处理 Windows 上的 Unicode 路径
static bool Imwrite(std::filesystem::path const &file_path, cv::Mat const &img,
                    std::vector<int> const &params = {})
{
    // Diagnostics::Trace::TraceInfo(L"正在保存图片: " + file_path.generic_wstring());
    std::vector<uint8_t> buffer;
    auto const ext = file_path.extension().string();
    if (!cv::imencode(ext, img, buffer, params))
        return false;
    std::ofstream file(file_path, std::ios::binary);
    if (!file.is_open())
        return false;
    return static_cast<bool>(file.write(reinterpret_cast<char const *>(buffer.data()), buffer.size()));
}

#undef min
#undef max

/*
 * 跟之前的区别时是旧的函数会自动计算布局和排序，修改布局为输入，按照数组顺序排序
 */
cv::Mat stitch(std::vector<cv::Mat const *> const &images, StretchMode stretchMode, ImageShape target_size, ImageUtilsNamespace::ImageLayout const &layout, int borderThickness)
{
    if (images.empty())
    {
        Diagnostics::Trace::TraceError(L"错误: 输入图片列表为空，无法进行拼接。");
        return cv::Mat();
    }

    if (borderThickness < 0)
    {
        borderThickness = std::min(target_size.width, target_size.height) / 33; // 默认边距为格子尺寸的 3%
    }

    // 2. 创建拼接画布
    const int canvas_width = layout.cols * target_size.width + (layout.cols + 1) * borderThickness;
    const int canvas_height = layout.rows * target_size.height + (layout.rows + 1) * borderThickness;
    cv::Mat canvas = cv::Mat::zeros(canvas_height, canvas_width, CV_8UC3);
    ImageTools::GradientFiller::FillBilinear(canvas,
                                             PixelData::PantoneColors::YearColor_2025_MochaMousse,
                                             PixelData::PantoneColors::YearColor_2024_PeachFuzz,
                                             PixelData::PantoneColors::YearColor_2023_VivaMagenta,
                                             PixelData::PantoneColors::YearColor_2022_VeryPeri);

    // 3. 循环填充图片
    for (int i = 0; i < static_cast<int>(images.size()); ++i)
    {
        int row_idx = i / layout.cols;
        int col_idx = i % layout.cols;

        if (row_idx >= layout.rows)
        {
            Diagnostics::Trace::TraceError("警告: 图片数量超过了计算的行列数，剩余图片将被忽略。");
            break; // 防止越界
        }

        // 定义画布上的感兴趣区域 (ROI)
        cv::Mat roi = canvas(cv::Rect(
            borderThickness + col_idx * (target_size.width + borderThickness),
            borderThickness + row_idx * (target_size.height + borderThickness),
            target_size.width,
            target_size.height));
        ImagePaster::Paste(&roi, images[i], stretchMode);
    }
    return canvas;
}

/**
 * @brief 拼接图片的核心算法
 * @param images 输入的图片向量
 * @param target_width 每个格子的目标宽度
 * @param target_height 每个格子的目标高度
 * @return 拼接后的单张图片
 */
cv::Mat stitch(std::vector<cv::Mat const *> const &images, StretchMode stretchMode, ImageShape target_size, int borderThickness)
{
    if (images.size() == 1)
    {
        target_size.width = images[0]->cols;
        target_size.height = images[0]->rows;
    }

    Diagnostics::Trace::TraceInfo(std::format(L"开始拼接 {} 张图片，每个格子尺寸: {}*{}，边距: {}",
                                              images.size(), target_size.width, target_size.height, borderThickness));

    auto layout = ImageTools::LayoutHelper::CalculateGridLayout(static_cast<int>(images.size()), target_size.width, target_size.height);
    Diagnostics::Trace::TraceInfo(std::format(L"计算得到的布局: {} 行 x {} 列。", layout.rows, layout.cols));

    return stitch(images, stretchMode, target_size, layout, borderThickness);
}

/**
 * @brief 拼接图片的核心算法
 * @param images 输入的图片向量
 * @param target_width 每个格子的目标宽度
 * @param target_height 每个格子的目标高度
 * @return 拼接后的单张图片
 */
cv::Mat stitch(const std::vector<cv::Mat> &images, StretchMode stretchMode, ImageShape target_size, int borderThickness = -1)
{
    std::vector<cv::Mat const *> ptrs{images.size()};
    std::transform(images.begin(), images.end(), ptrs.begin(), [](cv::Mat const &m)
                   { return &m; });
    return stitch(ptrs, stretchMode, target_size, borderThickness);
}

/**
 * @brief 基于路径的拼接 —— 逐个加载即用即弃，峰值内存仅画布 + 1 张图片
 */
cv::Mat stitch(std::vector<std::filesystem::path> const &image_paths, StretchMode stretchMode, ImageShape target_size, ImageUtilsNamespace::ImageLayout const &layout, int borderThickness)
{
    if (image_paths.empty())
    {
        Diagnostics::Trace::TraceError(L"错误: 输入图片列表为空，无法进行拼接。");
        return cv::Mat();
    }

    if (borderThickness < 0)
    {
        borderThickness = std::min(target_size.width, target_size.height) / 33;
    }

    const int canvas_width = layout.cols * target_size.width + (layout.cols + 1) * borderThickness;
    const int canvas_height = layout.rows * target_size.height + (layout.rows + 1) * borderThickness;
    cv::Mat canvas = cv::Mat::zeros(canvas_height, canvas_width, CV_8UC3);
    ImageTools::GradientFiller::FillBilinear(canvas,
                                             PixelData::PantoneColors::YearColor_2025_MochaMousse,
                                             PixelData::PantoneColors::YearColor_2024_PeachFuzz,
                                             PixelData::PantoneColors::YearColor_2023_VivaMagenta,
                                             PixelData::PantoneColors::YearColor_2022_VeryPeri);

    for (int i = 0; i < static_cast<int>(image_paths.size()); ++i)
    {
        int row_idx = i / layout.cols;
        int col_idx = i % layout.cols;

        if (row_idx >= layout.rows)
        {
            Diagnostics::Trace::TraceError("警告: 图片数量超过了计算的行列数，剩余图片将被忽略。");
            break;
        }

        cv::Mat roi = canvas(cv::Rect(
            borderThickness + col_idx * (target_size.width + borderThickness),
            borderThickness + row_idx * (target_size.height + borderThickness),
            target_size.width,
            target_size.height));

        // 逐张加载，用完即释
        cv::Mat img = Imread(image_paths[i], cv::IMREAD_COLOR);
        if (!img.empty())
            ImagePaster::Paste(&roi, &img, stretchMode);
    }
    return canvas;
}

#ifdef _CUDA_OPENCV
/**
 * @brief CUDA 加速版拼接（指针重载，需指定布局）
 */
cv::Mat stitchCUDA(std::vector<cv::Mat const *> const &images, StretchMode stretchMode, ImageShape target_size, ImageUtilsNamespace::ImageLayout const &layout, int borderThickness)
{
    if (images.empty())
    {
        Diagnostics::Trace::TraceError(L"错误: 输入图片列表为空，无法进行拼接。");
        return cv::Mat();
    }

    if (borderThickness < 0)
    {
        borderThickness = std::min(target_size.width, target_size.height) / 33;
    }

    const int canvas_width = layout.cols * target_size.width + (layout.cols + 1) * borderThickness;
    const int canvas_height = layout.rows * target_size.height + (layout.rows + 1) * borderThickness;

    cv::Mat canvas = cv::Mat::zeros(canvas_height, canvas_width, CV_8UC3);
    ImageTools::GradientFiller::FillBilinear(canvas,
                                             PixelData::PantoneColors::YearColor_2025_MochaMousse,
                                             PixelData::PantoneColors::YearColor_2024_PeachFuzz,
                                             PixelData::PantoneColors::YearColor_2023_VivaMagenta,
                                             PixelData::PantoneColors::YearColor_2022_VeryPeri);

    cv::cuda::GpuMat gpu_canvas;
    gpu_canvas.upload(canvas);

    for (int i = 0; i < static_cast<int>(images.size()); ++i)
    {
        int row_idx = i / layout.cols;
        int col_idx = i % layout.cols;
        if (row_idx >= layout.rows)
            break;

        cv::Rect roi_rect(
            borderThickness + col_idx * (target_size.width + borderThickness),
            borderThickness + row_idx * (target_size.height + borderThickness),
            target_size.width, target_size.height);

        cv::cuda::GpuMat gpu_img;
        gpu_img.upload(*images[i]);

        cv::cuda::GpuMat gpu_resized;
        if (images[i]->cols != target_size.width || images[i]->rows != target_size.height)
        {
            cv::cuda::resize(gpu_img, gpu_resized, cv::Size(target_size.width, target_size.height));
        }
        else
        {
            gpu_resized = gpu_img;
        }

        cv::cuda::GpuMat gpu_roi = gpu_canvas(roi_rect);
        gpu_resized.copyTo(gpu_roi);
    }

    gpu_canvas.download(canvas);
    return canvas;
}

/**
 * @brief CUDA 加速版拼接（std::vector<cv::Mat> 重载）
 */
cv::Mat stitchCUDA(const std::vector<cv::Mat> &images, StretchMode stretchMode, ImageShape target_size, int borderThickness = -1)
{
    std::vector<cv::Mat const *> ptrs{images.size()};
    std::transform(images.begin(), images.end(), ptrs.begin(), [](cv::Mat const &m)
                   { return &m; });
    return stitchCUDA(ptrs, stretchMode, target_size, ImageTools::LayoutHelper::CalculateGridLayout(static_cast<int>(images.size()), target_size.width, target_size.height), borderThickness);
}

/**
 * @brief CUDA 加速版基于路径的拼接 —— 逐个加载即用即弃
 */
cv::Mat stitchCUDA(std::vector<std::filesystem::path> const &image_paths, StretchMode stretchMode, ImageShape target_size, ImageUtilsNamespace::ImageLayout const &layout, int borderThickness)
{
    if (image_paths.empty())
    {
        Diagnostics::Trace::TraceError(L"错误: 输入图片列表为空，无法进行拼接。");
        return cv::Mat();
    }

    if (borderThickness < 0)
    {
        borderThickness = std::min(target_size.width, target_size.height) / 33;
    }

    const int canvas_width = layout.cols * target_size.width + (layout.cols + 1) * borderThickness;
    const int canvas_height = layout.rows * target_size.height + (layout.rows + 1) * borderThickness;

    cv::Mat canvas = cv::Mat::zeros(canvas_height, canvas_width, CV_8UC3);
    ImageTools::GradientFiller::FillBilinear(canvas,
                                             PixelData::PantoneColors::YearColor_2025_MochaMousse,
                                             PixelData::PantoneColors::YearColor_2024_PeachFuzz,
                                             PixelData::PantoneColors::YearColor_2023_VivaMagenta,
                                             PixelData::PantoneColors::YearColor_2022_VeryPeri);

    cv::cuda::GpuMat gpu_canvas;
    gpu_canvas.upload(canvas);

    for (int i = 0; i < static_cast<int>(image_paths.size()); ++i)
    {
        int row_idx = i / layout.cols;
        int col_idx = i % layout.cols;
        if (row_idx >= layout.rows)
            break;

        cv::Rect roi_rect(
            borderThickness + col_idx * (target_size.width + borderThickness),
            borderThickness + row_idx * (target_size.height + borderThickness),
            target_size.width, target_size.height);

        // 逐张加载到 CPU，上传 GPU，用完即释
        cv::Mat img = Imread(image_paths[i], cv::IMREAD_COLOR);
        if (img.empty())
            continue;

        cv::cuda::GpuMat gpu_img;
        gpu_img.upload(img);
        img.release(); // CPU 侧立即释放

        cv::cuda::GpuMat gpu_resized;
        if (gpu_img.cols != target_size.width || gpu_img.rows != target_size.height)
        {
            cv::cuda::resize(gpu_img, gpu_resized, cv::Size(target_size.width, target_size.height));
        }
        else
        {
            gpu_resized = gpu_img;
        }

        cv::cuda::GpuMat gpu_roi = gpu_canvas(roi_rect);
        gpu_resized.copyTo(gpu_roi);
        // gpu_img, gpu_resized, gpu_roi 在此释放
    }

    gpu_canvas.download(canvas);
    return canvas;
}
#endif // _CUDA_OPENCV

ImageNamespace::Image ImageNamespace::stitch(std::vector<void *> mats, StretchMode mode, ImageShape size, ImageUtilsNamespace::ImageLayout layout, int spacing)
{
    auto transforma = mats | std::views::transform([](void *ptr)
                                                   { return reinterpret_cast<cv::Mat const *>(ptr); }) |
                      std::ranges::to<std::vector>();
    auto img = ::stitch(transforma, mode, size, layout, spacing);
    return ImageNamespace::Image::FromNativeMatPointer(&img);
}

int ImageNamespace::stitch_cli_main(std::vector<std::string> const &args)
{
    // Diagnostics::Trace::TraceInfo(L"HamsterStudioToes Image Stitcher CLI Tool");
#ifdef _CUDA_OPENCV
    if (int deviceCount = cv::cuda::getCudaEnabledDeviceCount(); deviceCount == 0)
    {
        Diagnostics::Trace::TraceInfo("No CUDA device enabled.");
        return 1;
    }
    else
    {
        Diagnostics::Trace::TraceInfo(std::format("{} CUDA device(s) enabled.", deviceCount));
    }
#else
    Diagnostics::Trace::TraceInfo("_CUDA_OPENCV not defined, using CPU implementation.");
#endif

    namespace fs = std::filesystem;
    HamsterStudioToes::StopWatch watch;

    fs::path image_folder_path = fs::current_path();

    // 收集图片路径
    auto image_paths = fs::directory_iterator(image_folder_path) | std::views::filter([](auto const &entry)
                                                                                      { return entry.is_regular_file(); }) |
                       std::views::transform([](auto const &entry)
                                             { return entry.path(); }) |
                       std::views::filter([](auto const &path)
                                          { return !path.filename().wstring().starts_with(L"result_"); }) |
                       std::views::filter(IsSupportedExtension) | std::ranges::to<std::vector>();

    std::ranges::sort(image_paths, Win32Utils::PathCompare);
    Diagnostics::Trace::TraceInfo(std::format(L"找到 {} 张图片，正在加载...", image_paths.size()));

    if (image_paths.empty())
    {
        std::cerr << "Error: No supported images found in the current directory." << std::endl;
        return 1;
    }

    // 按横竖分组（只检查尺寸，不保留图片）
    std::vector<fs::path> portrait_paths, landscape_paths;
    for (auto const &path : image_paths)
    {
        cv::Mat img = Imread(path, cv::IMREAD_REDUCED_COLOR_8);
        if (img.empty())
            continue;
        if (img.rows > img.cols)
            portrait_paths.push_back(path);
        else
            landscape_paths.push_back(path);
        Diagnostics::Trace::TraceInfo(L"Checked: " + path.generic_wstring());
        // img 在此释放，不占用内存
    }

    size_t const total_count = portrait_paths.size() + landscape_paths.size();
    if (total_count == 0)
    {
        std::cerr << "Error: Failed to load any images." << std::endl;
        return 1;
    }

    Diagnostics::Trace::TraceInfo(std::format(L"加载完成。共计: {} 张 (横屏: {}, 竖屏: {})。",
                                              total_count, landscape_paths.size(), portrait_paths.size()));

    auto name = image_folder_path.filename().wstring();
    if (name == L"\u53c2\u8003")
    {
        name = image_folder_path.parent_path().filename().wstring();
    }

    constexpr ImageShape DefaultShape{2560, 1600, 3};

    /// 拼接时逐张加载，峰值内存仅画布 + 1 张图片
    auto do_stitch = [&](std::vector<fs::path> const &paths, ImageStitcheMode mode, fs::path const &out_path)
    {
        if (paths.empty())
        {
            Diagnostics::Trace::TraceWarning(L"没有足够的图片进行拼接。");
            return;
        }

        // 仅加载首张以计算目标宽高比
        cv::Mat first = Imread(paths[0], cv::IMREAD_COLOR);
        if (first.empty())
        {
            Diagnostics::Trace::TraceWarning(L"无法读取首张图片，跳过拼接。");
            return;
        }

        ImageShape target{DefaultShape.width, DefaultShape.height, DefaultShape.channels};
        if (mode == ImageStitcheMode::Portrait)
            target.width = static_cast<int>(DefaultShape.width * first.cols / first.rows);
        else if (mode == ImageStitcheMode::Landscape)
            target.height = static_cast<int>(DefaultShape.width * first.rows / first.cols);
        first.release();

        auto layout = ImageTools::LayoutHelper::CalculateGridLayout(static_cast<int>(paths.size()), target.width, target.height);
        Diagnostics::Trace::TraceInfo(std::format(L"布局: {} 行 x {} 列。", layout.rows, layout.cols));

        cv::Mat result;
#ifdef _CUDA_OPENCV
        Diagnostics::Trace::TraceInfo(L"使用 CUDA 加速进行图片拼接...");
        result = ::stitchCUDA(paths, StretchMode::Uniform, target, layout, -1);
#else
        Diagnostics::Trace::TraceInfo(L"使用 CPU 进行图片拼接...");
        result = ::stitch(paths, StretchMode::Uniform, target, layout, -1);
#endif
        if (!result.empty())
        {
           if(Imwrite(out_path, result))
                Diagnostics::Trace::TraceInfo(L"已保存: " + out_path.generic_wstring());
            else
                Diagnostics::Trace::TraceError(L"保存失败: " + out_path.generic_wstring());
        }
    };

    auto portrait_file = image_folder_path / (L"result_" + name + L"_portrait.jpg");
    auto landscape_file = image_folder_path / (L"result_" + name + L"_landscape.jpg");

    auto portrait_task = [&]
    { do_stitch(portrait_paths, ImageStitcheMode::Portrait, portrait_file); };
    auto landscape_task = [&]
    { do_stitch(landscape_paths, ImageStitcheMode::Landscape, landscape_file); };

    if (args.size() != 2)
    {
#ifdef _DEBUG
        Diagnostics::Trace::TraceWarning("No mode specified, generating all results sequentially...");
        portrait_task();
        landscape_task();
#else
        Diagnostics::Trace::TraceWarning("No mode specified, generating all results in parallel...");
        std::jthread th_portrait(portrait_task);
        std::jthread th_landscape(landscape_task);
        th_landscape.join();
        th_portrait.join();
#endif
    }
    else if (args[1] == "portrait" || args[1] == "p")
    {
        Diagnostics::Trace::TraceInfo("Generating portrait stitched image...");
        portrait_task();
    }
    else if (args[1] == "landscape" || args[1] == "l")
    {
        Diagnostics::Trace::TraceInfo("Generating landscape stitched image...");
        landscape_task();
    }
    else if (args[1] == "all" || args[1] == "a")
    {
        Diagnostics::Trace::TraceInfo("Generating all stitched images...");
        // 横屏在前，竖屏在后
        auto all_paths = landscape_paths;
        all_paths.insert(all_paths.end(), portrait_paths.begin(), portrait_paths.end());
        auto allinone_file = image_folder_path / (L"result_" + name + L"_all.jpg");
        do_stitch(all_paths, ImageStitcheMode::None, allinone_file);
    }
    else
    {
        std::cerr << "Invalid argument: " << args[1] << std::endl;
        std::cerr << "Usage: " << args[0] << " [portrait|landscape|all]" << std::endl;
        return 1;
    }

    auto elapsed_ms = std::chrono::duration_cast<std::chrono::milliseconds>(watch.elapsed()).count();
    Diagnostics::Trace::TraceInfo(std::format(L"Total processing time: {} ms", elapsed_ms));
    system("pause");
    return 0;
}
