#pragma once

#include <Windows.h>

#include <string_view>
#include <unordered_map>

class Application {
	HINSTANCE m_hInst;
	WNDCLASSEXA m_wndCls;
	int m_nShowCmd;
	LPCSTR lpszWindowClassName;
public:

	explicit Application(Application&&) = delete;
	void operator= (Application&&) = delete;

	explicit Application(Application const&) = delete;
	void operator= (Application const&) = delete;

	explicit Application(HINSTANCE hInst, HINSTANCE hPrevInst, LPCSTR lpCmdLine, int nShowCmd, std::string_view wndClsName)
		: m_hInst(hInst), m_nShowCmd(nShowCmd), m_wndCls(), lpszWindowClassName(wndClsName.data())
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
		RegisterClassExA(&m_wndCls);
	}

	constexpr inline HINSTANCE GetInst() const { return m_hInst; }
	constexpr inline int GetShowCmd() const { return m_nShowCmd; }
	constexpr inline LPCSTR GetClassNameStr() const { return lpszWindowClassName; }

	using WindowProcMapTy = std::unordered_map<HWND, WNDPROC>;
	bool RegisterWindowProc(WindowProcMapTy::key_type key, WindowProcMapTy::mapped_type value) {
		auto rslt = WindowProcMap.try_emplace(key, value);
		return rslt.second;
	}

private:

	static WindowProcMapTy WindowProcMap;
	static LRESULT MainWndProc(HWND hHostWnd, UINT message, WPARAM wParam, LPARAM lParam) {
		for (auto const& kv : WindowProcMap) {
			if (kv.first == hHostWnd) {
				return kv.second(hHostWnd, message, wParam, lParam);
			}
		}
		return DefWindowProcA(hHostWnd, message, wParam, lParam);
	}

};

Application::WindowProcMapTy Application::WindowProcMap;

struct WindowSize {
	long X, Y, Width, Height;
};
constexpr WindowSize DefaultWindowSize = {
	CW_USEDEFAULT, CW_USEDEFAULT, CW_USEDEFAULT, CW_USEDEFAULT
};

class Window {
	Application& m_app;
	HWND m_hWnd;

public:
	class ProcedureContext {
		using OnPaintProc = void(*)(Window const& window, PAINTSTRUCT const& ps);
		OnPaintProc m_OnPaintProc = nullptr;

	public:
		explicit ProcedureContext(OnPaintProc onpaint)
			: m_OnPaintProc(onpaint) {

		}

		void OnPaint(Window const& window, PAINTSTRUCT const& ps) const {
			if (m_OnPaintProc != nullptr) {
				m_OnPaintProc(window, ps);
			}
		}

	};

	struct WindowHash {
		size_t operator()(Window const& window) const {
			return reinterpret_cast<size_t>(window.GetHandle());
		}
	};

	bool operator==(Window const& right) const {
		return m_hWnd == right.m_hWnd;
	}

	explicit Window(Application& app, ProcedureContext const& proc, WindowSize const& size, std::string_view windowName)
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

		::ShowWindow(m_hWnd, app.GetShowCmd());
		::UpdateWindow(m_hWnd);
	}

	int Run() const {
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
		return msg.wParam;
	}

	constexpr inline HWND GetHandle() const { return m_hWnd; }
	inline WindowSize GetSize() const {
		::RECT rc;
		::GetWindowRect(m_hWnd, &rc);
		return WindowSize{ rc.left, rc.top, rc.right - rc.left, rc.bottom - rc.top };
	}

private:
	using _ProcsMapTy = std::unordered_map<Window, ProcedureContext, WindowHash>;
	static _ProcsMapTy _ProcsMap;
	static LRESULT MainWindowProc(HWND hHostWnd, UINT message, WPARAM wParam, LPARAM lParam) {
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

};

Window::_ProcsMapTy Window::_ProcsMap;
