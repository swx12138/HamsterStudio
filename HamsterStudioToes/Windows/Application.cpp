#include "./Application.h"

HamsterStudioToesAppNamespace::Application::Application(HINSTANCE hInst, HINSTANCE hPrevInst, LPCSTR lpCmdLine, std::string_view wndClsName)
	: m_hInst(hInst),
	m_wndCls(),
	lpszWindowClassName(wndClsName.data())
{
	m_wndCls.cbSize = sizeof(decltype(m_wndCls));
	m_wndCls.style = CS_HREDRAW |
		CS_VREDRAW;                    // redraw if size changes 
	m_wndCls.lpfnWndProc = MainWndProc;     // points to window procedure 
	m_wndCls.cbClsExtra = 0;                // no extra class memory 
	m_wndCls.cbWndExtra = 0;                // no extra window memory 
	m_wndCls.hInstance = hInst;         // handle to instance 
	m_wndCls.hIcon = LoadIcon(NULL,
		IDI_APPLICATION);              // predefined app. icon 
	m_wndCls.hCursor = LoadCursor(NULL,
		IDC_ARROW);                    // predefined arrow 
	m_wndCls.hbrBackground = (HBRUSH)GetStockObject(WHITE_BRUSH);                  // white background brush 
	m_wndCls.lpszClassName = lpszWindowClassName;  // name of window class 

	// Register the window class. 
	::RegisterClassExA(&m_wndCls);
}

bool HamsterStudioToesAppNamespace::Application::RegisterWindowProc(WindowProcMapTy::key_type key, WindowProcMapTy::mapped_type value)
{
	auto rslt = WindowProcMap.try_emplace(key, value);
	return rslt.second;
}

HamsterStudioToesAppNamespace::Application::WindowProcMapTy HamsterStudioToesAppNamespace::Application::WindowProcMap;

LRESULT HamsterStudioToesAppNamespace::Application::MainWndProc(HWND hHostWnd, UINT message, WPARAM wParam, LPARAM lParam)
{
	for (auto const& kv : WindowProcMap) {
		if (kv.first == hHostWnd) {
			return kv.second(hHostWnd, message, wParam, lParam);
		}
	}
	return DefWindowProcA(hHostWnd, message, wParam, lParam);
}
