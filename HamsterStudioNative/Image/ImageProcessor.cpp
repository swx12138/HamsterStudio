#define IMAGEPROCESSOR_EXPORTS

#include "ImageProcessor.h"

#include <algorithm>
#include <cmath>
#include <chrono>
#include <thread>
#include <vector>

#include <immintrin.h> // SIMD指令
#include <ppl.h>       // 并行模式库

namespace ImageProcessor
{
    // 常量定义
    constexpr float INV_255 = 1.0f / 255.0f;
    constexpr float MID_GRAY = 0.18f;

    ImageProcessor::ImageProcessor() {
        // 初始化查找表为恒等变换
        for (int i = 0; i < 256; ++i) {
            exposureLUT_[i] = i * INV_255;
            saturationLUT_[i] = i * INV_255;
            temperatureLUT_[i] = i * INV_255;
            tintLUT_[i] = i * INV_255;
        }
    }

    ImageProcessor::~ImageProcessor() = default;

    void ImageProcessor::createLookupTables(const ImageAdjustments &adjustments) {
        // 曝光查找表
        float exposureFactor = static_cast<float>(std::pow(2.0, adjustments.exposure));
        for (int i = 0; i < 256; ++i) {
            float value = i * INV_255;
            exposureLUT_[i] = std::clamp(value * exposureFactor, 0.0f, 1.0f);
        }

        // 饱和度查找表（简化版，实际应基于HSV）
        float saturationFactor = static_cast<float>(1.0 + adjustments.saturation / 100.0);
        for (int i = 0; i < 256; ++i) {
            float value = i * INV_255;
            saturationLUT_[i] = std::clamp(value * saturationFactor, 0.0f, 1.0f);
        }

        tablesUpdated_ = true;
    }

    bool ImageProcessor::processImage(ImageData &image, const ImageAdjustments &adjustments) {
        if (!image.pixels || image.width <= 0 || image.height <= 0) {
            return false;
        }

        auto startTime = std::chrono::high_resolution_clock::now();

        // 更新查找表
        createLookupTables(adjustments);

        // 单线程处理
        const int64_t totalPixels = static_cast<int64_t>(image.width) * image.height;
        uint8_t *pixelPtr = image.pixels;

        for (int64_t i = 0; i < totalPixels; ++i) {
            processPixelBGRASingle(pixelPtr, adjustments);
            pixelPtr += 4;
        }

        auto endTime = std::chrono::high_resolution_clock::now();
        std::chrono::duration<double, std::milli> duration = endTime - startTime;

        // 更新统计
        stats_.processingTimeMs = duration.count();
        stats_.pixelsProcessed = totalPixels;
        stats_.pixelsPerSecond = totalPixels / (duration.count() / 1000.0);

        return true;
    }

    bool ImageProcessor::processImageMT(ImageData &image, const ImageAdjustments &adjustments, int threadCount) {
        if (!image.pixels || image.width <= 0 || image.height <= 0) {
            return false;
        }

        auto startTime = std::chrono::high_resolution_clock::now();

        // 更新查找表
        createLookupTables(adjustments);

        const int64_t totalPixels = static_cast<int64_t>(image.width) * image.height;
        const int64_t chunkSize = 1024; // 每个任务处理1024个像素

        // 使用Parallel Patterns Library进行并行处理
        concurrency::parallel_for(int64_t(0), totalPixels, chunkSize,
            [&] (int64_t start) {
                int64_t end = std::min(start + chunkSize, totalPixels);
                uint8_t *pixelPtr = image.pixels + start * 4;

                for (int64_t i = start; i < end; ++i) {
                    processPixelBGRASingle(pixelPtr, adjustments);
                    pixelPtr += 4;
                }
            });

        auto endTime = std::chrono::high_resolution_clock::now();
        std::chrono::duration<double, std::milli> duration = endTime - startTime;

        // 更新统计
        stats_.processingTimeMs = duration.count();
        stats_.pixelsProcessed = totalPixels;
        stats_.pixelsPerSecond = totalPixels / (duration.count() / 1000.0);

        return true;
    }

    // SIMD优化版本（可选）
#ifdef __AVX2__
    void ImageProcessor::processPixelBGRABatch(uint8_t *pixels, size_t count, const ImageAdjustments &adj) {
        // AVX2 SIMD实现（处理8个像素一次）
        const __m256 inv255 = _mm256_set1_ps(INV_255);
        const __m256 exposureVec = _mm256_set1_ps(static_cast<float>(std::pow(2.0, adj.exposure)));

        for (size_t i = 0; i < count; i += 8) {
            // 加载8个像素的BGRA数据
            __m256i pixelData = _mm256_loadu_si256(reinterpret_cast<const __m256i *>(pixels + i * 4));

            // 提取并转换到浮点数
            __m256 b = _mm256_cvtepi32_ps(_mm256_cvtepu8_epi32(_mm256_castsi256_si128(pixelData)));
            __m256 g = _mm256_cvtepi32_ps(_mm256_cvtepu8_epi32(_mm256_castsi256_si128(_mm256_bsrli_epi128(pixelData, 1))));
            __m256 r = _mm256_cvtepi32_ps(_mm256_cvtepu8_epi32(_mm256_castsi256_si128(_mm256_bsrli_epi128(pixelData, 2))));

            // 归一化到[0,1]
            b = _mm256_mul_ps(b, inv255);
            g = _mm256_mul_ps(g, inv255);
            r = _mm256_mul_ps(r, inv255);

            // 应用曝光
            if (adj.exposure != 0) {
                r = _mm256_mul_ps(r, exposureVec);
                g = _mm256_mul_ps(g, exposureVec);
                b = _mm256_mul_ps(b, exposureVec);
            }

            // 钳制到[0,1]
            const __m256 zero = _mm256_setzero_ps();
            const __m256 one = _mm256_set1_ps(1.0f);
            r = _mm256_min_ps(_mm256_max_ps(r, zero), one);
            g = _mm256_min_ps(_mm256_max_ps(g, zero), one);
            b = _mm256_min_ps(_mm256_max_ps(b, zero), one);

            // 转换回整数并存储
            __m256i ri = _mm256_cvtps_epi32(_mm256_mul_ps(r, _mm256_set1_ps(255.0f)));
            __m256i gi = _mm256_cvtps_epi32(_mm256_mul_ps(g, _mm256_set1_ps(255.0f)));
            __m256i bi = _mm256_cvtps_epi32(_mm256_mul_ps(b, _mm256_set1_ps(255.0f)));

            // 重新打包并存储
            // ... (省略详细打包代码)
        }
    }
#endif

    void ImageProcessor::processPixelBGRASingle(uint8_t *pixel, const ImageAdjustments &adj) {
        // 转换为浮点数
        float b = pixel[0] * INV_255;
        float g = pixel[1] * INV_255;
        float r = pixel[2] * INV_255;

        // 应用曝光度
        if (adj.exposure != 0) {
            float exposure = static_cast<float>(std::pow(2.0, adj.exposure));
            r *= exposure;
            g *= exposure;
            b *= exposure;
        }

        // 转换为HSV进行饱和度调整
        float h, s, v;
        rgbToHSV(r, g, b, h, s, v);

        // 应用饱和度
        if (adj.saturation != 0) {
            float saturationAdjust = static_cast<float>(adj.saturation / 100.0);
            s = std::clamp(s + saturationAdjust, 0.0f, 1.0f);
        }

        // 应用鲜艳度
        if (adj.vibrance != 0) {
            float vibrance = static_cast<float>(adj.vibrance / 100.0);
            float avg = (r + g + b) / 3.0f;
            float maxDiff = std::max(std::abs(r - avg),
                std::max(std::abs(g - avg),
                std::abs(b - avg)));

            if (maxDiff < 0.1f) {
                float saturationBoost = vibrance * 0.5f;
                s = std::clamp(s + saturationBoost, 0.0f, 1.0f);
            }
        }

        // 转换回RGB
        hsvToRGB(h, s, v, r, g, b);

        // 应用色温和色调
        if (adj.temperature != 0 || adj.tint != 0) {
            float temp = static_cast<float>(adj.temperature / 100.0);
            float tint = static_cast<float>(adj.tint / 100.0);

            // 色温调整
            b = std::clamp(b - temp * 0.5f, 0.0f, 1.0f);
            r = std::clamp(r + temp * 0.25f, 0.0f, 1.0f);

            // 色调调整
            g = std::clamp(g - tint * 0.5f, 0.0f, 1.0f);
            r = std::clamp(r + tint * 0.25f, 0.0f, 1.0f);
        }

        // 应用色调调整（高光、阴影等）
        applyToneAdjustments(adj, r, g, b);

        // 钳制并转换回整数
        pixel[0] = static_cast<uint8_t>(std::clamp(b, 0.0f, 1.0f) * 255);
        pixel[1] = static_cast<uint8_t>(std::clamp(g, 0.0f, 1.0f) * 255);
        pixel[2] = static_cast<uint8_t>(std::clamp(r, 0.0f, 1.0f) * 255);
        // Alpha通道保持不变
    }

    void ImageProcessor::rgbToHSV(float r, float g, float b, float &h, float &s, float &v) {
        float min = std::min(std::min(r, g), b);
        float max = std::max(std::max(r, g), b);
        float delta = max - min;

        v = max;
        s = (max != 0.0f) ? delta / max : 0.0f;

        if (delta == 0.0f) {
            h = 0.0f;
        }
        else {
            if (max == r)
                h = (g - b) / delta + (g < b ? 6.0f : 0.0f);
            else if (max == g)
                h = (b - r) / delta + 2.0f;
            else
                h = (r - g) / delta + 4.0f;

            h /= 6.0f;
        }
    }

    void ImageProcessor::hsvToRGB(float h, float s, float v, float &r, float &g, float &b) {
        if (s == 0.0f) {
            r = g = b = v;
            return;
        }

        int i = static_cast<int>(h * 6.0f);
        float f = h * 6.0f - i;
        float p = v * (1.0f - s);
        float q = v * (1.0f - f * s);
        float t = v * (1.0f - (1.0f - f) * s);

        switch (i % 6) {
        case 0: r = v; g = t; b = p; break;
        case 1: r = q; g = v; b = p; break;
        case 2: r = p; g = v; b = t; break;
        case 3: r = p; g = q; b = v; break;
        case 4: r = t; g = p; b = v; break;
        default: r = v; g = p; b = q; break;
        }
    }

    float ImageProcessor::getLuminance(float r, float g, float b) {
        return 0.299f * r + 0.587f * g + 0.114f * b;
    }

    float ImageProcessor::lerp(float a, float b, float t) {
        return a + (b - a) * t;
    }

    void ImageProcessor::applyToneAdjustments(const ImageAdjustments &adj, float &r, float &g, float &b) {
        // 高光调整
        if (adj.highlights > 0) {
            float highlightFactor = static_cast<float>(adj.highlights / 100.0);
            float luminance = getLuminance(r, g, b);

            if (luminance > 0.7f) {
                float blend = (luminance - 0.7f) / 0.3f;
                r = lerp(r, 1.0f, blend * highlightFactor);
                g = lerp(g, 1.0f, blend * highlightFactor);
                b = lerp(b, 1.0f, blend * highlightFactor);
            }
        }
        else if (adj.highlights < 0) {
            float highlightFactor = static_cast<float>(adj.highlights / 100.0);
            r = lerp(r, MID_GRAY, -highlightFactor);
            g = lerp(g, MID_GRAY, -highlightFactor);
            b = lerp(b, MID_GRAY, -highlightFactor);
        }

        // 阴影调整
        if (adj.shadows > 0) {
            float shadowFactor = static_cast<float>(adj.shadows / 100.0);
            float luminance = getLuminance(r, g, b);

            if (luminance < 0.3f) {
                float blend = 1.0f - (luminance / 0.3f);
                r = lerp(r, 0.0f, blend * shadowFactor);
                g = lerp(g, 0.0f, blend * shadowFactor);
                b = lerp(b, 0.0f, blend * shadowFactor);
            }
        }
        else if (adj.shadows < 0) {
            float shadowFactor = static_cast<float>(adj.shadows / 100.0);
            r = lerp(r, MID_GRAY, shadowFactor);
            g = lerp(g, MID_GRAY, shadowFactor);
            b = lerp(b, MID_GRAY, shadowFactor);
        }
    }

    HistogramData ImageProcessor::calculateHistogram(const ImageData &image) {
        HistogramData hist;

        if (!image.pixels || image.width <= 0 || image.height <= 0) {
            return hist;
        }

        const int64_t totalPixels = static_cast<int64_t>(image.width) * image.height;
        const uint8_t *pixelPtr = image.pixels;

        // 并行计算直方图
        constexpr int numBins = 256;
        std::vector<HistogramData> threadHists(std::thread::hardware_concurrency());

        concurrency::parallel_for(int64_t(0), totalPixels, int64_t(1024),
            [&] (int64_t start) {
                int64_t end = std::min(start + 1024, totalPixels);
                HistogramData localHist;
                const uint8_t *localPtr = pixelPtr + start * 4;

                for (int64_t i = start; i < end; ++i) {
                    uint8_t b = localPtr[0];
                    uint8_t g = localPtr[1];
                    uint8_t r = localPtr[2];

                    localHist.blue[b]++;
                    localHist.green[g]++;
                    localHist.red[r]++;
                    localHist.luminance[static_cast<int>(0.299 * r + 0.587 * g + 0.114 * b)]++;

                    localPtr += 4;
                }

                // 合并到线程本地直方图
                auto threadId = concurrency::Context::CurrentContext()->Id();
                for (int j = 0; j < numBins; ++j) {
                    threadHists[threadId].red[j] += localHist.red[j];
                    threadHists[threadId].green[j] += localHist.green[j];
                    threadHists[threadId].blue[j] += localHist.blue[j];
                    threadHists[threadId].luminance[j] += localHist.luminance[j];
                }
            });

        // 合并所有线程的直方图
        for (const auto &threadHist : threadHists) {
            for (int i = 0; i < numBins; ++i) {
                hist.red[i] += threadHist.red[i];
                hist.green[i] += threadHist.green[i];
                hist.blue[i] += threadHist.blue[i];
                hist.luminance[i] += threadHist.luminance[i];
            }
        }

        return hist;
    }

    // 工厂函数
    std::unique_ptr<ImageProcessor> createImageProcessor() {
        return std::make_unique<ImageProcessor>();
    }

} // namespace ImageProcessor

// C接口实现
extern "C" {

    IMAGEPROCESSOR_API void *image_processor_create() {
        return new ImageProcessor::ImageProcessor();
    }

    IMAGEPROCESSOR_API void image_processor_destroy(void *processor) {
        delete static_cast<ImageProcessor::ImageProcessor *>(processor);
    }

    IMAGEPROCESSOR_API bool image_processor_process(
        void *processor,
        uint8_t *pixels,
        int32_t width,
        int32_t height,
        int32_t stride,
        const ImageProcessor::ImageAdjustments *adjustments) {

        if (!processor || !pixels || !adjustments) {
            return false;
        }

        ImageProcessor::ImageData image(pixels, width, height, stride);
        auto *proc = static_cast<ImageProcessor::ImageProcessor *>(processor);

        return proc->processImageMT(image, *adjustments);
    }

    IMAGEPROCESSOR_API bool image_processor_get_histogram(
        void *processor,
        const uint8_t *pixels,
        int32_t width,
        int32_t height,
        int32_t stride,
        int32_t *redHist,
        int32_t *greenHist,
        int32_t *blueHist,
        int32_t *luminanceHist) {

        if (!processor || !pixels || !redHist || !greenHist || !blueHist || !luminanceHist) {
            return false;
        }

        ImageProcessor::ImageData image(const_cast<uint8_t *>(pixels), width, height, stride);
        auto *proc = static_cast<ImageProcessor::ImageProcessor *>(processor);

        auto hist = proc->calculateHistogram(image);

        // 复制数据到输出数组
        std::copy(std::begin(hist.red), std::end(hist.red), redHist);
        std::copy(std::begin(hist.green), std::end(hist.green), greenHist);
        std::copy(std::begin(hist.blue), std::end(hist.blue), blueHist);
        std::copy(std::begin(hist.luminance), std::end(hist.luminance), luminanceHist);

        return true;
    }

}