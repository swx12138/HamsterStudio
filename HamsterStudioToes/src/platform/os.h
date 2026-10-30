#pragma once

#include "platform/shell_api.h"

#include <Windows.h>

#include <string>
#include <vector>
#include <memory>
#include <functional>
#include <concepts>
#include <filesystem>

namespace toes::win32
{
	double GetWindowsZoom();

	bool PathCompare(const std::filesystem::path &a, const std::filesystem::path &b);

	size_t GetMemoryUsage(HANDLE hProcess = NULL);

	std::string GetCurrentWallpaperPath();
	bool SetWallpaper(std::string const &path);
	HWND FindWallpaperLayer(HWND *theDesktopWorkerW = nullptr);

	void EnumHardLinkFiles(const std::filesystem::path& file_path, std::function<bool(std::filesystem::path const&)>&& pred);

	// 如果程序已经有一个在运行，则返回true
	bool IsInstanceExist(std::string_view const _uuid);

	using sheet_t = struct
	{
		std::string title;
		std::vector<std::pair<std::string, std::string>> body;
		std::string tail;
	};

	long DrawSheet(HDC hdc, sheet_t const& sht, const ::POINT& start, long h, long wl, long wr);

	enum class ProceserArchitecture
	{
		X64 = 9,
		ARM = 5,
		ARM64 = 12,
		IntelItanium = 6,
		x86 = 0,
		Unknown = 0xffff
	};

	class SystemInfo
	{
	public:
		SystemInfo();
		~SystemInfo() { }

		static std::shared_ptr<SystemInfo> GetInst();
		// static bool IsWow64();

		int PageSize() const { return _PageSize; }
		uint64_t MinApplAddr() { return _MinApplAddr; }
		uint64_t MaxApplAddr() { return _MaxApplAddr; }
		int CpuCount() { return _CpuCount; }

	private:
		int _PageSize;
		uint64_t _MinApplAddr, _MaxApplAddr;
		int _CpuCount;
	};

	// 基础消息处理器接口
	class IMessageHandler {
	public:
		virtual ~IMessageHandler() = default;
		virtual LRESULT HandleMessage(HWND hWnd, UINT msg,
			WPARAM wParam, LPARAM lParam) = 0;
		virtual bool CanHandleMessage(UINT msg) const = 0;
	};

	// 可组合的消息处理系统
	class ComposableWindowsProcedure {
	private:
		std::vector<std::shared_ptr<IMessageHandler>> handlers_;
		std::function<void()> onUnhandled_;

	public:
		template<typename Handler>
		void AddHandler(std::shared_ptr<Handler> handler) {
			handlers_.push_back(std::move(handler));
		}

		template<typename Callable>
		void SetUnhandledCallback(Callable &&callback) {
			onUnhandled_ = std::forward<Callable>(callback);
		}

		LRESULT ProcessMessage(HWND hWnd, UINT msg,
			WPARAM wParam, LPARAM lParam) {
			for (const auto &handler : handlers_) {
				if (handler->CanHandleMessage(msg)) {
					return handler->HandleMessage(hWnd, msg, wParam, lParam);
				}
			}

			if (onUnhandled_) {
				onUnhandled_();
			}

			return DefWindowProc(hWnd, msg, wParam, lParam);
		}
	};

}