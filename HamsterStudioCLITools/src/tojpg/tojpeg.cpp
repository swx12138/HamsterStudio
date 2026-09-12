
/**
 * hamster-tojpg — 将图片转换为 JPEG 格式
 *
 * 基于 HamsterStudioToes (OpenCV) 实现，支持 Unicode 路径。
 *
 * 用法:
 *   hamster-tojpg <输入图片路径> [选项]
 *
 * 选项:
 *   -o, --output <路径>    输出文件路径 (默认: 输入路径替换扩展名为 .jpg)
 *   -q, --quality <值>     JPEG 质量 1-100 (默认: 95)
 *       --mirror <方向>    镜像方向: horizontal/vertical/both (默认: 不镜像)
 *   -h, --help             显示此帮助信息
 *      --version           显示版本信息
 *
 * 示例:
 *   hamster-tojpg input.png
 *   hamster-tojpg input.png -o output.jpg -q 90
 */

#include <Image/Tools/ImageUtils.hpp>

#include <Windows.h>

#include <filesystem>
#include <format>
#include <iostream>
#include <string>
#include <string_view>
#include <vector>

namespace fs = std::filesystem;

static constexpr const char* kVersion = "1.0.0";

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
    std::cout << R"(用法: hamster-tojpg <输入图片路径> [选项]

将图片转换为 JPEG 格式。

选项:
  -o, --output <路径>    输出文件路径 (默认: 输入路径替换扩展名为 .jpg)
  -q, --quality <值>     JPEG 质量 1-100 (默认: 95)
      --mirror <方向>    镜像方向: horizontal/vertical/both (默认: 不镜像)
  -h, --help             显示此帮助信息
      --version          显示版本信息

示例:
  hamster-tojpg input.png
  hamster-tojpg input.png -o output.jpg -q 90
    hamster-tojpg input.png --mirror horizontal
)";
}

static void PrintVersion()
{
    std::cout << "hamster-tojpg (HamsterStudioCLITools) " << kVersion << "\n"
        << "将图片转换为 JPEG 格式，使用 C++23 编写。\n";
}

int wmain(int argc, wchar_t** argv)
{
    // 将控制台代码页设置为 UTF-8
    SetConsoleOutputCP(CP_UTF8);
    SetConsoleCP(CP_UTF8);

    // 解析命令行参数（宽字符，保留 Unicode 路径）
    std::vector<std::wstring_view> args(argv + 1, argv + argc);

    if (args.empty())
    {
        PrintUsage();
        return 1;
    }

    fs::path input_path;
    fs::path output_path;
    int quality = 95;
    int mirror_code = 0;
    bool mirror_requested = false;

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
        else if (args[i] == L"-o" || args[i] == L"--output")
        {
            if (++i >= args.size())
            {
                std::cerr << "错误: -o/--output 缺少参数。\n";
                return 1;
            }
            output_path = fs::path(args[i]);
        }
        else if (args[i] == L"-q" || args[i] == L"--quality")
        {
            if (++i >= args.size())
            {
                std::cerr << "错误: -q/--quality 缺少参数。\n";
                return 1;
            }
            try
            {
                quality = std::stoi(std::wstring(args[i]));
            }
            catch (...)
            {
                std::cerr << "错误: 无效的质量值。\n";
                return 1;
            }
            if (quality < 1 || quality > 100)
            {
                std::cerr << "错误: 质量值必须在 1~100 之间。\n";
                return 1;
            }
        }
        else if (args[i] == L"--mirror" || args[i] == L"--flip")
        {
            if (++i >= args.size())
            {
                std::cerr << "错误: --mirror/--flip 缺少方向参数。\n";
                return 1;
            }
            if (args[i] == L"horizontal" || args[i] == L"h")
                mirror_code = 0;
            else if (args[i] == L"vertical" || args[i] == L"v")
                mirror_code = 1;
            else if (args[i] == L"both" || args[i] == L"hv" || args[i] == L"vh")
                mirror_code = -1;
            else
            {
                std::cerr << "错误: 无效的镜像方向: " << ToUtf8(args[i])
                    << "，可选值为 horizontal、vertical 或 both。\n";
                return 1;
            }
            mirror_requested = true;
        }
        else if (!args[i].empty() && args[i][0] == L'-')
        {
            std::cerr << "错误: 未知选项: " << ToUtf8(args[i]) << "\n";
            PrintUsage();
            return 1;
        }
        else
        {
            if (!input_path.empty())
            {
                std::cerr << "错误: 只能指定一个输入图片路径。\n";
                return 1;
            }
            input_path = fs::path(args[i]);
        }
    }

    if (input_path.empty())
    {
        std::cerr << "错误: 未指定输入图片路径。\n";
        PrintUsage();
        return 1;
    }

    if (!fs::exists(input_path))
    {
        std::cerr << "错误: 文件不存在: " << ToUtf8(input_path.native()) << "\n";
        return 1;
    }

    // 加载图片
    std::cout << "正在加载: " << ToUtf8(input_path.native()) << " ...\n";
    auto image = toes::Imread(input_path);

    if (image.empty())
    {
        std::cerr << "错误: 无法加载图片: " << ToUtf8(input_path.native()) << "\n";
        return 1;
    }

    if (mirror_requested)
    {
        cv::flip(image, image, mirror_code);
        std::cout << "已应用镜像: "
            << (mirror_code == 1 ? "horizontal" : mirror_code == 0 ? "vertical" : "both")
            << "\n";
    }

    std::cout << std::format("图片尺寸: {}x{}\n", image.cols, image.rows);

    // 确定输出路径
    if (output_path.empty())
    {
        if (mirror_requested) {
            std::string appe;
            if (mirror_code == 1) {
                appe = "_VF";
            }
            else if (mirror_code == 0) {
                appe = "_HF";
            }
            else if (mirror_code == -1) {
                appe = "_BF";
            }
            output_path = input_path.parent_path() / (input_path.stem().string() + appe + input_path.extension().string());
        }
        else {
            output_path = input_path;
            if (output_path.has_extension())
                output_path.replace_extension(L".jpg");
            else
                output_path += L".jpg";
        }
    }

    if (output_path == input_path)
    {
        std::cerr << "错误: 输入文件已是 JPEG 格式，请使用 -o 指定不同的输出路径。\n";
        return 1;
    }

    if (fs::exists(output_path))
    {
        std::cerr << "错误: 文件已存在: " << ToUtf8(output_path.native()) << "\n";
        return 1;
    }

    std::cout << "正在保存: " << ToUtf8(output_path.native()) << " (质量: " << quality << ") ...\n";

    if (!toes::Imwrite(output_path, image, { cv::IMWRITE_JPEG_QUALITY, quality }))
    {
        std::cerr << "错误: 保存失败: " << ToUtf8(output_path.native()) << "\n";
        return 1;
    }

    std::cout << "完成!\n";
    return 0;
}