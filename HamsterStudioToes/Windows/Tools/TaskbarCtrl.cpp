#include "TaskbarCtrl.h"

Win32Namespace::TaskbarCtrl::TaskbarCtrl(COLORREF bgColor, BYTE nOpac, BYTE nOpacAll)
	: m_hWnd(FindWindowA("Shell_TrayWnd", NULL)),
	m_dwOpacity(nOpac),
	m_dwOpacityAll(nOpacAll),
	m_crAlphaColor(bgColor),
	m_style(ACCENT_STATE::ENABLE_TRANSPARENTGRADIENT)
{
}

BOOL Win32Namespace::TaskbarCtrl::update() const
{
	static LONG_PTR n = ::SetWindowLongPtrA(m_hWnd, GWL_EXSTYLE, ::GetWindowLongPtrA(m_hWnd, GWL_EXSTYLE) | WS_EX_LAYERED);
	BOOL b = WindowComposition::Set(m_hWnd, m_style, (m_dwOpacity << 24) + (m_crAlphaColor & 0xffffff));
	return b && ::SetLayeredWindowAttributes(m_hWnd, 0, m_dwOpacityAll, LWA_ALPHA);
}

void Win32Namespace::TaskbarCtrl::OnTaskbarCreated()
{
	m_hWnd = ::FindWindowA("Shell_TrayWnd", NULL);
}
