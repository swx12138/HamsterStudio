#include <Image/Image.h>
#include <Tools/BaitingMaster.h>

#include <Windows.h>

#include <format>
#include <iostream>
#include <string>
#include <string_view>

using namespace HamsterStudioToes;

static void PrintUsage()
{
    std::cout << "用法: hamster-bait <输入图片路径> [选项]\n"
              << "将图片水平分割成多个子图。\n"
              << "\n"
              << "选项:\n"
              << "  -c, --cols <数量>     分割列数 (默认: 2)\n"
              << "  -o, --output <前缀>   输出文件名前缀 (默认: bait)\n"
              << "  -q, --quality <值>    输出图片质量 1-100 (默认: 95)\n"
              << "  -h, --help            显示此帮助信息\n"
              << "\n"
              << "示例:\n"
              << "  hamster-bait input.jpg\n"
              << "  hamster-bait input.png -c 3 -o split -q 90\n";
}

int main(int argc, char** argv)
{
    // 将控制台代码页设置为 UTF-8
    SetConsoleOutputCP(CP_UTF8);
    SetConsoleCP(CP_UTF8);

    // 解析命令行参数
    std::vector<std::string_view> args(argv + 1, argv + argc);

    if (args.empty() || args[0] == "-h" || args[0] == "--help")
    {
        PrintUsage();
        return args.empty() ? 1 : 0;
    }

    std::filesystem::path input_path;
    int cols = 2;
    std::string output_prefix = "bait";
    bool output_prefix_set = false;
    int quality = 95;

    for (size_t i = 0; i < args.size(); ++i)
    {
        if (args[i] == "-c" || args[i] == "--cols")
        {
            if (++i >= args.size())
            {
                std::cerr << "错误: -c/--cols 缺少参数。\n";
                return 1;
            }
            cols = std::stoi(std::string(args[i]));
            if (cols < 1 || cols > 16)
            {
                std::cerr << "错误: 列数必须在 1~16 之间。\n";
                return 1;
            }
        }
        else if (args[i] == "-o" || args[i] == "--output")
        {
            if (++i >= args.size())
            {
                std::cerr << "错误: -o/--output 缺少参数。\n";
                return 1;
            }
            output_prefix = args[i];
            output_prefix_set = true;
        }
        else if (args[i] == "-q" || args[i] == "--quality")
        {
            if (++i >= args.size())
            {
                std::cerr << "错误: -q/--quality 缺少参数。\n";
                return 1;
            }
            quality = std::stoi(std::string(args[i]));
            if (quality < 1 || quality > 100)
            {
                std::cerr << "错误: 质量值必须在 1~100 之间。\n";
                return 1;
            }
        }
        else
        {
            input_path = args[i];
        }
    }

    if (input_path.empty())
    {
        std::cerr << "错误: 未指定输入图片路径。\n";
        PrintUsage();
        return 1;
    }

    if(!output_prefix_set)
    {
        output_prefix = input_path.stem().string();
    }

    // 加载图片
    std::cout << std::format("正在加载: {} ...\n", input_path.string());
    ImageNamespace::Image img(input_path);

    if (!img.Valid())
    {
        std::cerr << std::format("错误: 无法加载图片: {}\n", input_path.string());
        return 1;
    }

    std::cout << std::format("图片尺寸: {}x{}\n", img.Width(), img.Height());
    std::cout << std::format("分割列数: {}\n", cols);

    // 执行分割
    auto results = BaitingMaster::Baiting(img, cols);

    if (results.empty())
    {
        std::cerr << "错误: 图片分割失败。\n";
        return 1;
    }

    std::cout << std::format("分割成功，共 {} 个子图。正在保存...\n", results.size());

    // 保存结果
    for (size_t i = 0; i < results.size(); ++i)
    {
        auto filename = std::format("{}_{}#{}.jpg", output_prefix, cols, i);
        results[i].Save(filename, quality);
        std::cout << std::format("  已保存: {}\n", filename);
    }

    std::cout << "完成!\n";
    return 0;
}
