// HamsterStudioToes 单元测试
//
// 覆盖范围：算法、WAV 解析、字符串、诊断、PE 解析、图像处理、ROI 粘贴、拼接。
// 全部通过返回 0，任何一项失败返回 1。
//
// 依赖：OpenCV（路径见工程文件），运行时需要 opencv_world4120.dll 在 PATH 上。
#include <cstdint>
#include <cstdio>
#include <exception>
#include <filesystem>
#include <fstream>
#include <string>
#include <system_error>
#include <utility>
#include <vector>

#define NOMINMAX
#include <Windows.h>

#include <opencv2/opencv.hpp>

#include "legacy/algorithm/Math.h"
#include "legacy/algorithm/Search.hpp"
#include "legacy/algorithm/Sort.h"
#include "imaging/adjustments.h"
#include <hamster/toes/stitch.h>
#include <hamster/toes/layout.h>
#include "stitch/paste.h"
#include "legacy/media/Wave.h"
#include "core/diagnostics.h"
#include "win32/pe.h"
#include "win32/os.h"
#include "legacy/nstd/string.h"

// ImageStitcher.cpp 内部定义了按 cv::Mat 指针向量拼接的重载，但 ImageStitcher.h
// 只声明了 void* 版本，外部无法直接调用。测试里显式声明一下；把它们正式暴露到
// 头文件属于后续 API 清理，不在本次范围内。
cv::Mat stitch(std::vector<cv::Mat const *> const &images, toes::image::StretchMode mode,
               HamsterStudioToes::Image::ImageShape target,
               HamsterStudioToes::ImageTools::ImageLayout const &layout, int borderThickness);

namespace
{
    int g_checks = 0;
    int g_failures = 0;

    void Check(bool condition, char const *what)
    {
        ++g_checks;
        if (!condition) {
            ++g_failures;
            std::printf("FAIL  %s\n", what);
        }
        else {
            std::printf("ok    %s\n", what);
        }
    }

    void Section(char const *name)
    {
        std::printf("\n--- %s\n", name);
    }

    // ---------------------------------------------------------------- 工具

    void Put32(std::ofstream &o, uint32_t v)
    {
        char b[4] = { static_cast<char>(v), static_cast<char>(v >> 8),
                      static_cast<char>(v >> 16), static_cast<char>(v >> 24) };
        o.write(b, 4);
    }

    void Put16(std::ofstream &o, uint16_t v)
    {
        char b[2] = { static_cast<char>(v), static_cast<char>(v >> 8) };
        o.write(b, 2);
    }

    void Put4(std::ofstream &o, char const *s) { o.write(s, 4); }

    // ---------------------------------------------------------------- 算法

    void TestSort()
    {
        Section("Algorithm/Sort");

        int a[] = { 5, -3, 9, 5, 0, 12, -7 };
        HamsterStudioToes::Sort::Buble(a, static_cast<int>(std::size(a)));

        bool sorted = true;
        for (size_t i = 1; i < std::size(a); ++i) {
            if (a[i - 1] > a[i]) sorted = false;
        }
        Check(sorted, "Buble: 升序（含重复值与负数）");
        Check(a[0] == -7 && a[std::size(a) - 1] == 12, "Buble: 首尾元素正确");

        int one[] = { 42 };
        HamsterStudioToes::Sort::Buble(one, 1);
        Check(one[0] == 42, "Buble: len=1 不越界");

        HamsterStudioToes::Sort::Buble(nullptr, 0);
        Check(true, "Buble: len=0 不越界");
    }

    void TestBinarySearch()
    {
        Section("Algorithm/Search");

        HamsterStudioToes::Search::BinarySearch bs;
        int a[] = { -7, -3, 0, 5, 5, 9, 12 };
        int const n = static_cast<int>(std::size(a));

        bool all = true;
        for (int i = 0; i < n; ++i) {
            auto const p = bs(a[i], a, n);
            if (p == HamsterStudioToes::Search::NPos || a[p] != a[i]) all = false;
        }
        Check(all, "BinarySearch: 每个元素都能命中");

        int two[] = { 1, 2 };
        Check(bs(2, two, 2) == 1, "BinarySearch: {1,2} 找 2 能返回（曾死循环）");
        Check(bs(1, two, 2) == 0, "BinarySearch: {1,2} 找 1 能返回");
        Check(bs(100, two, 2) == HamsterStudioToes::Search::NPos, "BinarySearch: 缺失值返回 NPos");
    }

    void TestScaleConversion()
    {
        Section("Algorithm/Math");

        HamsterStudioToes::Math::ScaleConversion hex(16);
        Check(!hex(255).empty(), "ScaleConversion(16): 非空");
        Check(HamsterStudioToes::Math::ScaleConversion(1)(10).empty(), "ScaleConversion(1): 不死循环");
        Check(HamsterStudioToes::Math::ScaleConversion(0)(10).empty(), "ScaleConversion(0): 不除零");
    }

    // ---------------------------------------------------------------- WAV

    void TestWave()
    {
        Section("Media/Wave");

        char const *path = "toes_test.wav";
        {
            std::ofstream o(path, std::ios::binary);
            Put4(o, "RIFF");
            Put32(o, 4 + (8 + 16) + (8 + 8) + (8 + 4));
            Put4(o, "WAVE");

            Put4(o, "fmt ");
            Put32(o, 16);
            Put16(o, 1);      // PCM
            Put16(o, 1);      // 单声道
            Put32(o, 8000);   // 采样率
            Put32(o, 8000);   // 字节率
            Put16(o, 1);      // 块对齐
            Put16(o, 8);      // 位深

            // 首字节非 'd'、第二字节为 'a' 的块：旧的 && 判别会把它当成 data
            Put4(o, "fact");
            Put32(o, 8);
            o.write("ABCDEFGH", 8);

            Put4(o, "data");
            Put32(o, 4);
            char const payload[4] = { 0x12, 0x34, 0x56, 0x78 };
            o.write(payload, 4);
        }

        {
            HamsterStudioToes::Media::WaveFormat wave(path);
            Check(wave.GetBytePerSecond() == 8000, "Wave: 跳过 fact 块后读对 bytes_per_sec");
            Check(wave.Read16Bit() == 0x3412u, "Wave: 第一个 16 位字 = 0x3412（曾返回 1）");
            Check(wave.Read16Bit() == 0x7856u, "Wave: 第二个 16 位字 = 0x7856");
            Check(wave.EndOfWave(), "Wave: 读完后 EndOfWave()");
            Check(wave.Read8Bit() == 0, "Wave: 越界读返回 0");
        }

        std::error_code ec;
        std::filesystem::remove(path, ec);
    }

    // ---------------------------------------------------------------- 字符串

    void TestString()
    {
        Section("nstd/containers/string");

        toes::nstd::String a("Hello");
        toes::nstd::String b(a);
        Check(std::string(b.Data()) == "Hello", "String: 拷贝构造内容正确");

        toes::nstd::String c("World");
        c = a;
        Check(std::string(c.Data()) == "Hello", "String: 拷贝赋值内容正确");

        toes::nstd::String d(std::move(b));
        Check(std::string(d.Data()) == "Hello", "String: 移动构造内容正确");
        Check(b.Data() == nullptr, "String: 被移动后置空");

        toes::nstd::String e("AbC");
        e.ToLower();
        Check(std::string(e.Data()) == "abc", "String: ToLower 变小写（曾反向）");
        e.ToUpper();
        Check(std::string(e.Data()) == "ABC", "String: ToUpper 变大写");
    }

    // ---------------------------------------------------------------- 诊断

    void TestDiagnostics()
    {
        Section("Tools/Diagnostics");

        ::SetLastError(ERROR_FILE_NOT_FOUND);
        bool threw = false;
        int code = 0;
        try {
            HamsterStudioToes::Diagnostics::ThrowLastError();
        }
        catch (std::system_error const &ex) {
            threw = true;
            code = ex.code().value();
        }
        Check(threw, "ThrowLastError: 抛 std::system_error 而不是终止进程");
        Check(code == ERROR_FILE_NOT_FOUND, "ThrowLastError: 携带真实 Win32 错误码");

        ::SetLastError(ERROR_NO_MORE_FILES);
        threw = false;
        try {
            HamsterStudioToes::Diagnostics::ThrowLastError({ ERROR_NO_MORE_FILES });
        }
        catch (...) {
            threw = true;
        }
        Check(!threw, "ThrowLastError: 容忍列表中的错误码不抛");

        char const *self = "Tools/Diagnostics.cpp";
        if (std::filesystem::exists(self)) {
            std::vector<std::filesystem::path> links;
            threw = false;
            try {
                HamsterStudioToes::Win32Utils::EnumHardLinkFiles(self, [&](std::filesystem::path const &p) {
                    links.push_back(p);
                    return true;
                });
            }
            catch (std::system_error const &) {
                threw = true;
            }
            Check(!threw, "EnumHardLinkFiles: 正常返回（曾 ExitProcess）");
            Check(!links.empty(), "EnumHardLinkFiles: 至少回调一次");
        }
    }

    // ---------------------------------------------------------------- PE

    void TestPortableExecutable()
    {
        Section("Windows/Platform");

        char const *kKernel32 = "C:\\Windows\\System32\\kernel32.dll";
        if (!std::filesystem::exists(kKernel32)) {
            std::printf("skip  找不到 %s\n", kKernel32);
            return;
        }

        HamsterStudioToes::PortableExecutable pe(kKernel32);
        Check(pe.GetTimeDateStamp() != 0, "PE: GetTimeDateStamp 非 0");
        Check(pe.GetFoa(0) == 0, "PE: 头部区域的 RVA 原样返回");

        auto const table = pe.GetExportTable();
        Check(table != nullptr, "PE: 导出表非空");
        Check(table->Entries.size() > 100, "PE: 导出项数量合理");

        auto const *entry = table->Find("CreateFileW");
        Check(entry != nullptr, "PE: 能查到 CreateFileW");
        if (entry != nullptr) {
            auto const foa = pe.GetFoa(entry->Rva);
            Check(foa > 0 && foa < std::filesystem::file_size(kKernel32), "PE: RVA 换算成合法文件偏移");
        }

        bool threw = false;
        try {
            pe.GetFoa(0xFFFFFF00ul);
        }
        catch (std::out_of_range const &) {
            threw = true;
        }
        Check(threw, "PE: 不属于任何节的 RVA 抛 out_of_range");

        char const *kWow64 = "C:\\Windows\\SysWOW64\\kernel32.dll";
        if (std::filesystem::exists(kWow64)) {
            threw = false;
            try {
                HamsterStudioToes::PortableExecutable wrongBitness(kWow64);
            }
            catch (std::runtime_error const &) {
                threw = true;
            }
            Check(threw, "PE: 32 位映像因位宽不符被拒绝");
        }
    }

    // ---------------------------------------------------------------- ImageProcessor

    void TestImageProcessor()
    {
        Section("Image/ImageProcessor");

        using namespace HamsterStudioToes::ImageProcessor;

        constexpr int W = 32;
        constexpr int H = 24;

        auto makeBGRA = []() {
            std::vector<uint8_t> px(static_cast<size_t>(W) * H * 4);
            for (size_t i = 0; i < px.size(); i += 4) {
                px[i + 0] = 10;
                px[i + 1] = 20;
                px[i + 2] = 30;
                px[i + 3] = 200;
            }
            return px;
        };

        auto processor = CreateImageProcessor();

        // 外部缓冲区必须被真正写回
        {
            auto px = makeBGRA();
            ImageData data(px.data(), W, H, W * 4);
            ImageAdjustments adj;
            adj.exposure = 1.0;   // x2

            Check(processor->processImage(data, adj), "ImageProcessor: processImage 返回 true");
            Check(px[0] == 20, "ImageProcessor: 写回了调用方缓冲区（B 10->20）");
            Check(px[1] == 40 && px[2] == 60, "ImageProcessor: G/R 同样缩放");
            Check(px[3] == 200, "ImageProcessor: alpha 保持不变");
        }

        // LUT 不能跨调用缓存
        {
            auto a = makeBGRA();
            auto b = makeBGRA();
            ImageData da(a.data(), W, H, W * 4);
            ImageData db(b.data(), W, H, W * 4);

            ImageAdjustments e1; e1.exposure = 1.0;   // x2
            ImageAdjustments e2; e2.exposure = 2.0;   // x4

            auto p = CreateImageProcessor();
            p->processImage(da, e1);
            p->processImage(db, e2);
            Check(b[0] == 40, "ImageProcessor: 第二次用的是 x4 的 LUT");
        }

        // 多线程与单线程结果一致，且不改全局线程数
        {
            auto a = makeBGRA();
            auto b = makeBGRA();
            ImageData da(a.data(), W, H, W * 4);
            ImageData db(b.data(), W, H, W * 4);

            ImageAdjustments adj;
            adj.exposure = 0.5;
            adj.saturation = 30.0;
            adj.highlights = 40.0;

            int const before = cv::getNumThreads();
            auto p1 = CreateImageProcessor();
            auto p2 = CreateImageProcessor();
            p1->processImage(da, adj);
            p2->processImageMT(db, adj, 2);

            Check(a == b, "ImageProcessor: MT 与单线程结果一致");
            Check(cv::getNumThreads() == before, "ImageProcessor: 未修改全局 OpenCV 线程数");
        }

        // 直方图不再是全 0
        {
            auto px = makeBGRA();
            ImageData data(px.data(), W, H, W * 4);
            auto const hist = processor->calculateHistogram(data);

            int64_t blue = 0;
            for (int i = 0; i < 256; ++i) {
                blue += hist.blue[i];
            }
            Check(blue == static_cast<int64_t>(W) * H, "ImageProcessor: 直方图总数等于像素数（曾恒为 0）");
            Check(hist.blue[10] == W * H, "ImageProcessor: 蓝通道峰值落在 10");
        }
    }

    // ---------------------------------------------------------------- 粘贴

    void TestPaste()
    {
        Section("Image/Tools/PasteImageUtils");

        using namespace toes::image;

        cv::Scalar const src{ 10, 20, 30 };
        auto const canvas80 = []() { return cv::Mat(80, 100, CV_8UC3, cv::Scalar(0, 0, 0)); };

        auto pixelIs = [](cv::Mat const &m, int x, int y, cv::Scalar const &c) {
            auto const p = m.at<cv::Vec3b>(y, x);
            return p[0] == static_cast<uchar>(c[0])
                && p[1] == static_cast<uchar>(c[1])
                && p[2] == static_cast<uchar>(c[2]);
        };

        {
            cv::Mat canvas = canvas80();
            cv::Mat const img(50, 50, CV_8UC3, src);
            ImagePaster::Paste(&canvas, &img, StretchMode::Uniform);
            Check(pixelIs(canvas, 50, 40, src), "Paste: Uniform 居中贴在画布上");
            Check(!pixelIs(canvas, 2, 2, src), "Paste: Uniform 四角保留底色");
        }
        {
            cv::Mat canvas = canvas80();
            cv::Mat const img(50, 50, CV_8UC3, src);
            ImagePaster::Paste(&canvas, &img, StretchMode::UniformToFill);
            Check(pixelIs(canvas, 0, 0, src) && pixelIs(canvas, 99, 79, src), "Paste: UniformToFill 铺满");
        }
        {
            cv::Mat canvas = canvas80();
            cv::Mat const img(50, 50, CV_8UC3, src);
            ImagePaster::Paste(&canvas, &img, StretchMode::None);
            Check(pixelIs(canvas, 50, 40, src), "Paste: None 居中不缩放");
        }
        {
            // 灰度贴彩色：旧实现 cvtColor 直接写 ROI，结果整体丢失
            cv::Mat canvas = canvas80();
            cv::Mat const gray(50, 50, CV_8UC1, cv::Scalar(128));
            ImagePaster::Paste(&canvas, &gray, StretchMode::Fill);
            auto const p = canvas.at<cv::Vec3b>(40, 50);
            Check(p[0] == 128 && p[1] == 128 && p[2] == 128, "Paste: 灰度图贴进彩色画布生效");
        }
        {
            cv::Mat canvas = canvas80();
            cv::Mat empty;
            ImagePaster::Paste(&canvas, &empty, StretchMode::Fill);
            Check(true, "Paste: 空图片不崩溃");
        }
    }

    // ---------------------------------------------------------------- 拼接

    void TestStitch()
    {
        Section("Image/ImageStitcher");

        using ImageShape = HamsterStudioToes::Image::ImageShape;
        using ImageLayout = HamsterStudioToes::ImageTools::ImageLayout;

        constexpr int kCell = 100;
        constexpr int kBorder = 8;

        cv::Mat m0(kCell, kCell, CV_8UC3, cv::Scalar(10, 0, 0));
        cv::Mat m1(kCell, kCell, CV_8UC3, cv::Scalar(0, 20, 0));
        cv::Mat m2(kCell, kCell, CV_8UC3, cv::Scalar(0, 0, 30));
        cv::Mat m3(kCell, kCell, CV_8UC3, cv::Scalar(40, 40, 40));
        std::vector<cv::Mat const *> four{ &m0, &m1, &m2, &m3 };

        ImageShape const target{ kCell, kCell, 3 };
        ImageLayout const layout{ 2, 2 };

        cv::Mat const canvas = stitch(four, toes::image::StretchMode::Fill, target, layout, kBorder);

        int const expectW = layout.cols * kCell + (layout.cols + 1) * kBorder;
        int const expectH = layout.rows * kCell + (layout.rows + 1) * kBorder;
        Check(canvas.cols == expectW && canvas.rows == expectH, "Stitch: 画布尺寸正确");

        auto center = [&](int col, int row) {
            int const x = kBorder + col * (kCell + kBorder) + kCell / 2;
            int const y = kBorder + row * (kCell + kBorder) + kCell / 2;
            return canvas.at<cv::Vec3b>(y, x);
        };
        Check(center(0, 0) == cv::Vec3b(10, 0, 0), "Stitch: 左上格子 = 第 0 张");
        Check(center(1, 0) == cv::Vec3b(0, 20, 0), "Stitch: 右上格子 = 第 1 张");
        Check(center(1, 1) == cv::Vec3b(40, 40, 40), "Stitch: 右下格子 = 第 3 张");

        // 图片多于格子时忽略多余项
        cv::Mat m4(kCell, kCell, CV_8UC3, cv::Scalar(99, 99, 99));
        std::vector<cv::Mat const *> five{ &m0, &m1, &m2, &m3, &m4 };
        cv::Mat const overflow = stitch(five, toes::image::StretchMode::Fill, target, layout, kBorder);
        Check(overflow.cols == expectW, "Stitch: 图片多于格子时忽略多余项");

        // 空输入
        std::vector<cv::Mat const *> none;
        cv::Mat const empty = stitch(none, toes::image::StretchMode::Fill, target, layout, kBorder);
        Check(empty.empty(), "Stitch: 空输入返回空 Mat");
    }
}

int main()
{
    std::printf("HamsterStudioToes tests\n");

    try {
        TestSort();
        TestBinarySearch();
        TestScaleConversion();
        TestWave();
        TestString();
        TestDiagnostics();
        TestPortableExecutable();
        TestImageProcessor();
        TestPaste();
        TestStitch();
    }
    catch (std::exception const &ex) {
        std::printf("\nUNCAUGHT EXCEPTION: %s\n", ex.what());
        return 1;
    }

    std::printf("\n%d checks, %d failure(s)\n", g_checks, g_failures);
    return g_failures == 0 ? 0 : 1;
}
