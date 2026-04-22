#include "./Window.h"

HamsterStudioToesAppNamespace::Window::ProcedureContext::ProcedureContext(OnPaintProc onpaint)
	: m_OnPaintProc(onpaint) {

}

void HamsterStudioToesAppNamespace::Window::ProcedureContext::OnPaint(Window const& window, PAINTSTRUCT const& ps) const
{
	if (m_OnPaintProc != nullptr) {
		m_OnPaintProc(window, ps);
	}
}

struct HamsterStudioToesAppNamespace::Window::WindowHash {
	size_t operator()(Window const& window) const {
		return reinterpret_cast<size_t>(window.GetHandle());
	}
};

bool HamsterStudioToesAppNamespace::Window::operator==(Window const& right) const
{
	return m_hWnd == right.m_hWnd;
}

HamsterStudioToesAppNamespace::Window::Window(Application& app, int nCmdShow, ProcedureContext const& proc, WindowSize const& size, std::string_view windowName)
	:m_app(app) {

	m_hWnd = ::CreateWindowA(m_app.GetClassNameStr(), windowName.data(), WS_OVERLAPPEDWINDOW,
		size.X, size.Y, size.Width, size.Height, NULL, NULL, app.GetInst(), NULL);

	if (!m_hWnd) {
		return;
	}

	if (!_ProcsMap.try_emplace(*this, proc).second)
	{
		// shall not been reached
		return;
	}

	m_app.RegisterWindowProc(m_hWnd, MainWindowProc);

	::ShowWindow(m_hWnd, nCmdShow);
	::UpdateWindow(m_hWnd);
}

int HamsterStudioToesAppNamespace::Window::RunMessageLoop() const
{
	static MSG msg;
	static int bRet;

	do {
		//bRet = ::PeekMessageA(&msg, NULL, 0, 0, PM_NOREMOVE);
		//if (bRet == 0) { break; }

		bRet = ::GetMessageA(&msg, NULL, 0, 0);
		if (bRet != 0 && bRet != -1) {
			::TranslateMessage(&msg);
			::DispatchMessageA(&msg);
		}
		else {
			break;
		}
	} while (1);
	return static_cast<int>(msg.wParam);
}

inline HamsterStudioToesAppNamespace::WindowSize HamsterStudioToesAppNamespace::Window::GetWindowSize() const
{
	::RECT rc;
	::GetWindowRect(m_hWnd, &rc);
	return WindowSize{ rc.left, rc.top, rc.right - rc.left, rc.bottom - rc.top };
}

inline bool HamsterStudioToesAppNamespace::Window::DobleBuffer(std::function<void(::HDC, int, int)> draw) const
{
	PAINTSTRUCT ps;
	HDC hdc = BeginPaint(m_hWnd, &ps);

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

	return TRUE == ::EndPaint(m_hWnd, &ps);
}

HamsterStudioToesAppNamespace::Window::_ProcsMapTy HamsterStudioToesAppNamespace::Window::_ProcsMap;

LRESULT HamsterStudioToesAppNamespace::Window::MainWindowProc(HWND hHostWnd, UINT message, WPARAM wParam, LPARAM lParam)
{
	auto proc = std::find_if(_ProcsMap.cbegin(), _ProcsMap.cend(), [=](auto const& iter) {
		return iter.first.GetHandle() == hHostWnd;
		});
	if (_ProcsMap.cend() != proc) {
		switch (message) {
		case WM_CLOSE:
			::DestroyWindow(hHostWnd);
			break;
		case WM_DESTROY:
			::PostQuitMessage(0);
			break;
		case WM_PAINT:
		{
			PAINTSTRUCT ps;
			auto _ = ::BeginPaint(hHostWnd, &ps);
			proc->second.OnPaint(proc->first, ps);
			::EndPaint(hHostWnd, &ps);
		}
		break;
		}
	}
	return ::DefWindowProcA(hHostWnd, message, wParam, lParam);
}

int HamsterStudioToesAppNamespace::CreateWindowTestMain()
{
	using namespace HamsterStudioToesAppNamespace;
	Application app{ ::GetModuleHandleA(NULL), NULL, ::GetCommandLineA(), "HamsterStudioToesApp" };
	Window window{ app, SW_SHOW, Window::ProcedureContext(nullptr), DefaultWindowSize, "HamsterStudioToes" };
	return window.RunMessageLoop();
}
