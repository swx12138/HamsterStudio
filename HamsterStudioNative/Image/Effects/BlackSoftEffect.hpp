#pragma once

#include <opencv2/opencv.hpp>
#include <iostream>

#include "./Effects.hpp"

ImageEffectsNamespaceBegin;

class DarkCornerEffect {
public:
    /**
	 * @brief 为图像添加暗角效果，模拟镜头暗角现象，增强图像的艺术感和氛围。
     * @param src 输入的原始图像
     * @param strength 暗角强度 (0.0 - 1.0)，值越大暗角越明显
     * @param vignette_size 暗角范围 (0.0 - 1.0)，值越大暗角覆盖范围越广
     * @return 应用效果后的图像
     */
    static cv::Mat applyEffect(const cv::Mat &src, double strength = 0.5f, double vignette_size = 0.6f) {
        if (src.empty()) {
            return src;
        }

        cv::Mat dst = src.clone();

        // 确保图像为浮点型以进行精确计算
        cv::Mat img_f;
        dst.convertTo(img_f, CV_32F);

        int width = img_f.cols;
        int height = img_f.rows;

        // 计算图像中心点
        cv::Point2f center(width / 2.0f, height / 2.0f);

        // 计算从中心到最远角落的距离，作为归一化基准
        double max_distance = cv::norm(center);

        // 创建一个与图像同样大小的遮罩矩阵
        cv::Mat mask = cv::Mat::ones(height, width, CV_32F);

        for (int y = 0; y < height; ++y) {
            for (int x = 0; x < width; ++x) {
                // 计算当前像素到中心的距离
                cv::Point2f pixel_pos((float)x, (float)y);
                double distance_from_center = cv::norm(pixel_pos - center);

                // 归一化距离
                double normalized_dist = distance_from_center / max_distance;

                // 根据 vignette_size 参数调整遮罩的陡峭程度
                // vignette_size 越小，遮罩变化越陡峭，暗角范围越小
                double adjusted_dist = std::pow(normalized_dist, 1.0f / vignette_size);

                // 计算遮罩值，strength 控制暗角的深浅
                // normalized_dist 为 0 时，mask_value 为 1 (无影响)
                // normalized_dist 为 1 时，mask_value 为 (1 - strength)
                double mask_value = 1.0f - (strength * adjusted_dist);

                // 将遮罩值应用到该像素的每个通道 (B, G, R)
                for (int c = 0; c < img_f.channels(); ++c) {
                    img_f.at<cv::Vec3f>(y, x)[c] *= (float)mask_value;
                }
            }
        }

        // 将结果转换回 8 位无符号整型
        img_f.convertTo(dst, CV_8UC3);

        // --- 可选的额外处理：降低亮度，增加一点对比度 ---
        // 这一步可以进一步强化“黑柔”的感觉
        dst.convertTo(dst, -1, 0.9, -10); // gamma < 1 提高对比度, bias < 0 降低亮度

        return dst;
    }
};

/**
 * @brief 应用黑柔滤镜效果
 * @param src 输入的原始图像
 * @param high_pass_sigma 高反差保留步骤中的高斯核标准差，控制柔化强度
 * @param darkening_sigma 深化暗部步骤中的高斯核标准差
 * @param darkening_strength 深化暗部的强度
 * @return 应用滤镜后的图像
 */
cv::Mat applyBlackVelvetEffect(const cv::Mat &src, double high_pass_sigma = 1.0, double darkening_sigma = 0.5, double darkening_strength = 1.0) {
    if (src.empty() || src.channels() != 3) {
        std::cerr << "Error: Input image is empty or not a 3-channel BGR image." << std::endl;
        return src;
    }

    cv::Mat src_double;
    src.convertTo(src_double, CV_32F, 1.0 / 255.0); // 转换为浮点型，便于计算

    // 1. 高反差保留 (High Pass Filter)
    cv::Mat blurred;
    cv::GaussianBlur(src_double, blurred, cv::Size(0, 0), high_pass_sigma, high_pass_sigma);
    cv::Mat high_pass_detail = src_double - blurred; // 得到细节图

    // 2. 分离明暗细节
    cv::Mat bright_details = cv::Mat::zeros(high_pass_detail.size(), high_pass_detail.type());
    cv::Mat dark_details = cv::Mat::zeros(high_pass_detail.size(), high_pass_detail.type());

    // 仅保留亮部细节 (正值部分)
    cv::compare(high_pass_detail, cv::Mat::zeros(high_pass_detail.size(), high_pass_detail.type()), bright_details, cv::CMP_GT);
    bright_details = bright_details.mul(high_pass_detail);

    // 仅保留暗部细节 (负值部分)
    cv::compare(high_pass_detail, cv::Mat::zeros(high_pass_detail.size(), high_pass_detail.type()), dark_details, cv::CMP_LT);
    dark_details = dark_details.mul(high_pass_detail);

    // 3. 柔化亮部
    cv::Mat softened_bright_details;
    cv::GaussianBlur(bright_details, softened_bright_details, cv::Size(0, 0), high_pass_sigma * 2, high_pass_sigma * 2); // 使用更大的sigma

    // 4. 深化暗部
    cv::Mat darkened_image = src_double;
    cv::Mat blurred_dark_details;
    cv::GaussianBlur(dark_details, blurred_dark_details, cv::Size(0, 0), darkening_sigma, darkening_sigma);
    darkened_image = darkened_image + blurred_dark_details * darkening_strength;

    // 5. 合成最终图像
    cv::Mat final_image = darkened_image + softened_bright_details;

    // 6. 裁剪并转换回8位
    cv::Mat result;
    cv::convertScaleAbs(final_image, result, 255.0, 0);

    return result;
}

ImageEffectsNamespaceEnd
