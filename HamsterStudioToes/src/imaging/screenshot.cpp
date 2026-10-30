#include "imaging/screenshot.h"

#include "win32/os.h"

#include <opencv2/opencv.hpp>
#include <vector>
#include <stdexcept>

#pragma warning(push)
#pragma warning(disable : 4244)

// 只负责捕获屏幕像素，不管理 GDI 泄漏
cv::Mat getScreenshot()
{
    double zoom = Win32Namespace::GetWindowsZoom();
    int width  = static_cast<int>(GetSystemMetrics(SM_CXSCREEN) * zoom);
    int height = static_cast<int>(GetSystemMetrics(SM_CYSCREEN) * zoom);

    // 获取屏幕 DC
    HDC screenDC = ::GetDC(NULL);
    if (!screenDC) {
        return cv::Mat(); // 返回空矩阵
    }

    // 创建兼容 DC
    HDC compatibleDC = ::CreateCompatibleDC(screenDC);
    if (!compatibleDC) {
        ::ReleaseDC(NULL, screenDC);
        return cv::Mat();
    }

    // 创建兼容位图
    HBITMAP hBitmap = ::CreateCompatibleBitmap(screenDC, width, height);
    if (!hBitmap) {
        ::DeleteDC(compatibleDC);
        ::ReleaseDC(NULL, screenDC);
        return cv::Mat();
    }

    // 把位图选入 DC 并保存原图
    HBITMAP hOldBitmap = (HBITMAP)::SelectObject(compatibleDC, hBitmap);

    // 拷贝屏幕内容到兼容 DC
    if (!::BitBlt(compatibleDC, 0, 0, width, height, screenDC, 0, 0, SRCCOPY)) {
        ::SelectObject(compatibleDC, hOldBitmap);
        ::DeleteObject(hBitmap);
        ::DeleteDC(compatibleDC);
        ::ReleaseDC(NULL, screenDC);
        return cv::Mat();
    }

    // 用 vector 管理像素数据（RAII，自动释放）
    int cb = width * height * 4;
    std::vector<char> buf(cb);
    if (!::GetBitmapBits(hBitmap, cb, buf.data())) {
        ::SelectObject(compatibleDC, hOldBitmap);
        ::DeleteObject(hBitmap);
        ::DeleteDC(compatibleDC);
        ::ReleaseDC(NULL, screenDC);
        return cv::Mat();
    }

    // 创建 OpenCV Mat（会拷贝数据，此时 buf 可安全销毁）
    cv::Mat screenshot(height, width, CV_8UC4, buf.data());
    // 强制拷贝：让 Mat 拥有独立内存
    cv::Mat result = screenshot.clone();

    // 清理 GDI 资源
    ::SelectObject(compatibleDC, hOldBitmap);
    ::DeleteObject(hBitmap);
    ::DeleteDC(compatibleDC);
    ::ReleaseDC(NULL, screenDC);

    return result;
}

// ---------- Impl 实现 ----------
struct ScreenshotNamespace::Screenshot::Impl
{
    cv::Mat m_data;
};

// ---------- Screenshot 成员函数 ----------
ScreenshotNamespace::Screenshot::Screenshot()
{
    auto scr = getScreenshot();
    if (scr.empty()) {
        // 截图失败时返回空对象
        pImpl = std::make_unique<Impl>();
        return;
    }

    HWND hCon = ::GetConsoleWindow();
    RECT rc{};
    ::GetClientRect(hCon, &rc);

    POINT pt{0, 0};
    ::ClientToScreen(hCon, &pt);

    pImpl = std::make_unique<Impl>();
    pImpl->m_data = scr(cv::Rect(pt.x, pt.y, rc.right, rc.bottom)).clone();
}

ScreenshotNamespace::Screenshot::Screenshot(int x, int y, int width, int height)
{
    auto scr = getScreenshot();
    pImpl = std::make_unique<Impl>();
    if (!scr.empty()) {
        pImpl->m_data = scr(cv::Rect(x, y, width, height)).clone();
    }
}

ScreenshotNamespace::Screenshot::~Screenshot() = default; // 必须在此定义，让 unique_ptr 看到 Impl 的完整定义

void ScreenshotNamespace::Screenshot::saveAs(std::string_view filename)
{
    if (!pImpl->m_data.empty()) {
        cv::imwrite(filename.data(), pImpl->m_data);
    }
}

#pragma warning(pop)