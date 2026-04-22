#pragma once

#include "./Application.h"

#include <functional>

namespace HamsterStudioToesAppNamespace
{
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
			explicit ProcedureContext(OnPaintProc onpaint);

			void OnPaint(Window const& window, PAINTSTRUCT const& ps) const;

		};

		struct WindowHash;

		bool operator==(Window const& right) const;

		explicit Window(Application& app, _In_ int nCmdShow, ProcedureContext const& proc, WindowSize const& size, std::string_view windowName);

		int RunMessageLoop() const;

		constexpr inline HWND GetHandle() const { return m_hWnd; }
		inline WindowSize GetWindowSize() const;

		inline bool DobleBuffer(std::function<void(::HDC, int, int)> draw) const;

	private:
		using _ProcsMapTy = std::unordered_map<Window, ProcedureContext, WindowHash>;
		static _ProcsMapTy _ProcsMap;
		static LRESULT MainWindowProc(HWND hHostWnd, UINT message, WPARAM wParam, LPARAM lParam);

	};

}