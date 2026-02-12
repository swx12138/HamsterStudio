
#include "./Screenshot.h"

#include "../Win32/Win32.hpp"

#pragma warning(push)
#pragma warning(disable : 4244)

cv::Mat getScreenshot()
{
	double zoom = Win32Namespace::GetWindowsZoom();
	int width = GetSystemMetrics(SM_CXSCREEN) * zoom;
	int height = GetSystemMetrics(SM_CYSCREEN) * zoom;

	// 获取屏幕 DC
	HDC screenDC = ::GetDC(NULL);
	HDC compatibleDC = ::CreateCompatibleDC(screenDC);

	// 创建位图
	HBITMAP hBitmap = ::CreateCompatibleBitmap(screenDC, width, height);
	::SelectObject(compatibleDC, hBitmap);

	// 得到位图的数据
	::BitBlt(compatibleDC, 0, 0, width, height, screenDC, 0, 0, SRCCOPY);

	int cb = width * height * 4;
	void *screenshotData = new char[cb];
	::GetBitmapBits(hBitmap, cb, screenshotData);

	// 创建图像
	cv::Mat screenshot(height, width, CV_8UC4, screenshotData);

	return screenshot;
}

ScreenshotNamespace::Screenshot::Screenshot()
{
	auto scr = getScreenshot();

	HWND hCon = ::GetConsoleWindow();
	RECT rc;
	::GetClientRect(hCon, &rc);

	POINT pt { 0, 0 };
	::ClientToScreen(hCon, &pt);

	m_data = scr(cv::Rect(pt.x, pt.y, rc.right, rc.bottom));
}

ScreenshotNamespace::Screenshot::Screenshot(int x, int y, int width, int height)
{
	auto scr = getScreenshot();
	m_data = scr(cv::Rect(x, y, width, height));
}

void ScreenshotNamespace::Screenshot::saveAs(std::string_view filename)
{
	cv::imwrite(filename.data(), m_data);
}

#pragma warning(pop)
