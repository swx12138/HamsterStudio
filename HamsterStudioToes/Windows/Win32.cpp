
#define _WINSOCK_DEPRECATED_NO_WARNINGS

#include "./Win32.hpp"

#include <stdexcept>
#include <format>

#pragma comment(lib, "Gdiplus.lib")

namespace HamsterStudio::Win32
{
    const UINT dpi = ::GetDpiForSystem();
    const int cx = ::GetSystemMetricsForDpi(SM_CXSCREEN, dpi);
    const int cy = ::GetSystemMetricsForDpi(SM_CYSCREEN, dpi);

    std::string GetCurrentWallpaper()
    {
        HKEY hKey = nullptr;
        const char szSubkey[] = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Wallpapers";
        if (ERROR_SUCCESS == ::RegOpenKeyExA(HKEY_CURRENT_USER, szSubkey, 0, KEY_ALL_ACCESS, &hKey))
        {
            DWORD size = 0x100;
            char *szBuffer = new char[size];
            if (ERROR_SUCCESS == ::RegGetValueA(hKey, nullptr, "BackgroundHistoryPath0", RRF_RT_REG_SZ, nullptr, szBuffer, &size))
            {
                return szBuffer;
            }
        }
        return "";
    }

    void SetIcon(HWND hWnd, const HICON hIcon)
    {
        ::SendMessageA(hWnd, WM_SETICON, ICON_BIG, reinterpret_cast<LPARAM>(hIcon));
        ::SendMessageA(hWnd, WM_SETICON, ICON_SMALL, reinterpret_cast<LPARAM>(hIcon));
    }

    class WindowComposition
    {
    public:
        enum class Attribute : unsigned int
        {
            UNDEFINED = 0,
            NCRENDERING_ENABLED = 1,
            NCRENDERING_POLICY = 2,
            TRANSITIONS_FORCEDISABLED = 3,
            ALLOW_NCPAINT = 4,
            CAPTION_BUTTON_BOUNDS = 5,
            NONCLIENT_RTL_LAYOUT = 6,
            FORCE_ICONIC_REPRESENTATION = 7,
            EXTENDED_FRAME_BOUNDS = 8,
            HAS_ICONIC_BITMAP = 9,
            THEME_ATTRIBUTES = 10,
            NCRENDERING_EXILED = 11,
            NCADORNMENTINFO = 12,
            EXCLUDED_FROM_LIVEPREVIEW = 13,
            VIDEO_OVERLAY_ACTIVE = 14,
            FORCE_ACTIVEWINDOW_APPEARANCE = 15,
            DISALLOW_PEEK = 16,
            CLOAK = 17,
            CLOAKED = 18,
            ACCENT_POLICY = 19,
            FREEZE_REPRESENTATION = 20,
            EVER_UNCLOAKED = 21,
            VISUAL_OWNER = 22,
            LAST = 23
        };

        struct Data
        {
            Attribute nAttribute;
            PVOID pvData;
            SIZE_T cbData;
        };

        enum class ACCENT_STATE
        {
            DISABLED = 0,                   // 全黑
            ENABLE_GRADIENT = 1,            // 单色不透明
            ENABLE_TRANSPARENTGRADIENT = 2, // 全透明
            ENABLE_BLURBEHIND = 3,          // 模糊
            ENABLE_ACRYLICBLURBEHIND = 4,   // 亚克力
            INVALID_STATE = 5,              // 全黑
            ACCENT_NORMAL = 150             // (Fake value) Emulate regular taskbar appearance
        };

        struct AccentPolicy
        {
            ACCENT_STATE AccentState;
            DWORD AccentFlags;
            DWORD GradientColor;
            DWORD AnimationId;
        };

        static int Set(HWND hWnd, ACCENT_STATE mode, DWORD AlphaColor) {}
    };

    // 任务栏
    class TaskbarCtrl
    {
        using ACCENT_STATE = WindowComposition::ACCENT_STATE;

    public:
        TaskbarCtrl(COLORREF bgColor = 0x7a7ae9, BYTE nOpac = 68, BYTE nOpacAll = 255) : m_hWnd(FindWindowA("Shell_TrayWnd", NULL)),
                                                                                         m_dwOpacity(nOpac),
                                                                                         m_dwOpacityAll(nOpacAll),
                                                                                         m_crAlphaColor(bgColor),
                                                                                         m_style(ACCENT_STATE::ENABLE_TRANSPARENTGRADIENT)
        {
        }

        TaskbarCtrl(const TaskbarCtrl &) = delete;
        ~TaskbarCtrl() = default;

    public:
        BOOL update() const
        {
            static LONG_PTR n = ::SetWindowLongPtrA(m_hWnd, GWL_EXSTYLE, ::GetWindowLongPtrA(m_hWnd, GWL_EXSTYLE) | WS_EX_LAYERED);
            BOOL b = WindowComposition::Set(m_hWnd, m_style, (m_dwOpacity << 24) + (m_crAlphaColor & 0xffffff));
            return b && ::SetLayeredWindowAttributes(m_hWnd, 0, m_dwOpacityAll, LWA_ALPHA);
        }
        void OnTaskbarCreated()
        {
            m_hWnd = ::FindWindowA("Shell_TrayWnd", NULL);
        }

    private:
        COLORREF m_crAlphaColor; // 颜色
        BYTE m_dwOpacity;        // 颜色透明度 0-255
        ACCENT_STATE m_style;    // 样式
        BYTE m_dwOpacityAll;     // 整体透明度 0-255
        HWND m_hWnd;
    };

    /*
        BOOL WindowComposition::Set(HWND hWnd, ACCENT_STATE mode, unsigned long AlphaColor)
        {
            WindowComposition::AccentPolicy accent = {mode, 0, AlphaColor, 0};
            WindowComposition::Data data;

            data.nAttribute = WindowComposition::Attribute::ACCENT_POLICY;
            data.pvData = &accent;
            data.cbData = sizeof(accent);

            // return user32::SetWindowCompositionAttribute(hWnd, &data);
            static auto pFnSWCA =
                reinterpret_cast<std::add_pointer_t<BOOL WINAPI(decltype(hWnd), WindowComposition::Data *)>>(
                    GetProcAddress(GetModuleHandleA("user32.dll"), "SetWindowCompositionAttribute"));

            return pFnSWCA(hWnd, &data);
        }
        */

    SystemInfo::SystemInfo()
    {
        ::SYSTEM_INFO si = {0};
        ::GetSystemInfo(&si);

        _PageSize = si.dwPageSize;

        _MinApplAddr = reinterpret_cast<uint64_t>(si.lpMinimumApplicationAddress);
        _MaxApplAddr = reinterpret_cast<uint64_t>(si.lpMaximumApplicationAddress);

        _CpuCount = si.dwNumberOfProcessors;

        //_Archit = si.wProcessorArchitecture;
        //_Level = si.wProcessorLevel;
        //_Revision = si.wProcessorRevision;
    }

    std::shared_ptr<SystemInfo> SystemInfo::GetInst()
    {
        static std::shared_ptr<SystemInfo> si(nullptr);
        if (si == nullptr)
        {
            si = std::make_shared<SystemInfo>();
        }
        return si;
    }

}

double HamsterStudio::Win32::GetWindowsZoom()
{
    // 获取窗口当前显示的监视器
    HWND hWnd = GetDesktopWindow();
    HMONITOR hMonitor = MonitorFromWindow(hWnd, MONITOR_DEFAULTTONEAREST);

    // 获取监视器逻辑宽度
    MONITORINFOEX monitorInfo;
    monitorInfo.cbSize = sizeof(monitorInfo);
    GetMonitorInfo(hMonitor, &monitorInfo);
    int cxLogical = (monitorInfo.rcMonitor.right - monitorInfo.rcMonitor.left);

    // 获取监视器物理宽度
    DEVMODE dm;
    dm.dmSize = sizeof(dm);
    dm.dmDriverExtra = 0;
    EnumDisplaySettings(monitorInfo.szDevice, ENUM_CURRENT_SETTINGS, &dm);
    int cxPhysical = dm.dmPelsWidth;

    return cxPhysical * 1.0 / cxLogical;
}

std::string HamsterStudio::Win32::GetErrorMessage()
{
    DWORD dwError = 126;

    HLOCAL hLocal = NULL;

    DWORD sysLocal = MAKELANGID(LANG_NEUTRAL, SUBLANG_NEUTRAL);

    BOOL fOk = FormatMessageA(FORMAT_MESSAGE_FROM_SYSTEM | FORMAT_MESSAGE_IGNORE_INSERTS | FORMAT_MESSAGE_ALLOCATE_BUFFER, NULL, dwError, sysLocal, (PSTR)&hLocal, 0, NULL);

    if (!fOk)
    {
        HMODULE hDll = LoadLibraryA("netmsg.dll");
        if (hDll)
        {
            fOk = FormatMessageA(FORMAT_MESSAGE_FROM_SYSTEM | FORMAT_MESSAGE_IGNORE_INSERTS | FORMAT_MESSAGE_ALLOCATE_BUFFER, hDll, dwError, sysLocal, (PSTR)&hLocal, 0, NULL);
            FreeLibrary(hDll);
        }
    }

    if (fOk && hLocal != NULL)
    {
        // printf_s("code:%d,msg:%s\n", dwError, hLocal);
        std::string ret = (char *)hLocal;
        LocalFree(hLocal);
        return ret;
    }

    throw std::runtime_error(std::format("std::string api::except::GetErrorMessage() Failed.code : {}", fOk).c_str());
}

bool HamsterStudio::Win32::SetWallpaper(std::string const &path)
{
    return SystemParametersInfoA(SPI_SETDESKWALLPAPER, 0, (LPVOID)path.c_str(), SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);
}

HWND HamsterStudio::Win32::FindWallpaperLow(HWND *theDesktopWorkerW)
{
    HWND **pParam = new HWND *[2];
    pParam[0] = new HWND;
    pParam[1] = theDesktopWorkerW;

    DWORD_PTR result;
    HWND windowHandle = FindWindowA("Progman", nullptr);
    SendMessageTimeout(windowHandle, 0x052c, 0, 0, SMTO_NORMAL, 0x3e8,
                       (PDWORD_PTR)&result);

    EnumWindows([](_In_ HWND tophandle, _In_ LPARAM lParam)
                {
			HWND defview = FindWindowExA(tophandle, 0, "SHELLDLL_DefView", nullptr);
			if (defview != nullptr) {
				HWND **phWnds = (HWND **)lParam;
				*phWnds[0] = FindWindowExA(0, tophandle, "WorkerW", 0);
				if (nullptr != phWnds[1])
					*phWnds[1] = tophandle;
				return FALSE;
			}
			return TRUE; },
                (LPARAM)(pParam));
    if (!*pParam[0])
    {
        return HWND();
    }
    return *pParam[0];
}

bool HamsterStudio::Win32::IsInstanceExist(std::string_view const _uuid)
{
    //{ A83E7C7F - 8C59 - 4984 - 8E70 - 5F3AB97B90A0 } "A83E7C7F-8C59-4984-8E70-5F3AB97B90A0"
    HANDLE hMutex = CreateMutexA(NULL, FALSE, _uuid.data());
    if (hMutex && (GetLastError() == ERROR_ALREADY_EXISTS))
    {
        CloseHandle(hMutex);
        hMutex = NULL;
        return true;
    }
    else
    {
        return false;
    }
}

long HamsterStudio::Win32::Drawing::Sheet(HDC hdc, sheet_t const &sht, const ::POINT &start, long h, long wl, long wr)
{
    // 顶部线
    ::MoveToEx(hdc, start.x, start.y, nullptr);
    ::LineTo(hdc, start.x + wl + wr, start.y);

    RECT rc = {start.x, start.y, start.x + wl, start.y + h};

    if (!sht.title.empty())
    {
        // 第一列
        rc.left = start.x;
        rc.right = start.x + wl + wr;
        ::DrawTextA(hdc, sht.title.c_str(), -1, &rc, DT_VCENTER | DT_CENTER | DT_SINGLELINE);

        // 行底部线
        ::MoveToEx(hdc, start.x, rc.bottom, nullptr);
        ::LineTo(hdc, rc.right, rc.bottom);

        rc.top = rc.bottom;
        rc.bottom += h;
    }

    // 主体
    for (auto &e : sht.body)
    {
        // 第一列
        rc.left = start.x;
        rc.right = start.x + wl;
        ::DrawTextA(hdc, e.first.c_str(), -1, &rc, DT_VCENTER | DT_CENTER | DT_SINGLELINE);

        // 第二列
        rc.left = rc.right;
        rc.right = start.x + wl + wr;
        ::DrawTextA(hdc, e.second.c_str(), -1, &rc, DT_VCENTER | DT_CENTER | DT_SINGLELINE);

        // 行底部线
        ::MoveToEx(hdc, start.x, rc.bottom, nullptr);
        ::LineTo(hdc, rc.right, rc.bottom);

        rc.top = rc.bottom;
        rc.bottom += h;
    }

    if (!sht.tail.empty())
    {
        static HFONT hFontSm = ::CreateFontA(
            18, 9, 0, 0, FW_HEAVY,
            0, 0, 0, GB2312_CHARSET,
            OUT_DEFAULT_PRECIS, CLIP_DEFAULT_PRECIS, DEFAULT_QUALITY, DEFAULT_PITCH | FF_DONTCARE,
            nullptr);
        auto hOld = ::SelectObject(hdc, hFontSm);

        // 第一列
        rc.left = start.x;
        rc.right = start.x + wl + wr;
        ::DrawTextA(hdc, sht.tail.c_str(), -1, &rc, DT_VCENTER | DT_CENTER | DT_SINGLELINE);

        // 行底部线
        ::MoveToEx(hdc, start.x, rc.bottom, nullptr);
        ::LineTo(hdc, rc.right, rc.bottom);

        rc.top = rc.bottom;
        rc.bottom += h;

        ::SelectObject(hdc, hOld);
    }

    // 竖线
    for (long e : std::vector<long>{0, wl, wl + wr})
    {
        ::MoveToEx(hdc, start.x + e, start.y + ((e == wl && !sht.title.empty()) ? h : 0), nullptr);
        ::LineTo(hdc, start.x + e, rc.top - ((e == wl && !sht.tail.empty()) ? h : 0));
    }

    return rc.bottom - h;
}
