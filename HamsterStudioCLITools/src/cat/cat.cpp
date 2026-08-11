/**
 * hamster-cat — Windows 版 cat 命令
 *
 * 模仿 GNU coreutils cat，支持常用选项。
 * 纯 C++20，无外部依赖（仅 Windows SDK + C++ 标准库）。
 *
 * 用法:
 *   hamster-cat [选项] [文件...]
 *
 * 选项:
 *   -A, --show-all           等价于 -vET
 *   -b, --number-nonblank    对非空行编号（覆盖 -n）
 *   -e                       等价于 -vE
 *   -E, --show-ends          在每行末尾显示 $
 *   -n, --number             对所有行编号
 *   -s, --squeeze-blank      将连续的空行压缩为一行
 *   -t                       等价于 -vT
 *   -T, --show-tabs          将制表符显示为 ^I
 *   -u                       （忽略，兼容 POSIX）
 *   -v, --show-nonprinting   使用 ^ 和 M- 表示法显示不可打印字符
 *   -h, --help               显示此帮助信息
 *   --version                显示版本信息
 *
 * 若无文件参数或文件为 "-"，则从标准输入读取。
 */

#include <Windows.h>

#include <algorithm>
#include <array>
#include <cstdio>
#include <filesystem>
#include <format>
#include <iostream>
#include <iterator>
#include <string>
#include <string_view>
#include <vector>

// ============================================================
// 配置 / 选项
// ============================================================
struct CatOptions
{
    bool show_all = false;       // -A
    bool number_nonblank = false; // -b
    bool show_ends = false;      // -E
    bool number_all = false;     // -n
    bool squeeze_blank = false;  // -s
    bool show_tabs = false;      // -T
    bool show_nonprinting = false; // -v

    bool show_help = false;
    bool show_version = false;

    // -b 会覆盖 -n
    void normalize()
    {
        if (number_nonblank)
            number_all = false;
        if (show_all)
        {
            show_nonprinting = true;
            show_ends = true;
            show_tabs = true;
        }
    }
};

// ============================================================
// 帮助与版本
// ============================================================
static void PrintUsage()
{
    std::cout << R"(用法: hamster-cat [选项] [文件...]

将文件连接到标准输出。

若无文件参数或文件为 "-"，则从标准输入读取。

选项:
  -A, --show-all           等价于 -vET
  -b, --number-nonblank    对非空行编号（覆盖 -n）
  -e                       等价于 -vE
  -E, --show-ends          在每行末尾显示 $
  -n, --number             对所有行编号
  -s, --squeeze-blank      将连续的空行压缩为一行
  -t                       等价于 -vT
  -T, --show-tabs          将制表符显示为 ^I
  -u                       （忽略，兼容 POSIX）
  -v, --show-nonprinting   使用 ^ 和 M- 表示法显示不可打印字符
  -h, --help               显示此帮助信息
      --version            显示版本信息

示例:
  hamster-cat file.txt                输出 file.txt 的内容
  hamster-cat -n file.txt             带行号输出
  hamster-cat file1.txt file2.txt     依次输出两个文件
  hamster-cat -s -n file.txt          压缩空行并编号
  echo hello | hamster-cat -n         从标准输入读取并编号
)";
}

static void PrintVersion()
{
    std::cout << "hamster-cat (HamsterStudioCLITools) 1.0.0\n"
              << "Windows 版 cat 命令，使用 C++20 编写。\n";
}

// ============================================================
// 命令行解析
// ============================================================
static CatOptions ParseArgs(int argc, wchar_t** argv,
                            std::vector<std::filesystem::path>& files)
{
    CatOptions opts;

    for (int i = 1; i < argc; ++i)
    {
        std::wstring_view arg(argv[i]);

        // 文件参数 / 标准输入
        if (!arg.starts_with(L"-"))
        {
            files.emplace_back(arg);
            continue;
        }

        if (arg == L"-")
        {
            files.emplace_back(L"-"); // 特殊标记: stdin
            continue;
        }

        // 长选项
        if (arg == L"--show-all")           { opts.show_all = true; continue; }
        if (arg == L"--number-nonblank")    { opts.number_nonblank = true; continue; }
        if (arg == L"--show-ends")          { opts.show_ends = true; continue; }
        if (arg == L"--number")             { opts.number_all = true; continue; }
        if (arg == L"--squeeze-blank")      { opts.squeeze_blank = true; continue; }
        if (arg == L"--show-tabs")          { opts.show_tabs = true; continue; }
        if (arg == L"--show-nonprinting")   { opts.show_nonprinting = true; continue; }
        if (arg == L"--help")               { opts.show_help = true; continue; }
        if (arg == L"--version")            { opts.show_version = true; continue; }

        // 组合短选项（-vET 等）
        std::wstring_view short_opts = arg.substr(1);
        for (size_t si = 0; si < short_opts.size(); ++si)
        {
            switch (short_opts[si])
            {
            case L'A': opts.show_all = true;           break;
            case L'b': opts.number_nonblank = true;    break;
            case L'e': opts.show_nonprinting = true;
                       opts.show_ends = true;           break;
            case L'E': opts.show_ends = true;           break;
            case L'n': opts.number_all = true;          break;
            case L's': opts.squeeze_blank = true;       break;
            case L't': opts.show_nonprinting = true;
                       opts.show_tabs = true;           break;
            case L'T': opts.show_tabs = true;           break;
            case L'u': /* 忽略 POSIX -u */             break;
            case L'v': opts.show_nonprinting = true;    break;
            case L'h': opts.show_help = true;           break;
            default:
                std::cerr << std::format("hamster-cat: 无效选项 -- '{}'\n",
                                         static_cast<char>(short_opts[si]));
                std::cerr << "请使用 'hamster-cat --help' 查看更多信息。\n";
                std::exit(1);
            }
        }
    }

    opts.normalize();
    return opts;
}

// ============================================================
// 输出处理状态
// ============================================================
struct OutputState
{
    unsigned long long line_number = 0;
    bool prev_line_was_blank = false;
    bool line_start = true; // 当前是否在行首
};

// ============================================================
// 不可打印字符转换（-v 选项核心）
// ============================================================
// 返回转换后的字符串：普通字符直接返回自身，控制字符转为 ^X，高位字节转为 M-X
static void WriteCharWithV(unsigned char ch, std::string& out)
{
    if (ch == '\t' || ch == '\n')
    {
        // Tab 和换行由 -T / -E 单独处理，-v 不做额外转换
        out.push_back(static_cast<char>(ch));
        return;
    }

    if (ch < 0x20)
    {
        // 控制字符: ^@ ^A ... ^_  (对应 0x00..0x1F)
        out.push_back('^');
        out.push_back(static_cast<char>(ch + 0x40)); // '@' == 0x40
    }
    else if (ch == 0x7F)
    {
        // DEL
        out.push_back('^');
        out.push_back('?');
    }
    else if (ch >= 0x80)
    {
        // meta 字符: M-X
        out.append("M-");
        unsigned char low = ch & 0x7F;
        if (low < 0x20 || low == 0x7F)
        {
            out.push_back('^');
            if (low == 0x7F)
                out.push_back('?');
            else
                out.push_back(static_cast<char>(low + 0x40));
        }
        else
        {
            out.push_back(static_cast<char>(low));
        }
    }
    else
    {
        out.push_back(static_cast<char>(ch));
    }
}

// ============================================================
// 核心: 处理单个字节并写入输出
// ============================================================
static void ProcessByte(unsigned char ch, const CatOptions& opts,
                        OutputState& state, std::string& line_buf,
                        std::string& out_buf)
{
    // -- 换行处理 --
    if (ch == '\n')
    {
        // -s squeeze: 连续空行压缩
        bool this_line_is_blank = (line_buf.empty());
        if (opts.squeeze_blank && this_line_is_blank && state.prev_line_was_blank)
        {
            // 跳过此行
            line_buf.clear();
            return;
        }
        state.prev_line_was_blank = this_line_is_blank;

        // 行号
        bool do_number = false;
        if (opts.number_all)
            do_number = true;
        else if (opts.number_nonblank && !this_line_is_blank)
            do_number = true;

        if (do_number)
        {
            ++state.line_number;
            out_buf += std::format("{:6}\t", state.line_number);
        }

        // 输出行内容
        out_buf += line_buf;

        // -E: 行尾标记
        if (opts.show_ends)
            out_buf.push_back('$');

        out_buf.push_back('\n');

        line_buf.clear();
        state.line_start = true;
        return;
    }

    // -- 非换行字符 --
    state.line_start = false;

    // -T: 制表符
    if (ch == '\t' && opts.show_tabs)
    {
        line_buf.append("^I");
        return;
    }

    // -v: 不可打印字符
    if (opts.show_nonprinting)
    {
        WriteCharWithV(ch, line_buf);
    }
    else
    {
        line_buf.push_back(static_cast<char>(ch));
    }
}

// ============================================================
// 处理输入流（文件或 stdin）
// ============================================================
static bool ProcessStream(std::FILE* fp, const CatOptions& opts,
                          OutputState& state)
{
    std::string line_buf;
    std::string out_buf;

    // 预分配缓冲区以减少重新分配
    line_buf.reserve(4096);
    out_buf.reserve(65536);

    int c;
    while ((c = std::fgetc(fp)) != EOF)
    {
        ProcessByte(static_cast<unsigned char>(c), opts, state,
                    line_buf, out_buf);

        // 定期刷新大缓冲区
        if (out_buf.size() >= 65536)
        {
            std::fwrite(out_buf.data(), 1, out_buf.size(), stdout);
            out_buf.clear();
        }
    }

    // 刷新剩余缓冲
    if (!out_buf.empty())
    {
        std::fwrite(out_buf.data(), 1, out_buf.size(), stdout);
    }

    // 文件末尾如果没有以换行结尾，仍需要输出缓冲内容
    if (!line_buf.empty())
    {
        bool do_number = false;
        if (opts.number_all)
            do_number = true;
        else if (opts.number_nonblank)
            do_number = true;

        if (do_number)
        {
            ++state.line_number;
            std::fprintf(stdout, "%6llu\t", state.line_number);
        }
        std::fwrite(line_buf.data(), 1, line_buf.size(), stdout);
        // 末尾没有换行，不添加 -E 的 $
    }

    return true;
}

// ============================================================
// 入口
// ============================================================
int wmain(int argc, wchar_t** argv)
{
    // 控制台 UTF-8 输出支持
    SetConsoleOutputCP(CP_UTF8);
    SetConsoleCP(CP_UTF8);

    // 解析参数
    std::vector<std::filesystem::path> files;
    CatOptions opts = ParseArgs(argc, argv, files);

    if (opts.show_help)
    {
        PrintUsage();
        return 0;
    }
    if (opts.show_version)
    {
        PrintVersion();
        return 0;
    }

    // 无文件参数 → 读标准输入
    if (files.empty())
    {
        files.emplace_back(L"-");
    }

    OutputState state;
    bool ok = true;

    for (const auto& file : files)
    {
        if (file == L"-")
        {
            ok &= ProcessStream(stdin, opts, state);
        }
        else
        {
            // 使用 _wfopen 以便正确处理 Unicode 路径
            std::FILE* fp = nullptr;
            errno_t err = _wfopen_s(&fp, file.c_str(), L"rb");
            if (err != 0 || fp == nullptr)
            {
                std::cerr << std::format("hamster-cat: {}: {}\n",
                                         file.string(),
                                         std::generic_category().message(
                                             static_cast<int>(err)));
                ok = false;
                continue;
            }
            ok &= ProcessStream(fp, opts, state);
            std::fclose(fp);
        }
    }

    return ok ? 0 : 1;
}
