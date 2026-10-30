#pragma once

#include <stdexcept>
#include <string>
#include <vector>

namespace toes::diagnostics
{
	class AssertException : public std::runtime_error
	{
	public:
		explicit AssertException(const std::string &message);
	};

	void Assert(bool condition, std::string const &message = "");

	/// <summary>
	/// 读取 GetLastError() 并在需要时抛出 std::system_error。
	/// toleratedCodes 中的错误码视为正常收尾，直接返回。
	/// 本函数不会终止进程——库代码没有权力替宿主决定生死。
	/// </summary>
	void ThrowLastError(std::vector<int> const &toleratedCodes = {});

	class Trace {
	public:
		static void TraceInfo(std::string const &message);
		static void TraceInfo(std::wstring const &message);

		static void TraceWarning(std::string const &message);
		static void TraceWarning(std::wstring const &message);
		static void TraceWarning(std::exception const &ex);

		static void TraceError(std::string const &message);
		static void TraceError(std::wstring const &message);
		static void TraceError(std::exception const &ex);

	private:
		static void OutputToDebugConsole(std::string const &message);
		static void OutputToDebugConsole(std::wstring const &message);
	};
}