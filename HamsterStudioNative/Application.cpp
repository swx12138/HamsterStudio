
#include <memory>
#include <string>
#include <functional>
#include <thread>

#include "./Win32/Win32.hpp"

constexpr std::string_view AppName = "HamsterStudioApp";
constexpr std::string_view AppTitle = "HamsterStudio";

namespace HamsterStudio
{
    class Window;

    class Application
    {
    public:
        Application(_In_ HINSTANCE hInst, _In_opt_ HINSTANCE hPrevInst) : hInst(hInst)
        {
            //_Container = std::make_shared<Core::Container>();
            //_Logger = std::make_shared<Core::Logger>();
            pWindow = std::make_shared<Window>(this);
        }

        Application(Application const &) = delete;
        Application(Application &&) = delete;
        ~Application() {}

    public:
        HINSTANCE GetInstance() const { return hInst; }

        std::shared_ptr<const Window> GetMainWindow() const { return pWindow; }

    private:
        HINSTANCE hInst;
        std::shared_ptr<Window> pWindow;
        // std::shared_ptr<Core::Container> _Container;
        // std::shared_ptr<Core::Logger> _Logger;
    };

    class Window
    {
    public:
        Window(const Application *pApp)
        {
            _RegisterWindow(pApp->GetInstance());
            _CreateWindow();
        }

        Window(Window const &) = delete;
        Window(Window &&) = delete;

        ~Window() {}

    public:
        int Run() const
        {
            ShowWindow(hWnd, SW_SHOW);
            UpdateWindow(hWnd);
            return _RunMessageLoop();
        }

        virtual void OnLeftButtonUp(int x, int y) const
        {
            Win32::SetWallpaper("D:\\70065180186060360_17_bili_d3df0dae29fb84993d8c39e01a1a78c4dc448095.jpg");
            return;

            ::HWND hDesktop = Win32::FindWallpaperLow();
            ::HDC hDesctopDc = ::GetDC(hDesktop);
            ::Rectangle(hDesctopDc, 100, 100, 1100, 1100);

            Gdiplus::Graphics gr(hDesctopDc);
            Gdiplus::Point px(100, 100);
            Gdiplus::Point py(1100, 1100);

            Gdiplus::LinearGradientBrush brush(px, py, Gdiplus::Color::LightSkyBlue, Gdiplus::Color::Blue);

            Gdiplus::Rect rf(100, 100, 1000, 1000);
            gr.FillRectangle(&brush, rf);

            // InvalidateRect(hWnd, nullptr, true);
        }

    private:
        virtual bool _RegisterWindow(HINSTANCE hInst);

        virtual bool _CreateWindow()
        {
            // 创建窗口
            hWnd = CreateWindowExA(
                0,                   // 扩展样式
                AppName.data(),      // 类名
                AppTitle.data(),     // 窗口标题
                WS_OVERLAPPEDWINDOW, // 样式
                0,                   // 右侧位置，例如距离右侧边缘200像素
                0,                   // 上侧位置，距离顶部边缘0像素
                500,                 // 宽度
                500,                 // 高度
                NULL,                // 父窗口句柄，无父窗口
                NULL,                // 菜单句柄，这里没有菜单
                wndClass.hInstance,  // 程序实例句柄
                NULL                 // 创建窗口附加的应用程序数据
            );

            if (!hWnd)
            {
                MessageBoxA(NULL, "CreateWindow Failed", "Error", MB_OK);
                return false;
            }

            //::SetParent(hWnd, (HWND)Win32::FindWallpaper());

            return true;
        }

        virtual int _RunMessageLoop() const
        {
            // 消息循环
            MSG msg;
            while (GetMessageA(&msg, NULL, 0, 0))
            {
                TranslateMessage(&msg);
                DispatchMessageA(&msg);
            }

            return msg.wParam;
        }

        virtual int _DobleBuffer(std::function<void(::HDC, int, int)> draw) const
        {
            PAINTSTRUCT ps;
            HDC hdc = BeginPaint(hWnd, &ps);

            // 获取设备上下文
            HDC memDC = CreateCompatibleDC(hdc);
            int cx = GetDeviceCaps(hdc, HORZRES), cy = GetDeviceCaps(hdc, VERTRES);
            HBITMAP bitmap = CreateCompatibleBitmap(hdc, cx, cy);
            SelectObject(memDC, bitmap);

            draw(memDC, cx, cy);

            // 将内存 DC 的内容复制到屏幕上的 DC
            BitBlt(hdc, 0, 0, cx, cy, memDC, 0, 0, SRCCOPY);

            // 清理资源
            DeleteObject(bitmap);
            DeleteDC(memDC);

            return EndPaint(hWnd, &ps);
        }

    private:
        ::HWND hWnd;
        ::WNDCLASSA wndClass;
        Win32::GdiplusEnv _Init;
    };

}

HamsterStudio::Application *app;

int WINAPI WinMain(_In_ HINSTANCE hInst, _In_opt_ HINSTANCE hPrevInst, _In_ LPSTR lpCmdLine, _In_ int nShowCmd)
{
    app = new HamsterStudio::Application(hInst, hPrevInst);
    app->GetMainWindow()->Run();
    return 0;
}

// 窗口过程
static LRESULT CALLBACK WndProc(HWND hWnd, UINT message, WPARAM wParam, LPARAM lParam)
{
    switch (message)
    {
    case WM_CREATE:
        // 窗口创建时的操作
        break;
    case WM_PAINT:
    {
        PAINTSTRUCT ps;
        HDC hdc = BeginPaint(hWnd, &ps);
        // 在这里绘制内容
        EndPaint(hWnd, &ps);
    }
    break;
    case WM_LBUTTONUP:
    {
        // 获取鼠标点击位置
        int x = LOWORD(wParam);
        int y = HIWORD(wParam);
        app->GetMainWindow()->OnLeftButtonUp(x, y);
    }
    break;
    case WM_DESTROY:
        PostQuitMessage(0);
        break;
    default:
        return DefWindowProc(hWnd, message, wParam, lParam);
    }
    return 0;
}

bool HamsterStudio::Window::_RegisterWindow(HINSTANCE hInst)
{
    // 定义窗口类
    wndClass = {0};
    wndClass.style = CS_HREDRAW | CS_VREDRAW;            // 支持水平和垂直重绘
    wndClass.lpfnWndProc = WndProc;                      // 窗口过程
    wndClass.cbClsExtra = 0;                             // 不使用额外的类内存
    wndClass.cbWndExtra = 0;                             // 不使用额外的窗口内存
    wndClass.hInstance = hInst;                          // 当前实例
    wndClass.hIcon = LoadIcon(NULL, IDI_APPLICATION);    // 应用程序图标
    wndClass.hCursor = LoadCursor(NULL, IDC_ARROW);      // 箭头光标
    wndClass.hbrBackground = (HBRUSH)(COLOR_WINDOW + 1); // 默认背景色
    wndClass.lpszMenuName = NULL;                        // 没有菜单
    wndClass.lpszClassName = AppName.data();             // 类名称

    // 注册窗口类
    if (!::RegisterClassA(&wndClass))
    {
        ::MessageBoxA(NULL, "RegisterClass Failed", "Error", MB_OK);
        return false;
    }
    return true;
}
