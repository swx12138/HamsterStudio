
#include <memory>
#include <string>
#include <functional>
#include <thread>

#include "Win32/Win32.h"

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
            // ��������
            hWnd = CreateWindowExA(
                0,                   // ��չ��ʽ
                AppName.data(),      // ����
                AppTitle.data(),     // ���ڱ���
                WS_OVERLAPPEDWINDOW, // ��ʽ
                0,                   // �Ҳ�λ�ã���������Ҳ��Ե200����
                0,                   // �ϲ�λ�ã����붥����Ե0����
                500,                 // ���
                500,                 // �߶�
                NULL,                // �����ھ�����޸�����
                NULL,                // �˵����������û�в˵�
                wndClass.hInstance,  // ����ʵ�����
                NULL                 // �������ڸ��ӵ�Ӧ�ó�������
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
            // ��Ϣѭ��
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

            // ��ȡ�豸������
            HDC memDC = CreateCompatibleDC(hdc);
            int cx = GetDeviceCaps(hdc, HORZRES), cy = GetDeviceCaps(hdc, VERTRES);
            HBITMAP bitmap = CreateCompatibleBitmap(hdc, cx, cy);
            SelectObject(memDC, bitmap);

            draw(memDC, cx, cy);

            // ���ڴ� DC �����ݸ��Ƶ���Ļ�ϵ� DC
            BitBlt(hdc, 0, 0, cx, cy, memDC, 0, 0, SRCCOPY);

            // ������Դ
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

// ���ڹ���
static LRESULT CALLBACK WndProc(HWND hWnd, UINT message, WPARAM wParam, LPARAM lParam)
{
    switch (message)
    {
    case WM_CREATE:
        // ���ڴ���ʱ�Ĳ���
        break;
    case WM_PAINT:
    {
        PAINTSTRUCT ps;
        HDC hdc = BeginPaint(hWnd, &ps);
        // �������������
        EndPaint(hWnd, &ps);
    }
    break;
    case WM_LBUTTONUP:
    {
        // ��ȡ�����λ��
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
    // ���崰����
    wndClass = {0};
    wndClass.style = CS_HREDRAW | CS_VREDRAW;            // ֧��ˮƽ�ʹ�ֱ�ػ�
    wndClass.lpfnWndProc = WndProc;                      // ���ڹ���
    wndClass.cbClsExtra = 0;                             // ��ʹ�ö�������ڴ�
    wndClass.cbWndExtra = 0;                             // ��ʹ�ö���Ĵ����ڴ�
    wndClass.hInstance = hInst;                          // ��ǰʵ��
    wndClass.hIcon = LoadIcon(NULL, IDI_APPLICATION);    // Ӧ�ó���ͼ��
    wndClass.hCursor = LoadCursor(NULL, IDC_ARROW);      // ��ͷ���
    wndClass.hbrBackground = (HBRUSH)(COLOR_WINDOW + 1); // Ĭ�ϱ���ɫ
    wndClass.lpszMenuName = NULL;                        // û�в˵�
    wndClass.lpszClassName = AppName.data();             // ������

    // ע�ᴰ����
    if (!::RegisterClassA(&wndClass))
    {
        ::MessageBoxA(NULL, "RegisterClass Failed", "Error", MB_OK);
        return false;
    }
    return true;
}
