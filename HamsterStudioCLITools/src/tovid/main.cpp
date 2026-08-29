/**
 * hamster-tovid — 将目录下的所有图片合成为一个视频
 *
 * 基于 HamsterStudioToes (OpenCV) 实现，支持 Unicode 图片路径。
 *
 * 用法:
 *   hamster-tovid [选项]
 *
 * 选项:
 *   -d, --dir <目录>       图片目录 (默认: 当前目录)
 *   -o, --output <路径>    输出视频路径 (默认: output.mp4)
 *   -f, --fps <值>         视频帧率，每张图片 1 帧 (默认: 6)
 *   -c, --codec <名称>     编码器 fourcc: mp4v/avc1/h264/x264/mjpg/xvid (默认: h264)
 *   -h, --help             显示此帮助信息
 *      --version           显示版本信息
 *
 * 说明:
 *   视频宽高与图片宽高一致，若超过 4K (3840x2160) 则等比缩小；
 *   横图与竖图分开合成，分别输出为 <输出名>_landscape 与 <输出名>_portrait。
 */

// 禁用 Windows.h 的 min/max 宏，避免与 std::min/std::max 冲突
#define NOMINMAX

#include <Image/Tools/ImageUtils.hpp>

#include <Windows.h>

#include <algorithm>
#include <cmath>
#include <cwctype>
#include <filesystem>
#include <format>
#include <iostream>
#include <string>
#include <string_view>
#include <vector>

namespace fs = std::filesystem;

static constexpr const char* kVersion = "1.0.0";

// 4K 上限
static constexpr int kMaxWidth = 3840;
static constexpr int kMaxHeight = 2160;

// 将宽字符串转换为 UTF-8，用于控制台输出（配合 CP_UTF8 代码页）
static std::string ToUtf8(std::wstring_view w)
{
    if (w.empty())
        return {};
    int n = ::WideCharToMultiByte(CP_UTF8, 0, w.data(), static_cast<int>(w.size()),
                                  nullptr, 0, nullptr, nullptr);
    std::string s(static_cast<size_t>(n), '\0');
    ::WideCharToMultiByte(CP_UTF8, 0, w.data(), static_cast<int>(w.size()),
                          s.data(), n, nullptr, nullptr);
    return s;
}

static void PrintUsage()
{
    std::cout << R"(用法: hamster-tovid [选项]

将目录下的图片按横竖方向分别合成为视频。

选项:
  -d, --dir <目录>       图片目录 (默认: 当前目录)
  -o, --output <路径>    输出视频路径 (默认: output.mp4)
  -f, --fps <值>         视频帧率，每张图片 1 帧 (默认: 6)
  -c, --codec <名称>     编码器 fourcc: mp4v/avc1/h264/x264/mjpg/xvid (默认: h264)
  -h, --help             显示此帮助信息
      --version          显示版本信息

说明:
  视频宽高与图片宽高一致，若超过 4K (3840x2160) 则等比缩小；
  横图与竖图分开合成，分别输出为 <输出名>_landscape 与 <输出名>_portrait。

示例:
  hamster-tovid
  hamster-tovid -d ./frames -o out.mp4 -f 2 -c avc1
)";
}

static void PrintVersion()
{
    std::cout << "hamster-tovid (HamsterStudioCLITools) " << kVersion << "\n"
              << "将图片合成为视频，使用 C++23 编写。\n";
}

// 根据 fourcc 名称解析编码器
static int ParseFourcc(std::string_view name)
{
    if (name == "mp4v")  return cv::VideoWriter::fourcc('m', 'p', '4', 'v');
    if (name == "avc1")  return cv::VideoWriter::fourcc('a', 'v', 'c', '1');
    if (name == "h264")  return cv::VideoWriter::fourcc('a', 'v', 'c', '1');
    if (name == "x264")  return cv::VideoWriter::fourcc('X', '2', '6', '4');
    if (name == "mjpg" || name == "mjpeg") return cv::VideoWriter::fourcc('M', 'J', 'P', 'G');
    if (name == "xvid")  return cv::VideoWriter::fourcc('X', 'V', 'I', 'D');
    if (name.size() == 4)
        return cv::VideoWriter::fourcc(name[0], name[1], name[2], name[3]);
    return cv::VideoWriter::fourcc('m', 'p', '4', 'v');
}

// 自然排序比较（数字按数值大小比较，避免 img2 > img10）
static bool NaturalLess(const fs::path& a, const fs::path& b)
{
    const std::wstring sa = a.filename().native();
    const std::wstring sb = b.filename().native();

    size_t i = 0, j = 0;
    while (i < sa.size() && j < sb.size())
    {
        if (std::iswdigit(sa[i]) && std::iswdigit(sb[j]))
        {
            size_t i0 = i, j0 = j;
            while (i < sa.size() && std::iswdigit(sa[i])) ++i;
            while (j < sb.size() && std::iswdigit(sb[j])) ++j;
            std::wstring na = sa.substr(i0, i - i0);
            std::wstring nb = sb.substr(j0, j - j0);
            // 去掉前导零后按长度与字典序比较
            auto strip = [](std::wstring s)
            {
                size_t p = s.find_first_not_of(L'0');
                return p == std::wstring::npos ? std::wstring() : s.substr(p);
            };
            std::wstring ca = strip(na), cb = strip(nb);
            if (ca.size() != cb.size())
                return ca.size() < cb.size();
            if (ca != cb)
                return ca < cb;
        }
        else
        {
            wchar_t la = static_cast<wchar_t>(std::towlower(sa[i]));
            wchar_t lb = static_cast<wchar_t>(std::towlower(sb[j]));
            if (la != lb)
                return la < lb;
            ++i; ++j;
        }
    }
    return sa.size() < sb.size();
}

// 计算目标尺寸：与图片宽高一致，超过 4K 则等比缩小，并调整为偶数
static cv::Size ComputeTargetSize(cv::Size src)
{
    double scale = std::min(
        1.0,
        std::min(static_cast<double>(kMaxWidth) / src.width,
                 static_cast<double>(kMaxHeight) / src.height));
    int w = std::max(1, static_cast<int>(std::round(src.width * scale)));
    int h = std::max(1, static_cast<int>(std::round(src.height * scale)));
    w += w & 1;  // 编码器通常要求偶数宽高
    h += h & 1;
    return { w, h };
}

// 保持宽高比缩放并居中填充黑边到目标尺寸
static cv::Mat FitToSize(const cv::Mat& img, cv::Size target)
{
    double scale = std::min(
        static_cast<double>(target.width) / img.cols,
        static_cast<double>(target.height) / img.rows);
    int new_w = std::max(1, static_cast<int>(std::round(img.cols * scale)));
    int new_h = std::max(1, static_cast<int>(std::round(img.rows * scale)));
    new_w = std::min(new_w, target.width);   // 防止舍入导致超出画布
    new_h = std::min(new_h, target.height);

    cv::Mat resized;
    cv::resize(img, resized, cv::Size(new_w, new_h));

    cv::Mat canvas = cv::Mat::zeros(target, img.type());
    int x = (target.width - new_w) / 2;
    int y = (target.height - new_h) / 2;
    resized.copyTo(canvas(cv::Rect(x, y, new_w, new_h)));
    return canvas;
}

// 在扩展名前插入后缀，如 output.mp4 → output_landscape.mp4
static fs::path MakeGroupOutput(const fs::path& base, std::wstring_view suffix)
{
    fs::path result = base;
    result.replace_filename(base.stem().wstring() + std::wstring(suffix) + base.extension().wstring());
    return result;
}

// 将一组图片合成为一个视频（图片缩放时保持宽高比，居中填充黑边）
static bool WriteVideo(const std::vector<fs::path>& paths,
                       const fs::path& output_path,
                       cv::Size target,
                       double fps, int fourcc, const std::string& codec,
                       int group_index)
{
    std::cout << std::format("视频尺寸: {}x{} (帧率: {})\n", target.width, target.height, fps);

    // 先写入当前目录下的临时 ASCII 文件名，再重命名为最终路径，
    // 以避免 cv::VideoWriter 对 Unicode 输出路径支持不佳的问题
    fs::path tmp_path = fs::current_path() /
        (".hamster-tovid-tmp-" + std::to_string(::GetCurrentProcessId()) + "-" + std::to_string(group_index) + ".mp4");

    cv::VideoWriter writer(tmp_path.string(), fourcc, fps, target);
    if (!writer.isOpened())
    {
        std::cerr << "错误: 无法创建视频写入器 (编码器: " << codec << ")。\n";
        return false;
    }

    std::cout << "正在合成视频...\n";
    size_t written = 0;
    for (const auto& image_path : paths)
    {
        cv::Mat img = toes::Imread(image_path);
        if (img.empty())
        {
            std::cerr << "警告: 跳过无法加载的图片: " << ToUtf8(image_path.native()) << "\n";
            continue;
        }
        if (img.size() != target)
            img = FitToSize(img, target);
        writer.write(img);
        ++written;
        std::cout << std::format("  [{}/{}] {}\n", written, paths.size(), ToUtf8(image_path.filename().native()));
    }

    writer.release();

    if (written == 0)
    {
        std::cerr << "错误: 没有成功写入任何帧。\n";
        std::error_code ec;
        fs::remove(tmp_path, ec);
        return false;
    }

    // 重命名为最终输出路径（若已存在则先删除以覆盖）
    std::error_code ec;
    fs::remove(output_path, ec);
    ec.clear();
    fs::rename(tmp_path, output_path, ec);
    if (ec)
    {
        std::cerr << "错误: 无法重命名输出文件: " << ec.message() << "\n";
        return false;
    }

    std::cout << std::format("完成! 共 {} 帧，输出: {}\n", written, ToUtf8(output_path.native()));
    return true;
}

int wmain(int argc, wchar_t** argv)
{
    // 将控制台代码页设置为 UTF-8
    SetConsoleOutputCP(CP_UTF8);
    SetConsoleCP(CP_UTF8);

    // 解析命令行参数（宽字符，保留 Unicode 路径）
    std::vector<std::wstring_view> args(argv + 1, argv + argc);

    fs::path input_dir = fs::current_path();
    fs::path output_path = "output.mp4";
    double fps = 6.0;
    std::string codec = "h264";

    for (size_t i = 0; i < args.size(); ++i)
    {
        if (args[i] == L"-h" || args[i] == L"--help")
        {
            PrintUsage();
            return 0;
        }
        else if (args[i] == L"--version")
        {
            PrintVersion();
            return 0;
        }
        else if (args[i] == L"-d" || args[i] == L"--dir")
        {
            if (++i >= args.size())
            {
                std::cerr << "错误: -d/--dir 缺少参数。\n";
                return 1;
            }
            input_dir = fs::path(args[i]);
        }
        else if (args[i] == L"-o" || args[i] == L"--output")
        {
            if (++i >= args.size())
            {
                std::cerr << "错误: -o/--output 缺少参数。\n";
                return 1;
            }
            output_path = fs::path(args[i]);
        }
        else if (args[i] == L"-f" || args[i] == L"--fps")
        {
            if (++i >= args.size())
            {
                std::cerr << "错误: -f/--fps 缺少参数。\n";
                return 1;
            }
            try
            {
                fps = std::stod(std::wstring(args[i]));
            }
            catch (...)
            {
                std::cerr << "错误: 无效的帧率。\n";
                return 1;
            }
            if (fps <= 0)
            {
                std::cerr << "错误: 帧率必须大于 0。\n";
                return 1;
            }
        }
        else if (args[i] == L"-c" || args[i] == L"--codec")
        {
            if (++i >= args.size())
            {
                std::cerr << "错误: -c/--codec 缺少参数。\n";
                return 1;
            }
            codec = ToUtf8(args[i]);
        }
        else if (!args[i].empty() && args[i][0] == L'-')
        {
            std::cerr << "错误: 未知选项: " << ToUtf8(args[i]) << "\n";
            PrintUsage();
            return 1;
        }
        else
        {
            input_dir = fs::path(args[i]);
        }
    }

    if (!fs::exists(input_dir) || !fs::is_directory(input_dir))
    {
        std::cerr << "错误: 目录不存在: " << ToUtf8(input_dir.native()) << "\n";
        return 1;
    }

    // 收集目录下所有受支持的图片，并按文件名自然排序
    std::vector<fs::path> images;
    for (const auto& entry : fs::directory_iterator(input_dir))
    {
        if (!entry.is_regular_file())
            continue;
        if (!toes::IsSupportedExtension(entry.path()))
            continue;
        images.push_back(entry.path());
    }
    std::sort(images.begin(), images.end(), NaturalLess);

    if (images.empty())
    {
        std::cerr << "错误: 目录中没有找到图片: " << ToUtf8(input_dir.native()) << "\n";
        return 1;
    }

    // 按横竖方向分组（宽 >= 高 视为横图，否则为竖图），并记录每组最大宽高
    std::vector<fs::path> landscape, portrait;
    int landscape_max_w = 0, landscape_max_h = 0;
    int portrait_max_w = 0, portrait_max_h = 0;
    for (const auto& image_path : images)
    {
        cv::Mat probe = toes::Imread(image_path);
        if (probe.empty())
        {
            std::cerr << "警告: 跳过无法加载的图片: " << ToUtf8(image_path.native()) << "\n";
            continue;
        }
        if (probe.cols >= probe.rows)
        {
            landscape.push_back(image_path);
            landscape_max_w = std::max(landscape_max_w, probe.cols);
            landscape_max_h = std::max(landscape_max_h, probe.rows);
        }
        else
        {
            portrait.push_back(image_path);
            portrait_max_w = std::max(portrait_max_w, probe.cols);
            portrait_max_h = std::max(portrait_max_h, probe.rows);
        }
    }

    if (landscape.empty() && portrait.empty())
    {
        std::cerr << "错误: 没有可用的图片。\n";
        return 1;
    }

    std::cout << std::format("共找到 {} 张图片 (横图 {} 张, 竖图 {} 张)\n",
                             images.size(), landscape.size(), portrait.size());

    int fourcc = ParseFourcc(codec);
    int group_index = 0;
    bool ok = true;

    if (!landscape.empty())
    {
        fs::path out = MakeGroupOutput(output_path, L"_landscape");
        std::cout << std::format("\n[横图] {} 张 → {}\n", landscape.size(), ToUtf8(out.native()));
        ok &= WriteVideo(landscape, out, ComputeTargetSize({ landscape_max_w, landscape_max_h }), fps, fourcc, codec, group_index++);
    }
    if (!portrait.empty())
    {
        fs::path out = MakeGroupOutput(output_path, L"_portrait");
        std::cout << std::format("\n[竖图] {} 张 → {}\n", portrait.size(), ToUtf8(out.native()));
        ok &= WriteVideo(portrait, out, ComputeTargetSize({ portrait_max_w, portrait_max_h }), fps, fourcc, codec, group_index++);
    }

    return ok ? 0 : 1;
}
