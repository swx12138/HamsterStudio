
#define _WINSOCK_DEPRECATED_NO_WARNINGS

#include "platform/os.h"

#include <stdexcept>
#include <format>

namespace toes::win32
{
    const UINT dpi = ::GetDpiForSystem();
    const int cx = ::GetSystemMetricsForDpi(SM_CXSCREEN, dpi);
    const int cy = ::GetSystemMetricsForDpi(SM_CYSCREEN, dpi);

    void SetIcon(HWND hWnd, const HICON hIcon)
    {
        ::SendMessageA(hWnd, WM_SETICON, ICON_BIG, reinterpret_cast<LPARAM>(hIcon));
        ::SendMessageA(hWnd, WM_SETICON, ICON_SMALL, reinterpret_cast<LPARAM>(hIcon));
    }

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

double toes::win32::GetWindowsZoom()
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

bool toes::win32::PathCompare(const std::filesystem::path &a, const std::filesystem::path &b)
{
    return StrCmpLogicalW(a.filename().c_str(), b.filename().c_str()) < 0;
}

#include <psapi.h> // 包含 GetProcessMemoryInfo 的声明

size_t toes::win32::GetMemoryUsage(HANDLE hProcess)
{
    if (hProcess == NULL) {
        hProcess = ::GetCurrentProcess();
    }

    PROCESS_MEMORY_COUNTERS pmc;
    if (K32GetProcessMemoryInfo(GetCurrentProcess(), &pmc, sizeof(pmc))) {
        return pmc.WorkingSetSize; // 返回当前内存使用量（以字节为单位）
    }
    else {
        throw std::runtime_error("Failed to get process memory info");
    }
}

std::string toes::win32::GetCurrentWallpaperPath()
{
    HKEY hKey = nullptr;
    const char szSubkey[] = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Wallpapers";
    if (ERROR_SUCCESS == ::RegOpenKeyExA(HKEY_CURRENT_USER, szSubkey, 0, KEY_ALL_ACCESS, &hKey))
    {
        DWORD size = 0x100;
        char* szBuffer = new char[size];
        if (ERROR_SUCCESS == ::RegGetValueA(hKey, nullptr, "BackgroundHistoryPath0", RRF_RT_REG_SZ, nullptr, szBuffer, &size))
        {
            return szBuffer;
        }
    }
    return "";
}

bool toes::win32::SetWallpaper(std::string const &path)
{
    return SystemParametersInfoA(SPI_SETDESKWALLPAPER, 0, (LPVOID)path.c_str(), SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);
}

HWND toes::win32::FindWallpaperLayer(HWND *theDesktopWorkerW)
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

#include "core/diagnostics.h"

void toes::win32::EnumHardLinkFiles(const std::filesystem::path& file_path, std::function<bool(std::filesystem::path const&)>&& pred)
{
    std::vector<wchar_t> linkNameBuffer(0x400);
    DWORD strLength = static_cast<DWORD>(linkNameBuffer.capacity());
    HANDLE hFileName = FindFirstFileNameW(file_path.c_str(), 0, &strLength, linkNameBuffer.data()); // Get required buffer size
    if (hFileName == INVALID_HANDLE_VALUE) {
        // 原实现直接 ExitProcess；库层改为抛 std::system_error，由调用方决定怎么处理
        diagnostics::ThrowLastError();
    }

    // 保证 pred 抛异常时句柄也会被关掉
    auto const closeFind = +[](void *handle) { ::FindClose(handle); };
    auto const guard = std::unique_ptr<void, void (*)(void *)>(hFileName, closeFind);

    do {
        std::wstring linkName(linkNameBuffer.data(), strLength);
        pred(linkName);
        strLength = static_cast<DWORD>(linkNameBuffer.capacity());
    } while (::FindNextFileNameW(guard.get(), &strLength, linkNameBuffer.data()) == TRUE);

    // 迭代正常结束时 GetLastError() 是 ERROR_NO_MORE_FILES。原代码只容忍
    // ERROR_HANDLE_EOF，于是正常收尾也会 ExitProcess——这是个潜在缺陷，一并修掉。
    diagnostics::ThrowLastError({ ERROR_NO_MORE_FILES, ERROR_HANDLE_EOF });
}

bool toes::win32::IsInstanceExist(std::string_view const _uuid)
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

long toes::win32::DrawSheet(HDC hdc, sheet_t const &sht, const ::POINT &start, long h, long wl, long wr)
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
