#pragma once

#include <Windows.h>

#include <string_view>
#include <unordered_map>

#define HamsterStudioToesAppNamespace HamsterStudioToes

namespace HamsterStudioToesAppNamespace
{
	class Application {
		HINSTANCE m_hInst;
		WNDCLASSEXA m_wndCls;
		LPCSTR lpszWindowClassName;
	public:

		explicit Application(Application&&) = delete;
		void operator= (Application&&) = delete;

		explicit Application(Application const&) = delete;
		void operator= (Application const&) = delete;

		explicit Application(HINSTANCE hInst, HINSTANCE hPrevInst, LPCSTR lpCmdLine, std::string_view wndClsName);

		constexpr inline HINSTANCE GetInst() const { return m_hInst; }
		constexpr inline LPCSTR GetClassNameStr() const { return lpszWindowClassName; }

		using WindowProcMapTy = std::unordered_map<HWND, WNDPROC>;
		bool RegisterWindowProc(WindowProcMapTy::key_type key, WindowProcMapTy::mapped_type value);

	private:
		static WindowProcMapTy WindowProcMap;
		static LRESULT MainWndProc(HWND hHostWnd, UINT message, WPARAM wParam, LPARAM lParam);
	};

}