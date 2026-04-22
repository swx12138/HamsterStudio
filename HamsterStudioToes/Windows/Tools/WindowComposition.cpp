#include "WindowComposition.h"

int Win32Namespace::WindowComposition::Set(HWND hWnd, ACCENT_STATE mode, DWORD AlphaColor)
{
    WindowComposition::AccentPolicy accent = { mode, 0, AlphaColor, 0 };
    WindowComposition::Data data;

    data.nAttribute = WindowComposition::Attribute::ACCENT_POLICY;
    data.pvData = &accent;
    data.cbData = sizeof(accent);

    // return user32::SetWindowCompositionAttribute(hWnd, &data);
    static auto pFnSWCA =
        reinterpret_cast<std::add_pointer_t<BOOL WINAPI(decltype(hWnd), WindowComposition::Data*)>>(
            GetProcAddress(GetModuleHandleA("user32.dll"), "SetWindowCompositionAttribute"));

    return pFnSWCA(hWnd, &data);
}   