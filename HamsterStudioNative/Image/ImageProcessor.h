#pragma once

#include <cstdint>
#include <vector>
#include <memory>

#ifdef IMAGEPROCESSOR_EXPORTS
#define IMAGEPROCESSOR_API __declspec(dllexport)
#else
#define IMAGEPROCESSOR_API __declspec(dllimport)
#endif

namespace ImageProcessor
{

    // 图像调整参数结构（与C#对齐）
    struct ImageAdjustments {
        double exposure = 0.0;
        double temperature = 0.0;
        double tint = 0.0;
        double highlights = 0.0;
        double shadows = 0.0;
        double whites = 0.0;
        double blacks = 0.0;
        double saturation = 0.0;
        double vibrance = 0.0;
    };

    // 图像数据包装
    struct ImageData {
        uint8_t *pixels;          // BGRA格式数据
        int32_t width;
        int32_t height;
        int32_t stride;           // 每行字节数

        ImageData() : pixels(nullptr), width(0), height(0), stride(0) { }
        ImageData(uint8_t *p, int32_t w, int32_t h, int32_t s)
            : pixels(p), width(w), height(h), stride(s) { }
    };

    // 直方图数据
    struct HistogramData {
        int32_t red[256] = { 0 };
        int32_t green[256] = { 0 };
        int32_t blue[256] = { 0 };
        int32_t luminance[256] = { 0 };

        void clear() {
            std::fill_n(red, 256, 0);
            std::fill_n(green, 256, 0);
            std::fill_n(blue, 256, 0);
            std::fill_n(luminance, 256, 0);
        }
    };

    // 核心图像处理器类
    class IMAGEPROCESSOR_API ImageProcessor {
    public:
        ImageProcessor();
        ~ImageProcessor();

        // 主处理函数
        bool processImage(ImageData &image, const ImageAdjustments &adjustments);

        // 计算直方图
        HistogramData calculateHistogram(const ImageData &image);

        // 批量处理（支持多线程）
        bool processImageMT(ImageData &image, const ImageAdjustments &adjustments, int threadCount = 0);

        // 创建查找表（用于曝光、饱和度等）
        void createLookupTables(const ImageAdjustments &adjustments);

        // 性能统计
        struct PerformanceStats {
            double processingTimeMs = 0.0;
            double pixelsPerSecond = 0.0;
            int64_t pixelsProcessed = 0;
        };

        PerformanceStats getStats() const { return stats_; }

    private:
        // 内部处理函数
        void processPixelBGRABatch(uint8_t *pixels, size_t count, const ImageAdjustments &adj);
        void processPixelBGRASingle(uint8_t *pixel, const ImageAdjustments &adj);

        // 颜色转换辅助函数
        static void rgbToHSV(float r, float g, float b, float &h, float &s, float &v);
        static void hsvToRGB(float h, float s, float v, float &r, float &g, float &b);
        static float getLuminance(float r, float g, float b);
        static float lerp(float a, float b, float t);

        // 色调调整
        static void applyToneAdjustments(const ImageAdjustments &adj, float &r, float &g, float &b);

        // 查找表
        float exposureLUT_[256] = { 0 };
        float saturationLUT_[256] = { 0 };
        float temperatureLUT_[256] = { 0 };
        float tintLUT_[256] = { 0 };

        // 性能统计
        PerformanceStats stats_;

        // 预计算标志
        bool tablesUpdated_ = false;

        // 禁用拷贝
        ImageProcessor(const ImageProcessor &) = delete;
        ImageProcessor &operator=(const ImageProcessor &) = delete;
    };

    // 工厂函数
    IMAGEPROCESSOR_API std::unique_ptr<ImageProcessor> createImageProcessor();

    // C风格接口（便于P/Invoke调用）
    extern "C" {
        IMAGEPROCESSOR_API void *image_processor_create();
        IMAGEPROCESSOR_API void image_processor_destroy(void *processor);
        IMAGEPROCESSOR_API bool image_processor_process(
            void *processor,
            uint8_t *pixels,
            int32_t width,
            int32_t height,
            int32_t stride,
            const ImageAdjustments *adjustments);

        IMAGEPROCESSOR_API bool image_processor_get_histogram(
            void *processor,
            const uint8_t *pixels,
            int32_t width,
            int32_t height,
            int32_t stride,
            int32_t *redHist,
            int32_t *greenHist,
            int32_t *blueHist,
            int32_t *luminanceHist);
    }

} // namespace ImageProcessor